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

    void Awake()
    {
        if (floorUnlockText != null)
            floorUnlockText.gameObject.SetActive(false);

        if (healthBar != null)
        {
            healthBar.fillAmount = 1f;
            healthBar.color = Color.green;
        }

        if (ammoBar != null)
        {
            ammoBar.fillAmount = 1f;
            ammoBar.color = normalAmmoColor;
        }
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

        if (ammoText != null)
        {
            ammoText.text = currentAmmo.ToString();
            ammoText.color = (currentAmmo <= lowAmmoThreshold && !isReloading) ? lowAmmoColor : normalAmmoColor;
        }

        if (maxAmmoText != null)
        {
            maxAmmoText.text = "/ " + maxAmmo.ToString();
        }

        if (ammoBar != null)
        {
            ammoBar.fillAmount = (float)currentAmmo / maxAmmo;
            ammoBar.color = (currentAmmo <= lowAmmoThreshold && !isReloading) ? lowAmmoColor : normalAmmoColor;
        }

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
            float fillAmount = currentHealth / maxHealth;
            healthBar.fillAmount = fillAmount;

            if (fillAmount > 0.6f)
                healthBar.color = Color.green;
            else if (fillAmount > 0.3f)
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
        if (killCountText != null)
        {
            killCountText.text = currentKills.ToString();
        }
    }

    public int GetKillCount() => currentKills;

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