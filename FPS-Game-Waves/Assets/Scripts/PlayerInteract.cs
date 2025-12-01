using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    public Camera playerCamera; // assign the FPS camera
    public float interactRange = 3f;
    public LayerMask interactMask = ~0; // default all
    public KeyCode interactKey = KeyCode.E;

    void Reset()
    {
        // try to auto-find a Camera on player
        playerCamera = GetComponentInChildren<Camera>();
    }

    void Update()
    {
        if (Input.GetKeyDown(interactKey))
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
