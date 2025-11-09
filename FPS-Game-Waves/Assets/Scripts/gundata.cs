using UnityEngine;

[CreateAssetMenu(fileName = "newgundata", menuName = "Gun/GunData")]
public class GunData  : ScriptableObject
{
    [Header("Stats")]
    public int damage = 25;
    public float range = 100f;
    public float fireRate = 10f;
    public int maxAmmo = 30;
    public float reloadTime = 1.5f;

    [Header("Recoil")]
    public float recoilAmount = 0.5f;
    public float recoilRecoverySpeed = 5f;

    [Header("Effects")]
    public ParticleSystem muzzleFlash;
    public AudioClip shootSound;
    public AudioClip reloadSound;
    public AudioClip emptySound;

    [Header("Projectile (optional)")]
    public GameObject projectilePrefab;        // drag your bullet/projectile prefab here
    public float projectileSpeed = 50f;        // velocity applied to Rigidbody
    public float projectileLifetime = 5f;      // auto-destroy after this many seconds
}