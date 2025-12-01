using UnityEngine;
using TMPro;

public class HUDMoney : MonoBehaviour
{
    public TextMeshProUGUI moneyText;

    void OnEnable()
    {
        if (MoneyManager.Instance != null) MoneyManager.Instance.OnMoneyChanged += Refresh;
        Refresh(MoneyManager.Instance != null ? MoneyManager.Instance.CurrentMoney : 0);
    }

    void OnDisable()
    {
        if (MoneyManager.Instance != null) MoneyManager.Instance.OnMoneyChanged -= Refresh;
    }

    void Refresh(int value)
    {
        if (moneyText != null) moneyText.text = "$" + value;
    }
}