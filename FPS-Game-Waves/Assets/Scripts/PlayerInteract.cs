using UnityEngine;
using UnityEngine.InputSystem; // ADD THIS LINE

public class PlayerInteract : MonoBehaviour
{
    public Camera playerCamera;
    public float interactRange = 3f;
    public LayerMask interactMask = ~0;

    void Reset()
    {
        playerCamera = GetComponentInChildren<Camera>();
    }

    void Update()
    {
        // NEW INPUT SYSTEM
        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.eKey.wasPressedThisFrame)
        {
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactMask))
            {
                var door = hit.collider.GetComponentInParent<DoorController>();
                if (door != null)
                {
                    door.TryOpen();
                }
            }
        }
    }
}