using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Panels (leave animator empty if using rotation)")]
    public Transform leftPanel;
    public Transform rightPanel;

    [Header("Animator (optional)")]
    public Animator doorAnimator; // set if you want an animation with "Open" trigger

    [Header("Rotation fallback")]
    public float openAngle = 90f; // degrees to rotate each panel (left = -openAngle, right = +openAngle)
    public float openSpeed = 6f;

    [Header("Unlock")]
    public int unlockAmount = 100; // how much money required to open
    public bool autoUnlockOnAmount = false; // if true, door opens automatically when money >= unlockAmount

    bool isOpen = false;
    Quaternion leftClosedRot, rightClosedRot;
    Quaternion leftTargetRot, rightTargetRot;

    void Start()
    {
        if (leftPanel) leftClosedRot = leftPanel.localRotation;
        if (rightPanel) rightClosedRot = rightPanel.localRotation;

        leftTargetRot = leftClosedRot * Quaternion.Euler(0, -openAngle, 0);
        rightTargetRot = rightClosedRot * Quaternion.Euler(0, openAngle, 0);

        if (autoUnlockOnAmount && MoneyManager.Instance != null)
        {
            MoneyManager.Instance.OnMoneyChanged += OnMoneyChanged;
        }
    }

    void OnDestroy()
    {
        if (MoneyManager.Instance != null) MoneyManager.Instance.OnMoneyChanged -= OnMoneyChanged;
    }

    void Update()
    {
        if (isOpen && doorAnimator == null)
        {
            // lerp panels to target
            if (leftPanel) leftPanel.localRotation = Quaternion.Slerp(leftPanel.localRotation, leftTargetRot, Time.deltaTime * openSpeed);
            if (rightPanel) rightPanel.localRotation = Quaternion.Slerp(rightPanel.localRotation, rightTargetRot, Time.deltaTime * openSpeed);
        }
    }

    void OnMoneyChanged(int current)
    {
        if (!isOpen && current >= unlockAmount) OpenDoor();
    }

    // called by PlayerInteract or other scripts
    public void TryOpen()
    {
        if (isOpen) return;

        if (MoneyManager.Instance != null && MoneyManager.Instance.CurrentMoney >= unlockAmount)
        {
            OpenDoor();
        }
        else
        {
            // locked feedback
            Debug.Log($"Door locked. Need ${unlockAmount} (you have ${MoneyManager.Instance?.CurrentMoney ?? 0})");
            // TODO: play sound / UI flash
        }
    }

    public void OpenDoor()
    {
        if (isOpen) return;
        isOpen = true;

        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger("Open");
        }
        else
        {
            // panels will lerp in Update to target rotations
        }
    }
}