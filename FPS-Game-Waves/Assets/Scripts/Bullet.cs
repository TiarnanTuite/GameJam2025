using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private int damage = 25;
    [SerializeField] private GameObject impactEffect;
    [SerializeField] private float bulletSpeed = 50f; // Add speed for reference

    private bool hasHit = false; // Prevent multiple hits

    void Start()
    {
        // Ignore collisions with player layer
        int playerLayer = LayerMask.NameToLayer("Player");
        int bulletLayer = gameObject.layer;

        if (playerLayer != -1 && bulletLayer != -1)
        {
            Physics.IgnoreLayerCollision(bulletLayer, playerLayer, true);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Prevent multiple hits from same bullet
        if (hasHit) return;
        hasHit = true;

        Debug.Log($"[Bullet] Hit: {collision.gameObject.name}, Tag: {collision.gameObject.tag}, Layer: {LayerMask.LayerToName(collision.gameObject.layer)}");

        // Double-check we're not hitting player
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            Debug.Log("[Bullet] Hit player, ignoring...");
            Destroy(gameObject);
            return;
        }

        // Try to get Enemy component from what we hit OR its parent
        Enemy enemy = collision.gameObject.GetComponent<Enemy>();
        if (enemy == null)
        {
            enemy = collision.gameObject.GetComponentInParent<Enemy>();
        }

        if (enemy != null)
        {
            Debug.Log($"[Bullet] Found Enemy component! Dealing {damage} damage");
            enemy.TakeDamage(damage);
        }
        else
        {
            Debug.Log($"[Bullet] No Enemy component found on {collision.gameObject.name}");
        }

        // Spawn impact effect
        if (impactEffect != null && collision.contacts.Length > 0)
        {
            ContactPoint contact = collision.contacts[0];
            GameObject impact = Instantiate(impactEffect, contact.point, Quaternion.LookRotation(contact.normal));
            Destroy(impact, 2f);
        }

        Destroy(gameObject);
    }
}