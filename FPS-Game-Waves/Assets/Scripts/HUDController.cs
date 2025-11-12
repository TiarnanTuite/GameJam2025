using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDController : MonoBehaviour
{
    [Header("References")]
    public GunController gunController;

    [Header("Ammo UI")]
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI maxAmmoText;
    public Image ammoBar;
    public GameObject reloadingPanel;
    public TextMeshProUGUI reloadingText;

    [Header("Health UI")]
    public TextMeshProUGUI healthText;
    public Image healthBar;

    [Header("Weapon UI")]
    public TextMeshProUGUI weaponNameText;

    [Header("Colors")]
    public Color normalAmmoColor = Color.cyan;
    public Color lowAmmoColor = Color.red;
    public int lowAmmoThreshold = 5;

    void Update()
    {
        if (gunController != null)
        {
            UpdateAmmo();
        }
    }

    void UpdateAmmo()
    {
        int currentAmmo = gunController.GetCurrentAmmo();
        int maxAmmo = gunController.GetMaxAmmo();
        bool isReloading = gunController.IsReloading();

        // Update ammo text
        if (ammoText != null)
        {
            ammoText.text = currentAmmo.ToString();

            // Change color if low ammo
            if (currentAmmo <= lowAmmoThreshold && !isReloading)
            {
                ammoText.color = lowAmmoColor;
            }
            else
            {
                ammoText.color = normalAmmoColor;
            }
        }

        if (maxAmmoText != null)
        {
            maxAmmoText.text = "/ " + maxAmmo.ToString();
        }

        // Update ammo bar
        if (ammoBar != null)
        {
            float fillAmount = (float)currentAmmo / maxAmmo;
            ammoBar.fillAmount = fillAmount;

            // Change bar color if low
            if (currentAmmo <= lowAmmoThreshold && !isReloading)
            {
                ammoBar.color = lowAmmoColor;
            }
            else
            {
                ammoBar.color = normalAmmoColor;
            }
        }

        // Show/hide reloading indicator
        if (reloadingPanel != null)
        {
            reloadingPanel.SetActive(isReloading);
        }
    }

    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        if (healthText != null)
        {
            healthText.text = Mathf.RoundToInt(currentHealth).ToString();
        }

        if (healthBar != null)
        {
            healthBar.fillAmount = currentHealth / maxHealth;

            // Change color based on health
            if (currentHealth > 60)
                healthBar.color = Color.green;
            else if (currentHealth > 30)
                healthBar.color = Color.yellow;
            else
                healthBar.color = Color.red;
        }
    }

    public void SetWeaponName(string weaponName)
    {
        if (weaponNameText != null)
        {
            weaponNameText.text = weaponName;
        }
    }
}
