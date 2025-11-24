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

    [Header("Kill Counter")]
    public TextMeshProUGUI killCountText;
    public TextMeshProUGUI floorUnlockText;

    [Header("Colors")]
    public Color normalAmmoColor = Color.cyan;
    public Color lowAmmoColor = Color.red;
    public int lowAmmoThreshold = 5;

    private int currentKills = 0;

    void Start()
    {
        if (floorUnlockText != null)
            floorUnlockText.gameObject.SetActive(false);
    }

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

            Debug.Log($"Ammo Bar Fill: {fillAmount} ({currentAmmo}/{maxAmmo})");

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
        else
        {
            Debug.LogWarning("Ammo Bar is not assigned in HUDController!");
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

    public void AddKill()
    {
        currentKills++;
        UpdateKillCount();
    }

    void UpdateKillCount()
    {
        if (killCountText != null)
        {
            killCountText.text = currentKills.ToString();
        }
    }

    public int GetKillCount()
    {
        return currentKills;
    }

    public void ShowFloorUnlock(string message)
    {
        if (floorUnlockText != null)
        {
            floorUnlockText.text = message;
            floorUnlockText.gameObject.SetActive(true);
            Invoke(nameof(HideFloorUnlock), 3f);
        }
    }

    void HideFloorUnlock()
    {
        if (floorUnlockText != null)
        {
            floorUnlockText.gameObject.SetActive(false);
        }
    }
}