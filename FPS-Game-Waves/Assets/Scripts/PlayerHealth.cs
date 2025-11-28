using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    [Header("References")]
    [SerializeField] private HUDController hudController;

    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;

        if (hudController == null)
        {
            hudController = FindFirstObjectByType<HUDController>();
        }

        UpdateHealthUI();
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        if (hudController != null)
        {
            hudController.UpdateHealth(currentHealth, maxHealth);
        }
    }

    void Die()
    {
        isDead = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ShowDeathScreen();
        }
    }

    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public bool IsDead() => isDead;
    public float GetHealthPercentage() => currentHealth / maxHealth;
}