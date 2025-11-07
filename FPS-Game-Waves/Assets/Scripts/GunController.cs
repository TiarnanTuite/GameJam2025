using UnityEngine;
using UnityEngine.InputSystem;

public class GunController : MonoBehaviour
{
    [Header("Gun Settings")]
    public float range = 100f;
    public float fireRate = 15f; // Rounds per second
    public int maxAmmo = 30;
    public float reloadTime = 1.5f;

    [Header("Effects")]
    public ParticleSystem muzzleFlash;
    public GameObject impactEffect;

    [Header("Recoil")]
    public float recoilAmount = 0.5f;
    public float recoilRecoverySpeed = 5f;

    [Header("References")]
    public Camera fpsCam;
    public Transform gunEnd; // Tip of barrel (optional)

    [Header("Audio")]
    public AudioClip shootSound;
    public AudioClip reloadSound;
    public AudioClip emptySound;

    private AudioSource audioSource;
    private int currentAmmo;
    private float nextTimeToFire = 0f;
    private bool isReloading = false;
    private Vector3 originalGunPosition;

    void Start()
    {
        currentAmmo = maxAmmo;
        originalGunPosition = transform.localPosition;

        if (fpsCam == null)
            fpsCam = Camera.main;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound
    }

    void Update()
    {
        // Recover from recoil
        transform.localPosition = Vector3.Lerp(transform.localPosition, originalGunPosition, Time.deltaTime * recoilRecoverySpeed);

        if (isReloading)
            return;

        // Shooting input
        var mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.isPressed && Time.time >= nextTimeToFire)
        {
            if (currentAmmo > 0)
            {
                nextTimeToFire = Time.time + 1f / fireRate;
                Shoot();
            }
            else
            {
                if (emptySound != null)
                    audioSource.PlayOneShot(emptySound);
                StartReload();
            }
        }

        // Reload input
        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.rKey.wasPressedThisFrame && currentAmmo < maxAmmo && !isReloading)
        {
            StartReload();
        }
    }

    void Shoot()
    {
        currentAmmo--;
        Debug.Log($"Shot fired! Ammo remaining: {currentAmmo}/{maxAmmo}");

        // Play effects
        if (shootSound != null)
            audioSource.PlayOneShot(shootSound);

        if (muzzleFlash != null)
            muzzleFlash.Play();

        // Apply recoil
        transform.localPosition += new Vector3(0, 0, -recoilAmount * 0.1f);

        // Raycast for hit detection
        Vector3 shootOrigin = fpsCam.transform.position;
        Vector3 shootDirection = fpsCam.transform.forward;

        // Use gun end position if available
        if (gunEnd != null)
        {
            shootOrigin = gunEnd.position;
            shootDirection = (fpsCam.transform.position + fpsCam.transform.forward * range - gunEnd.position).normalized;
        }

        RaycastHit hit;
        if (Physics.Raycast(shootOrigin, shootDirection, out hit, range))
        {
            Debug.Log($"Hit: {hit.transform.name} at distance {hit.distance}");

            // Spawn impact effect
            if (impactEffect != null)
            {
                GameObject impact = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impact, 2f);
            }
        }

        // Debug visualization
        Debug.DrawRay(shootOrigin, shootDirection * range, Color.red, 1f);
    }

    void StartReload()
    {
        if (isReloading || currentAmmo == maxAmmo)
            return;

        isReloading = true;
        Debug.Log("Reloading...");

        if (reloadSound != null)
            audioSource.PlayOneShot(reloadSound);

        Invoke(nameof(FinishReload), reloadTime);
    }

    void FinishReload()
    {
        currentAmmo = maxAmmo;
        isReloading = false;
        Debug.Log($"Reload complete! Ammo: {currentAmmo}/{maxAmmo}");
    }

    // Public getters for UI
    public int GetCurrentAmmo() => currentAmmo;
    public int GetMaxAmmo() => maxAmmo;
    public bool IsReloading() => isReloading;
}