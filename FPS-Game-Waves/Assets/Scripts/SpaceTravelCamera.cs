using UnityEngine;

public class SpaceTravelCamera : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Speed of forward movement")]
    public float travelSpeed = 5f;
    
    [Tooltip("The parent GameObject containing all space objects to move")]
    public GameObject spaceObjects;
    
    [Header("Seamless Loop Settings")]
    [Tooltip("Distance before objects reset (for seamless looping)")]
    public float resetDistance = 100f;
    
    [Tooltip("Enable seamless looping")]
    public bool enableLooping = true;
    
    private Vector3 startPosition;
    
    void Start()
    {
        if (spaceObjects != null)
        {
            startPosition = spaceObjects.transform.position;
        }
    }
    
    void Update()
    {
        if (spaceObjects == null) return;
        
        // Move objects backward (creates illusion of camera moving forward)
        spaceObjects.transform.position += Vector3.back * travelSpeed * Time.deltaTime;
        
        // Seamless loop: reset position when objects have moved far enough
        if (enableLooping)
        {
            float distanceTraveled = startPosition.z - spaceObjects.transform.position.z;
            
            if (distanceTraveled >= resetDistance)
            {
                // Reset to start position for seamless loop
                spaceObjects.transform.position = startPosition;
            }
        }
    }
}