using System;
using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance { get; private set; }

    [Tooltip("Money player currently has")]
    public int CurrentMoney = 0;

    [Tooltip("Money required to open the door")]
    public int RequiredMoney = 500;

    public event Action<int> OnMoneyChanged;
    public event Action OnDoorUnlocked;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddMoney(int amount)
    {
        if (amount <= 0) return;
        CurrentMoney += amount;
        OnMoneyChanged?.Invoke(CurrentMoney);
        if (CurrentMoney >= RequiredMoney) OnDoorUnlocked?.Invoke();
    }

    // Call this when an enemy dies. percent should be 0.25f for 25%.
    public void AddKillPercent(float percent)
    {
        int reward = Mathf.CeilToInt(RequiredMoney * percent);
        AddMoney(reward);
    }

    // Optional: reset or spend
    public bool Spend(int amount)
    {
        if (CurrentMoney < amount) return false;
        CurrentMoney -= amount;
        OnMoneyChanged?.Invoke(CurrentMoney);
        return true;
    }
}