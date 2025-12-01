using UnityEngine;
using System.Collections.Generic;

public class FloorManager : MonoBehaviour
{
    [System.Serializable]
    public class Floor
    {
        public string floorName;
        public int killsRequired;
        public GameObject blockingObject; // Can be cage+walls OR floorboards
        public List<WeaponPickup> weaponPickups = new List<WeaponPickup>();

        [HideInInspector] public bool isUnlocked = false;
    }

    [Header("References")]
    public HUDController hudController;

    [Header("Floors")]
    public List<Floor> floors = new List<Floor>();

    [Header("Debug Info")]
    [SerializeField] private bool showDebugLogs = true;

    private int totalKills = 0;

    void Start()
    {
        Debug.Log("=== FLOOR MANAGER START ===");

        // Validate floor setup
        ValidateFloorSetup();

        // Floor 0 always unlocked
        if (floors.Count > 0)
        {
            floors[0].isUnlocked = true;
            Debug.Log($"[FloorManager] Floor 0 ({floors[0].floorName}) is unlocked by default");
        }

        // Lock other floors
        for (int i = 1; i < floors.Count; i++)
        {
            floors[i].isUnlocked = false; // Make sure it's locked

            if (floors[i].blockingObject != null)
            {
                floors[i].blockingObject.SetActive(true);
                Debug.Log($"[FloorManager] Floor {i} ({floors[i].floorName}) LOCKED - requires {floors[i].killsRequired} kills - Blocking: {floors[i].blockingObject.name}");
            }
            else
            {
                Debug.LogError($"[FloorManager] ⚠️ Floor {i} ({floors[i].floorName}) has NO blocking object assigned!");
            }

            foreach (WeaponPickup weapon in floors[i].weaponPickups)
            {
                if (weapon != null)
                {
                    weapon.gameObject.SetActive(false);
                }
            }
        }

        Debug.Log("=== FLOOR MANAGER READY ===");
    }

    void ValidateFloorSetup()
    {
        if (floors.Count == 0)
        {
            Debug.LogError("[FloorManager] ❌ No floors configured! Add floors in the inspector.");
            return;
        }

        Debug.Log($"[FloorManager] Validating {floors.Count} floors:");
        for (int i = 0; i < floors.Count; i++)
        {
            string floorInfo = $"  Floor {i}: ";

            if (string.IsNullOrEmpty(floors[i].floorName))
            {
                floorInfo += "[NO NAME]";
            }
            else
            {
                floorInfo += floors[i].floorName;
            }

            floorInfo += $" - Requires {floors[i].killsRequired} kills";

            if (i > 0 && floors[i].killsRequired <= 0)
            {
                floorInfo += " ⚠️ WARNING: killsRequired should be > 0!";
            }

            if (floors[i].blockingObject == null)
            {
                floorInfo += " ⚠️ NO BLOCKING OBJECT!";
            }
            else
            {
                floorInfo += $" - Blocking: {floors[i].blockingObject.name}";
            }

            Debug.Log(floorInfo);
        }
    }

    public void OnEnemyKilled()
    {
        totalKills++;

        Debug.Log($"[FloorManager] ═══ ENEMY KILLED ═══");
        Debug.Log($"[FloorManager] Total kills now: {totalKills}");
        Debug.Log($"[FloorManager] Checking {floors.Count} floors for unlocks...");

        // Check each floor to see if it should be unlocked
        for (int i = 0; i < floors.Count; i++)
        {
            Floor floor = floors[i];

            Debug.Log($"[FloorManager] Checking Floor {i} ({floor.floorName}): Unlocked={floor.isUnlocked}, Requires={floor.killsRequired}, Current={totalKills}");

            // Check if floor should unlock
            if (!floor.isUnlocked && totalKills >= floor.killsRequired)
            {
                Debug.Log($"[FloorManager] 🔓 CONDITION MET! Unlocking floor {i}: {floor.floorName}");
                UnlockFloor(i);
            }
            else if (!floor.isUnlocked)
            {
                int needed = floor.killsRequired - totalKills;
                Debug.Log($"[FloorManager] Floor {i} still locked - need {needed} more kills");
            }
        }

        Debug.Log($"[FloorManager] ═══ CHECK COMPLETE ═══");
    }

