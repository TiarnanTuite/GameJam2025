using UnityEngine;
using UnityEngine.InputSystem;

public class GunBob : MonoBehaviour
{
    //settings for bobing while moving
    [Header("Bobbing Settings")]
    public float bobbingSpeed = 10f;
    public float bobbingAmount = 0.05f;
    public float runBobbingMultiplier = 1.5f;

    //gun movement while idle
    [Header("Idle Sway")]
    public float idleAmount = 0.02f;
    public float idleSpeed = 1f;

    private Vector3 originalPosition;
    private float timer = 0f;
    private CharacterController characterController;

    void Start()
    {
        originalPosition = transform.localPosition;
        characterController = GetComponentInParent<CharacterController>();
    }

    void Update()
    {
        bool isMoving = false;
        bool isRunning = false;

        if (characterController != null)
        {
            // Check if moving based on velocity
            isMoving = characterController.velocity.magnitude > 0.1f;
        }
        else
        {
            // Fallback: check new input system
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                isMoving = keyboard.wKey.isPressed || keyboard.aKey.isPressed ||
                          keyboard.sKey.isPressed || keyboard.dKey.isPressed;
            }
        }

        // Check if sprinting (new input system)
        var keyboardSprint = Keyboard.current;
        if (keyboardSprint != null)
        {
            isRunning = keyboardSprint.leftShiftKey.isPressed;
        }

        if (isMoving)
        {
            // Apply bobbing
            float speedMultiplier = isRunning ? runBobbingMultiplier : 1f;
            timer += Time.deltaTime * bobbingSpeed * speedMultiplier;

            float bobbingAmountAdjusted = bobbingAmount * speedMultiplier;

            // Calculate wave motion
            float verticalBob = Mathf.Sin(timer) * bobbingAmountAdjusted;
            float horizontalBob = Mathf.Cos(timer * 0.5f) * bobbingAmountAdjusted * 0.5f;

            // Apply bobbing offset
            transform.localPosition = originalPosition + new Vector3(horizontalBob, verticalBob, 0f);
        }
        else
        {
            // Idle sway
            timer += Time.deltaTime * idleSpeed;

            float idleX = Mathf.Sin(timer) * idleAmount;
            float idleY = Mathf.Cos(timer * 0.5f) * idleAmount * 0.5f;

            // Smoothly return to original position with idle sway
            transform.localPosition = Vector3.Lerp(
                transform.localPosition,
                originalPosition + new Vector3(idleX, idleY, 0f),
                Time.deltaTime * 5f
            );
        }
    }
}