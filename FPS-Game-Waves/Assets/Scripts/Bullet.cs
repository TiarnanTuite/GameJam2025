using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private int damage = 25;
    [SerializeField] private GameObject impactEffect;
    
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"[Bullet] Hit: {collision.gameObject.name}");
        
        // Ignore player collisions
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("[Bullet] Hit player, ignoring...");
            return;
        }
        
        // Check if we hit an enemy
        Enemy enemy = collision.gameObject.GetComponent<Enemy>();
        if (enemy != null)
        {
            Debug.Log($"[Bullet] Damaging enemy for {damage} damage!");
            enemy.TakeDamage(damage);
        }
        
        // Spawn impact effect at hit point
        if (impactEffect != null)
        {
            ContactPoint contact = collision.contacts[0];
            GameObject impact = Instantiate(impactEffect, contact.point, Quaternion.LookRotation(contact.normal));
            Destroy(impact, 2f);
        }
        
        // Destroy bullet
        Destroy(gameObject);
    }
}