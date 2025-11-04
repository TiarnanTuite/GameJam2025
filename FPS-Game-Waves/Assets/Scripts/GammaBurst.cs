using UnityEngine;

public class GammaBurst : MonoBehaviour
{
    [Header("Burst Settings")]
    [Tooltip("Direction the gamma burst fires")]
    public Vector3 burstDirection = Vector3.forward;
    
    [Tooltip("Length of the gamma burst beam")]
    public float burstLength = 50f;
    
    [Tooltip("Width of the gamma burst beam")]
    public float burstWidth = 2f;
    
    [Tooltip("Duration of the burst in seconds")]
    public float burstDuration = 3f;
    
    [Tooltip("Color of the gamma burst")]
    public Color burstColor = new Color(0.5f, 0.8f, 1f, 1f); // Cyan-blue
    
    [Tooltip("Custom material for the beam (optional - leave empty for auto-generated)")]
    public Material customBeamMaterial;
    
    [Header("Animation")]
    [Tooltip("Intensity pulses per second")]
    public float pulseSpeed = 5f;
    
    [Tooltip("How much the intensity varies")]
    public float pulseAmount = 0.3f;
    
    [Header("Trigger")]
    [Tooltip("Auto-trigger on start")]
    public bool autoTrigger = false;
    
    [Tooltip("Automatically shoot in random directions")]
    public bool autoShootRandom = false;
    
    [Tooltip("Time between random shots (in seconds)")]
    public float timeBetweenShots = 5f;
    
    private GameObject burstBeam;
    private ParticleSystem burstParticles;
    private float burstTimer = 0f;
    private bool isBursting = false;
    private Material beamMaterial;
    private float shootTimer = 0f;
    
    void Start()
    {
        if (autoTrigger)
        {
            TriggerBurst();
        }
    }
    
    void Update()
    {
        // Auto shoot in random directions
        if (autoShootRandom && !isBursting)
        {
            shootTimer += Time.deltaTime;
            if (shootTimer >= timeBetweenShots)
            {
                shootTimer = 0f;
                burstDirection = Random.onUnitSphere;
                TriggerBurst();
            }
        }
        
        // Update burst
        if (isBursting)
        {
            burstTimer += Time.deltaTime;
            
            // Animate intensity with pulse
            if (beamMaterial != null)
            {
                float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
                Color pulsedColor = burstColor * pulse;
                beamMaterial.SetColor("_EmissionColor", pulsedColor * 2f);
            }
            
            // End burst after duration
            if (burstTimer >= burstDuration)
            {
                EndBurst();
            }
        }
    }
    
    public void TriggerBurst()
    {
        if (isBursting) return;
        
        isBursting = true;
        burstTimer = 0f;
        
        CreateBeam();
        CreateParticles();
    }
    
    void CreateBeam()
    {
        // Create beam cylinder
        burstBeam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        burstBeam.name = "GammaBurstBeam";
        burstBeam.transform.SetParent(transform);
        
        // Remove collider
        Destroy(burstBeam.GetComponent<Collider>());
        
        // Position and scale
        Vector3 normalizedDir = burstDirection.normalized;
        burstBeam.transform.localPosition = normalizedDir * (burstLength / 2f);
        burstBeam.transform.localScale = new Vector3(burstWidth, burstLength / 2f, burstWidth);
        
        // Rotate to point in burst direction
        burstBeam.transform.localRotation = Quaternion.FromToRotation(Vector3.up, normalizedDir);
        
        // Use custom material if provided, otherwise create default
        if (customBeamMaterial != null)
        {
            beamMaterial = new Material(customBeamMaterial);
        }
        else
        {
            // Create glowing material with transparency support
            beamMaterial = new Material(Shader.Find("Standard"));
            beamMaterial.SetFloat("_Mode", 3); // Set to Transparent mode
            beamMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            beamMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            beamMaterial.SetInt("_ZWrite", 0);
            beamMaterial.DisableKeyword("_ALPHATEST_ON");
            beamMaterial.EnableKeyword("_ALPHABLEND_ON");
            beamMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            beamMaterial.renderQueue = 3000;
            
            beamMaterial.EnableKeyword("_EMISSION");
            
            // Apply color with alpha for transparency
            Color colorWithAlpha = new Color(burstColor.r, burstColor.g, burstColor.b, burstColor.a);
            beamMaterial.SetColor("_Color", colorWithAlpha);
            
            // Emission shouldn't have alpha
            Color emissionColor = new Color(burstColor.r, burstColor.g, burstColor.b, 1f) * 2f;
            beamMaterial.SetColor("_EmissionColor", emissionColor);
            
            beamMaterial.SetFloat("_Metallic", 0f);
            beamMaterial.SetFloat("_Glossiness", 0.8f);
        }
        
        burstBeam.GetComponent<Renderer>().material = beamMaterial;
    }
    
    void CreateParticles()
    {
        // Create particle system
        GameObject particlesObj = new GameObject("GammaBurstParticles");
        particlesObj.transform.SetParent(transform);
        particlesObj.transform.localPosition = Vector3.zero;
        
        burstParticles = particlesObj.AddComponent<ParticleSystem>();
        
        var main = burstParticles.main;
        main.startLifetime = 1.5f;
        main.startSpeed = burstLength / 1.5f;
        main.startSize = burstWidth * 0.5f;
        main.startColor = burstColor;
        main.maxParticles = 1000;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        var emission = burstParticles.emission;
        emission.rateOverTime = 200f;
        
        var shape = burstParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 5f;
        shape.radius = 0.1f;
        shape.rotation = Quaternion.LookRotation(burstDirection).eulerAngles;
        
        var colorOverLifetime = burstParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(burstColor, 0f), 
                new GradientColorKey(burstColor, 1f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1f, 0f), 
                new GradientAlphaKey(0f, 1f) 
            }
        );
        colorOverLifetime.color = gradient;
        
        var renderer = burstParticles.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.material.SetColor("_EmissionColor", burstColor);
    }
    
    void EndBurst()
    {
        isBursting = false;
        
        if (burstBeam != null)
        {
            Destroy(burstBeam);
        }
        
        if (burstParticles != null)
        {
            var emission = burstParticles.emission;
            emission.enabled = false;
            Destroy(burstParticles.gameObject, 2f);
        }
        
        if (beamMaterial != null)
        {
            Destroy(beamMaterial);
        }
    }
    
    void OnDestroy()
    {
        if (burstBeam != null) Destroy(burstBeam);
        if (burstParticles != null) Destroy(burstParticles.gameObject);
        if (beamMaterial != null) Destroy(beamMaterial);
    }
}