    void UnlockFloor(int floorIndex)
    {
        if (floorIndex < 0 || floorIndex >= floors.Count)
        {
            Debug.LogError($"[FloorManager] ❌ Invalid floor index: {floorIndex}");
            return;
        }

        Floor floor = floors[floorIndex];
        floor.isUnlocked = true;

        Debug.Log($"");
        Debug.Log($"╔═══════════════════════════════════════╗");
        Debug.Log($"║   UNLOCKING: {floor.floorName}");
        Debug.Log($"╚═══════════════════════════════════════╝");

        // Destroy blocking object (cage OR floorboards)
        if (floor.blockingObject != null)
        {
            Debug.Log($"[FloorManager] Found blocking object: {floor.blockingObject.name}");
            Debug.Log($"[FloorManager] Object is active: {floor.blockingObject.activeSelf}");
            Debug.Log($"[FloorManager] Object has {floor.blockingObject.transform.childCount} children");

            // Immediately disable all colliders
            Collider[] colliders = floor.blockingObject.GetComponentsInChildren<Collider>(true); // true = include inactive
            Debug.Log($"[FloorManager] Found {colliders.Length} colliders to disable");
            foreach (Collider col in colliders)
            {
                col.enabled = false;
                Debug.Log($"[FloorManager]   ✓ Disabled collider on: {col.gameObject.name}");
            }

            // Make it invisible
            Renderer[] renderers = floor.blockingObject.GetComponentsInChildren<Renderer>(true);
            Debug.Log($"[FloorManager] Found {renderers.Length} renderers to disable");
            foreach (Renderer rend in renderers)
            {
                rend.enabled = false;
                Debug.Log($"[FloorManager]   ✓ Disabled renderer on: {rend.gameObject.name}");
            }

            // Deactivate the whole object
            floor.blockingObject.SetActive(false);
            Debug.Log($"[FloorManager]   ✓ SetActive(false) on {floor.blockingObject.name}");

            // Now destroy it
            Destroy(floor.blockingObject);
            Debug.Log($"[FloorManager]   ✓ Destroy() called on {floor.blockingObject.name}");

            // Set reference to null
            floor.blockingObject = null;
        }
        else
        {
            Debug.LogError($"[FloorManager] ❌ No blocking object assigned for {floor.floorName}!");
        }

        // Spawn weapons
        int weaponsSpawned = 0;
        Debug.Log($"[FloorManager] Attempting to spawn {floor.weaponPickups.Count} weapons");
        foreach (WeaponPickup weapon in floor.weaponPickups)
        {
            if (weapon != null)
            {
                weapon.gameObject.SetActive(true);
                weaponsSpawned++;
                Debug.Log($"[FloorManager]   ✓ Spawned weapon: {weapon.weaponName}");
            }
        }
        Debug.Log($"[FloorManager] Total weapons spawned: {weaponsSpawned}");

        // Show message
        // if (hudController != null)
        // {
        //     hudController.ShowFloorUnlock($"{floor.floorName} UNLOCKED!");
        //     Debug.Log($"[FloorManager]   ✓ HUD message sent");
        // }
        // else
        // {
        //     Debug.LogError("[FloorManager] ❌ HUDController not assigned!");
        // }

        Debug.Log($"╔═══════════════════════════════════════╗");
        Debug.Log($"║   {floor.floorName} UNLOCK COMPLETE!");
        Debug.Log($"╚═══════════════════════════════════════╝");
        Debug.Log($"");
    }

    // Debug method to manually test unlocking
    [ContextMenu("Test Unlock Next Floor")]
    void TestUnlockNextFloor()
    {
        for (int i = 0; i < floors.Count; i++)
        {
            if (!floors[i].isUnlocked)
            {
                Debug.Log($"[TEST] Manually unlocking floor {i}");
                UnlockFloor(i);
                return;
            }
        }
        Debug.Log("[TEST] All floors already unlocked!");
    }

    [ContextMenu("Show Floor Status")]
    void ShowFloorStatus()
    {
        Debug.Log($"");
        Debug.Log($"═══════════════════════════════════════");
        Debug.Log($"FLOOR STATUS (Total Kills: {totalKills})");
        Debug.Log($"═══════════════════════════════════════");
        for (int i = 0; i < floors.Count; i++)
        {
            string status = floors[i].isUnlocked ? "🔓 UNLOCKED" : "🔒 LOCKED";
            string blockingStatus = floors[i].blockingObject != null ? $"Present ({floors[i].blockingObject.name})" : "NULL/Destroyed";
            Debug.Log($"Floor {i}: {floors[i].floorName}");
            Debug.Log($"  Status: {status}");
            Debug.Log($"  Blocking Object: {blockingStatus}");
            Debug.Log($"  Kills Required: {floors[i].killsRequired}");
            Debug.Log($"");
        }
        Debug.Log($"═══════════════════════════════════════");
    }

    [ContextMenu("Add 1 Kill (Test)")]
    void TestAddKill()
    {
        Debug.Log("[TEST] Adding 1 kill manually...");
        OnEnemyKilled();
    }

    // Getter methods
    public int GetTotalKills() => totalKills;
    public int GetNextFloorKillRequirement()
    {
        for (int i = 0; i < floors.Count; i++)
        {
            if (!floors[i].isUnlocked)
                return floors[i].killsRequired;
        }
        return -1;
    }
}