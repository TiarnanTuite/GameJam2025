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

    void Start()
    {
        // Find player by tag if not assigned in inspector
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("Player not found! Make sure player has 'Player' tag or assign in inspector.");
            }
        }

        currentHealth = maxHealth;
    }

    void Update()
    {
        if (isDead || player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Only follow if player is within detection range
        if (distanceToPlayer <= detectionRange)
        {
            FollowPlayer(distanceToPlayer);
        }
    }

    void FollowPlayer(float distance)
    {
        // Move towards player if outside stopping distance
        if (distance > stoppingDistance)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;

            // Rotate to face player
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            // Attack player when in range
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
        // Add attack animation trigger here if you have animations
        Debug.Log("Enemy attacked player!");
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        Debug.Log($"Enemy took {damageAmount} damage. Health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        Debug.Log("Enemy died!");

        // Notify HUD about kill
        HUDController hud = FindObjectOfType<HUDController>();
        if (hud != null)
        {
            hud.AddKill();
        }

        // Notify wave spawner
        WaveSpawner spawner = FindObjectOfType<WaveSpawner>();
        if (spawner != null)
        {
            spawner.OnEnemyKilled();
        }

        // Notify floor manager
        FloorManager floorManager = FindObjectOfType<FloorManager>();
        if (floorManager != null)
        {
            floorManager.OnEnemyKilled();
        }

        // Add death effects, animations, etc. here
        Destroy(gameObject, 0.1f);
    }

    // Visualize detection range in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stoppingDistance);
    }
}