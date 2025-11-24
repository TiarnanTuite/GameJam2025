using UnityEngine;
using System.Collections.Generic;

public class FloorManager : MonoBehaviour
{
    [System.Serializable]
    public class Floor
    {
        public string floorName;
        public int killsRequired;
        public GameObject floorObject; // The floor GameObject to activate
        public GameObject blockedDoor; // Door/barrier to remove
        public List<WeaponPickup> weaponPickups; // Weapon pickups that spawn
        public bool isUnlocked = false;
    }

    [Header("References")]
    public HUDController hudController;

    [Header("Floors")]
    public List<Floor> floors = new List<Floor>();

    private int totalKills = 0;

    void Start()
    {
        // Make sure first floor is unlocked
        if (floors.Count > 0)
        {
            floors[0].isUnlocked = true;
            if (floors[0].floorObject != null)
                floors[0].floorObject.SetActive(true);
        }

        // Lock other floors initially
        for (int i = 1; i < floors.Count; i++)
        {
            if (floors[i].floorObject != null)
                floors[i].floorObject.SetActive(false);

            // Disable weapon pickups
            foreach (WeaponPickup weapon in floors[i].weaponPickups)
            {
                if (weapon != null)
                    weapon.gameObject.SetActive(false);
            }
        }
    }

    public void OnEnemyKilled()
    {
        totalKills++;
        Debug.Log($"Total kills: {totalKills}");

        // Check if any floors should unlock
        CheckFloorUnlocks();
    }

    void CheckFloorUnlocks()
    {
        for (int i = 0; i < floors.Count; i++)
        {
            Floor floor = floors[i];

            if (!floor.isUnlocked && totalKills >= floor.killsRequired)
            {
                UnlockFloor(i);
            }
        }
    }

    void UnlockFloor(int floorIndex)
    {
        if (floorIndex < 0 || floorIndex >= floors.Count)
            return;

        Floor floor = floors[floorIndex];
        floor.isUnlocked = true;

        // Activate floor
        if (floor.floorObject != null)
            floor.floorObject.SetActive(true);

        // Remove door/barrier
        if (floor.blockedDoor != null)
            floor.blockedDoor.SetActive(false);

        // Spawn weapons
        foreach (WeaponPickup weapon in floor.weaponPickups)
        {
            if (weapon != null)
                weapon.gameObject.SetActive(true);
        }

        // Show notification
        if (hudController != null)
        {
            hudController.ShowFloorUnlock($"FLOOR UNLOCKED: {floor.floorName}");
        }

        Debug.Log($"Floor unlocked: {floor.floorName} ({floor.killsRequired} kills)");
    }

    public int GetTotalKills()
    {
        return totalKills;
    }

    public int GetNextFloorRequirement()
    {
        for (int i = 0; i < floors.Count; i++)
        {
            if (!floors[i].isUnlocked)
            {
                return floors[i].killsRequired;
            }
        }
        return -1; // All floors unlocked
    }
}