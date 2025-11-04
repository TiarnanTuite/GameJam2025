using UnityEngine;

public class MenuObjectRotation : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("Maximum rotation angle in degrees")]
    public float rotationAmplitude = 30f;
    
    [Tooltip("Speed of the rotation oscillation")]
    public float rotationSpeed = 1f;
    
    [Header("Rotation Axes")]
    [Tooltip("Enable rotation on X axis")]
    public bool rotateX = false;
    
    [Tooltip("Enable rotation on Y axis")]
    public bool rotateY = true;
    
    [Tooltip("Enable rotation on Z axis")]
    public bool rotateZ = false;
    
    [Header("Phase Offsets (Optional)")]
    [Tooltip("Phase offset for X rotation (creates variety)")]
    public float phaseOffsetX = 0f;
    
    [Tooltip("Phase offset for Y rotation")]
    public float phaseOffsetY = 0f;
    
    [Tooltip("Phase offset for Z rotation")]
    public float phaseOffsetZ = 0f;
    
    private Quaternion initialRotation;
    private float timeCounter = 0f;
    
    void Start()
    {
        // Store the initial rotation
        initialRotation = transform.rotation;
    }
    
    void Update()
    {
        // Increment time counter
        timeCounter += Time.deltaTime * rotationSpeed;
        
        // Calculate rotation angles using sine wave
        float rotX = rotateX ? Mathf.Sin(timeCounter + phaseOffsetX) * rotationAmplitude : 0f;
        float rotY = rotateY ? Mathf.Sin(timeCounter + phaseOffsetY) * rotationAmplitude : 0f;
        float rotZ = rotateZ ? Mathf.Sin(timeCounter + phaseOffsetZ) * rotationAmplitude : 0f;
        
        // Create the oscillation rotation
        Quaternion oscillation = Quaternion.Euler(rotX, rotY, rotZ);
        
        // Apply oscillation on top of the initial rotation
        transform.rotation = initialRotation * oscillation;
    }
}