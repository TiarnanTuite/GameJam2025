using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
// [CreateAssetMenu (fileName = "newgundata", menuName = "Gun/GunController")]

public class GunController : MonoBehaviour
{
    public GunData  gunData; // assign created GunData asset in Inspector

    // add these scene references so the compiler knows them and you can assign them in Inspector
    public Camera fpsCam;
    public Transform gunEnd;
    public GameObject impactEffect;

    // remove the duplicated fields moved to GunData
    private AudioSource audioSource;
    private int currentAmmo;
    private float nextTimeToFire = 0f;
    private bool isReloading = false;
    private Vector3 originalGunPosition;

    void Start()
    {
        if (gunData == null)
        {
            Debug.LogError("[GunController] gunData is not assigned. Create a GunData asset and assign it in the Inspector.");
            // fallback defaults to avoid further NREs
            currentAmmo = 30;
            originalGunPosition = transform.localPosition;
            if (fpsCam == null) fpsCam = Camera.main;
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            return;
        }

        currentAmmo = gunData.maxAmmo;
        originalGunPosition = transform.localPosition;

        if (fpsCam == null)
            fpsCam = Camera.main;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound
    }

    void Update()
    {
        if (gunData == null)
            return; // already logged in Start

        // Recover from recoil
        transform.localPosition = Vector3.Lerp(transform.localPosition, originalGunPosition, Time.deltaTime * gunData.recoilRecoverySpeed);

        if (isReloading)
            return;

        // Shooting input
        var mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.isPressed && Time.time >= nextTimeToFire)
        {
            if (currentAmmo > 0)
            {
                nextTimeToFire = Time.time + 1f / gunData.fireRate;
                Shoot();
            }
            else
            {
                if (gunData.emptySound != null)
                    audioSource.PlayOneShot(gunData.emptySound);
                StartReload();
            }
        }

        // Reload input
        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.rKey.wasPressedThisFrame && currentAmmo < gunData.maxAmmo && !isReloading)
        {
            StartReload();
        }
    }

    void Shoot()
    {
        currentAmmo--;
        Debug.Log($"Shot fired! Ammo remaining: {currentAmmo}/{gunData.maxAmmo}");

        // Play effects
        if (gunData.shootSound != null)
            audioSource.PlayOneShot(gunData.shootSound);

        if (gunData.muzzleFlash != null)
            gunData.muzzleFlash.Play();

        // Apply recoil
        transform.localPosition += new Vector3(0, 0, -gunData.recoilAmount * 0.1f);

        // Determine spawn origin and direction
        Vector3 shootOrigin = fpsCam != null ? fpsCam.transform.position : transform.position;
        Vector3 shootDirection = fpsCam != null ? fpsCam.transform.forward : transform.forward;

        if (gunEnd != null)
        {
            shootOrigin = gunEnd.position;
            if (fpsCam != null)
                shootDirection = (fpsCam.transform.position + fpsCam.transform.forward * gunData.range - gunEnd.position).normalized;
        }

        // If a projectile prefab is assigned, instantiate it and give it velocity.
        if (gunData.projectilePrefab != null)
        {
            // Shoot from camera position in camera direction for accuracy
            Vector3 bulletSpawnPos = fpsCam != null ? fpsCam.transform.position : shootOrigin;
            Vector3 bulletDirection = fpsCam != null ? fpsCam.transform.forward : shootDirection;
            
            // Offset spawn position slightly forward to avoid hitting player
            bulletSpawnPos += bulletDirection * 0.5f;
            
            GameObject proj = Instantiate(gunData.projectilePrefab, bulletSpawnPos, Quaternion.LookRotation(bulletDirection));
            Rigidbody rb = proj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = bulletDirection * gunData.projectileSpeed;
            }
            else
            {
                // if no Rigidbody present, try adding one (optional)
                rb = proj.AddComponent<Rigidbody>();
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                rb.useGravity = false;
                rb.linearVelocity = bulletDirection * gunData.projectileSpeed;
            }

            Destroy(proj, gunData.projectileLifetime);
        }
        else
        {
            // Fallback to raycast hit detection if no projectile prefab provided
            RaycastHit hit;
            if (Physics.Raycast(shootOrigin, shootDirection, out hit, gunData.range))
            {
                Debug.Log($"Hit: {hit.transform.name} at distance {hit.distance}");

                // DAMAGE ENEMY IF HIT
                Enemy enemy = hit.collider.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(gunData.damage);
                    Debug.Log($"Damaged enemy for {gunData.damage} damage!");
                }

                // Spawn impact effect
                if (impactEffect != null)
                {
                    GameObject impact = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                    Destroy(impact, 2f);
                }
            }

            // Debug visualization
            Debug.DrawRay(shootOrigin, shootDirection * gunData.range, Color.red, 1f);
        }
    }

    void StartReload()
    {
        if (isReloading || currentAmmo == gunData.maxAmmo)
            return;

        isReloading = true;
        Debug.Log("Reloading...");

        if (gunData.reloadSound != null)
            audioSource.PlayOneShot(gunData.reloadSound);

        Invoke(nameof(FinishReload), gunData.reloadTime);
    }

    void FinishReload()
    {
        currentAmmo = gunData.maxAmmo;
        isReloading = false;
        Debug.Log($"Reload complete! Ammo: {currentAmmo}/{gunData.maxAmmo}");
    }

    // Public getters for UI
    public int GetCurrentAmmo() => currentAmmo;
    public int GetMaxAmmo() => gunData.maxAmmo;
    public bool IsReloading() => isReloading;
}