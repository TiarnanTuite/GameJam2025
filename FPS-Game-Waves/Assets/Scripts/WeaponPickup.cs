using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponManager : MonoBehaviour
{
    [Header("References")]
    public Camera fpsCam;
    public Transform weaponHolder; // Parent object for weapons (MainCamera)
    public HUDController hudController;

    [Header("Settings")]
    public float pickupRange = 3f;

    [Header("Current Weapon")]
    public GameObject currentWeaponObject;
    public GunController currentGunController;

    [Header("UI")]
    public GameObject pickupPrompt; // "Press E to pickup" text

    private WeaponPickup nearbyWeapon;

    void Start()
    {
        if (fpsCam == null)
            fpsCam = Camera.main;

        if (weaponHolder == null)
            weaponHolder = fpsCam.transform;

        if (pickupPrompt != null)
            pickupPrompt.SetActive(false);
    }

    void Update()
    {
        CheckForWeaponPickup();
        HandleDropWeapon();
    }

    void CheckForWeaponPickup()
    {
        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, pickupRange))
        {
            WeaponPickup weapon = hit.collider.GetComponent<WeaponPickup>();

            if (weapon != null)
            {
                nearbyWeapon = weapon;

                if (pickupPrompt != null)
                    pickupPrompt.SetActive(true);

                // Press E to pickup
                var keyboard = Keyboard.current;
                if (keyboard != null && keyboard.eKey.wasPressedThisFrame)
                {
                    PickupWeapon(weapon);
                }
            }
            else
            {
                nearbyWeapon = null;
                if (pickupPrompt != null)
                    pickupPrompt.SetActive(false);
            }
        }
        else
        {
            nearbyWeapon = null;
            if (pickupPrompt != null)
                pickupPrompt.SetActive(false);
        }
    }

    void HandleDropWeapon()
    {
        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.gKey.wasPressedThisFrame && currentWeaponObject != null)
        {
            DropWeapon();
        }
    }

    void PickupWeapon(WeaponPickup weaponPickup)
    {
        // Drop current weapon if we have one
        if (currentWeaponObject != null)
        {
            DropWeapon();
        }

        // Instantiate the weapon model from GunData
        GameObject newWeapon = Instantiate(weaponPickup.gunData.muzzleFlash.gameObject.transform.parent.gameObject, weaponHolder);
        newWeapon.transform.localPosition = weaponPickup.holdPosition;
        newWeapon.transform.localRotation = Quaternion.Euler(weaponPickup.holdRotation);

        // Add or get GunController
        GunController gunController = newWeapon.GetComponent<GunController>();
        if (gunController == null)
        {
            gunController = newWeapon.AddComponent<GunController>();
        }

        // Set gun data
        gunController.gunData = weaponPickup.gunData;
        gunController.fpsCam = fpsCam;

        // Setup current weapon
        currentWeaponObject = newWeapon;
        currentGunController = gunController;

        // Update HUD
        if (hudController != null)
        {
            hudController.gunController = currentGunController;
            hudController.SetWeaponName(weaponPickup.weaponName);
        }

        // Destroy the pickup object
        Destroy(weaponPickup.gameObject);

        if (pickupPrompt != null)
            pickupPrompt.SetActive(false);

        Debug.Log("Picked up: " + weaponPickup.weaponName);
    }

    void DropWeapon()
    {
        if (currentWeaponObject == null)
            return;

        Destroy(currentWeaponObject);
        currentWeaponObject = null;
        currentGunController = null;

        if (hudController != null)
        {
            hudController.gunController = null;
            hudController.SetWeaponName("NO WEAPON");
        }

        Debug.Log("Dropped weapon");
    }

    public bool HasWeapon()
    {
        return currentWeaponObject != null;
    }
}

public class WeaponPickup : MonoBehaviour
{
    [Header("Weapon Configuration")]
    public GunData gunData;
    public string weaponName = "Assault Rifle";

    [Header("Hold Position")]
    public Vector3 holdPosition = new Vector3(0.5f, -0.3f, 0.5f);
    public Vector3 holdRotation = Vector3.zero;

    [Header("Weapon Tier")]
    public WeaponTier tier = WeaponTier.Basic;

    [Header("Pickup Visuals")]
    public float rotationSpeed = 50f;
    public float bobSpeed = 1f;
    public float bobAmount = 0.3f;

    private Vector3 startPosition;
    private float bobTimer = 0f;
    private Renderer pickupRenderer;

    void Start()
    {
        startPosition = transform.position;
        pickupRenderer = GetComponent<Renderer>();

        // Add a collider if it doesn't have one
        if (GetComponent<Collider>() == null)
        {
            BoxCollider col = gameObject.AddComponent<BoxCollider>();
            col.isTrigger = false; // Set to false so raycast detects it
        }

        // Apply tier color to pickup
        if (pickupRenderer != null)
        {
            Material mat = pickupRenderer.material;
            mat.color = GetTierColor();
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", GetTierColor() * 0.5f);
            }
        }
    }

    void Update()
    {
        // Rotate slowly
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // Bob up and down
        bobTimer += Time.deltaTime * bobSpeed;
        Vector3 bobOffset = new Vector3(0, Mathf.Sin(bobTimer) * bobAmount, 0);
        transform.position = startPosition + bobOffset;
    }

    Color GetTierColor()
    {
        switch (tier)
        {
            case WeaponTier.Basic: return Color.white;
            case WeaponTier.Improved: return Color.green;
            case WeaponTier.Advanced: return Color.blue;
            case WeaponTier.Elite: return new Color(1f, 0.5f, 0f); // Orange
            default: return Color.white;
        }
    }
}

public enum WeaponTier
{
    Basic,      // Starting weapons
    Improved,   // Floor 2
    Advanced,   // Floor 3
    Elite       // Rare/special
}