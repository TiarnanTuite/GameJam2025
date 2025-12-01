using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float stoppingDistance = 2f;

    [Header("Combat")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int damage = 10;
    [SerializeField] private float attackCooldown = 1.5f;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 20f;

    [Header("References")]
    [SerializeField] private Transform player;

    private int currentHealth;
    private float lastAttackTime;
    private bool isDead = false;
    private PlayerHealth playerHealth;
    private Rigidbody rb;

    void Start()
    {
        // Get or add Rigidbody
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        // Configure Rigidbody to prevent physics knockback
        rb.mass = 100f; // Heavy mass prevents easy knockback
        rb.linearDamping = 5f; // High drag stops sliding
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ; // Only allow Y rotation
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth == null)
                playerHealth = player.GetComponentInParent<PlayerHealth>();
            if (playerHealth == null)
                playerHealth = player.GetComponentInChildren<PlayerHealth>();
            if (playerHealth == null)
                playerHealth = FindFirstObjectByType<PlayerHealth>();
        }

        currentHealth = maxHealth;
        Debug.Log($"Enemy spawned with {maxHealth} health");
    }

    void Update()
    {
        if (isDead || player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            FollowPlayer(distanceToPlayer);
        }
    }

    void FollowPlayer(float distance)
    {
        if (distance > stoppingDistance)
        {
            Vector3 direction = (player.position - transform.position).normalized;

            // Use Rigidbody.MovePosition for physics-based movement that won't be affected by bullet impacts
            Vector3 newPosition = rb.position + direction * moveSpeed * Time.deltaTime;
            rb.MovePosition(newPosition);

            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            TryAttack();
        }
    }

    void TryAttack()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            AttackPlayer();
            lastAttackTime = Time.time;
        }
    }

    void AttackPlayer()
    {
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        Debug.Log($"[Enemy] Took {damageAmount} damage. Health: {currentHealth}/{maxHealth}");

        // Optional: Add small visual feedback without physics force
        StartCoroutine(DamageFlash());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    System.Collections.IEnumerator DamageFlash()
    {
        // Optional: Flash red or play hit animation
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        Color originalColor = Color.white;

        if (renderers.Length > 0 && renderers[0].material.HasProperty("_Color"))
        {
            originalColor = renderers[0].material.color;

            foreach (Renderer rend in renderers)
            {
                rend.material.color = Color.red;
            }

            yield return new WaitForSeconds(0.1f);

            foreach (Renderer rend in renderers)
            {
                rend.material.color = originalColor;
            }
        }
    }

    void Die()
    {
        isDead = true;
        Debug.Log("[Enemy] DIE() CALLED");

        HUDController hud = FindFirstObjectByType<HUDController>();
        if (hud != null)
        {
            hud.AddKill();
            Debug.Log("[Enemy] HUD notified");
        }
        else
        {
            Debug.LogError("[Enemy] HUD NOT FOUND!");
        }

        WaveSpawner spawner = FindFirstObjectByType<WaveSpawner>();
        if (spawner != null)
        {
            spawner.OnEnemyKilled();
            Debug.Log("[Enemy] WaveSpawner notified");
        }
        else
        {
            Debug.LogError("[Enemy] WaveSpawner NOT FOUND!");
        }

        FloorManager floorManager = FindFirstObjectByType<FloorManager>();
        if (floorManager != null)
        {
            floorManager.OnEnemyKilled();
            Debug.Log("[Enemy] FloorManager notified");
        }
        else
        {
            Debug.LogError("[Enemy] FloorManager NOT FOUND!");
        }

        Destroy(gameObject, 0.1f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            Die();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stoppingDistance);
    }
}