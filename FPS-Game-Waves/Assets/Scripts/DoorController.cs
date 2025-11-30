using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Tooltip("Animator with open animation OR we will rotate the door transform")]
    public Animator doorAnimator;
    public Transform doorTransform; // fallback if no animator
    public Vector3 openRotation = new Vector3(0, 90, 0);
    public float openSpeed = 6f;

    bool isOpen = false;
    Quaternion targetRotation;

    void Start()
    {
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.OnMoneyChanged += OnMoneyChanged;
            MoneyManager.Instance.OnDoorUnlocked += OpenDoor;
        }

        if (doorTransform != null) targetRotation = Quaternion.Euler(transform.localEulerAngles + openRotation);
    }

    void OnDestroy()
    {
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.OnMoneyChanged -= OnMoneyChanged;
            MoneyManager.Instance.OnDoorUnlocked -= OpenDoor;
        }
    }

    void Update()
    {
        if (!isOpen && doorAnimator == null && doorTransform != null)
        {
            // smooth rotate to open when targetRotation set
            doorTransform.localRotation = Quaternion.Lerp(doorTransform.localRotation, targetRotation, Time.deltaTime * openSpeed);
        }
    }

    void OnMoneyChanged(int current)
    {
        // optional visual feedback when near unlock
    }

    // Try to open via interaction (e.g., key press when near)
    public void TryOpen()
    {
        if (isOpen) return;
        if (MoneyManager.Instance != null && MoneyManager.Instance.CurrentMoney >= MoneyManager.Instance.RequiredMoney)
        {
            OpenDoor();
        }
        else
        {
            // locked feedback (sound/UI blink)
            Debug.Log("Door locked. Need $" + MoneyManager.Instance.RequiredMoney);
        }
    }

    public void OpenDoor()
    {
        if (isOpen) return;
        isOpen = true;
        if (doorAnimator != null) doorAnimator.SetTrigger("Open");
        // if not using animator, doorTransform will be rotated in Update towards targetRotation
    }
}