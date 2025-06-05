using System.Collections;
using UnityEngine;

public class EndGameSequenceManager : MonoBehaviour
{
    [Header("Door Settings")]
    private GameObject doorObject;
    public float doorOpenHeight = 15f;
    public float doorOpenDuration = 6f;
    public Transform doorCameraTarget;
    
    [Header("Moon Settings")]
    private GameObject moonObject;
    public Transform moonCameraTarget;
    public float moonSequenceDuration = 6f;
    public float moonDestructionDelay = 1.5f; // Tiempo antes de destruir la luna
    public GameObject moonDestructionParticlesPrefab; // Add this line
    
    [Header("Camera Settings")]
    public float cameraMoveDuration = 3f;
    public Vector3 cameraOffset = new Vector3(0, 3f, -8f);
    public Vector3 moonCameraOffset = new Vector3(0, 5f, -10f);
    
    [Header("Sequence Settings")]
    public float totalSequenceDuration = 5f;
    
    private ThirdPersonCamera originalCamera;
    private PlayerController playerController;
    private Camera mainCamera;
    private bool sequenceActive = false;

    private void Start()
    {
        originalCamera = FindFirstObjectByType<ThirdPersonCamera>();
        playerController = FindFirstObjectByType<PlayerController>();
        mainCamera = Camera.main;
        
        // Si no se asigna moonObject manualmente, buscar por tag
        doorObject = GameObject.FindGameObjectWithTag("PuertaFinal");
        moonObject = GameObject.FindGameObjectWithTag("Luna");
        
    }

    public void StartEndGameSequence()
    {
        if (sequenceActive) return;
        StartCoroutine(ExecuteEndGameSequence());
    }

    private IEnumerator ExecuteEndGameSequence()
    {
        sequenceActive = true;
        Debug.Log("Iniciando secuencia de final del juego");

        // 1. Ocultar UI de salud
        PlayerHealth playerHealth = playerController.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.healthUI.HideUI();
        }

        // 2. Deshabilitar control del jugador y cámara original
        playerController.enabled = false;
        originalCamera.enabled = false;
        var menu = FindFirstObjectByType<MainMenuManager>();
        menu.enabled = false;

        // 3. Ejecutar secuencia completa (luna + puertas + cámara)
        yield return StartCoroutine(MoonAndDoorSequence());

        // 4. Restaurar control
        playerController.enabled = true;
        originalCamera.enabled = true;
        menu.enabled = true;

        Debug.Log("Secuencia de final completada");
        sequenceActive = false;
    }

    private IEnumerator MoonAndDoorSequence()
    {
        AudioManager.Instance.StopMusic();
        
        // Guardar posición original de la cámara
        Vector3 originalCameraPos = mainCamera.transform.position;
        Quaternion originalCameraRot = mainCamera.transform.rotation;

        // === FASE 1: SECUENCIA DE LA LUNA ===
        yield return StartCoroutine(MoonSequence(originalCameraPos, originalCameraRot));

        // === FASE 2: SECUENCIA DE PUERTAS ===
        yield return StartCoroutine(DoorSequence(originalCameraPos, originalCameraRot));
    }

    private IEnumerator MoonSequence(Vector3 originalCameraPos, Quaternion originalCameraRot)
    {
        if (moonObject == null || moonCameraTarget == null)
        {
            Debug.LogWarning("Luna o objetivo de cámara de luna no asignados");
            yield break;
        }

        Debug.Log("Iniciando secuencia de la luna");

        // Calcular posición objetivo de la cámara para la luna
        Vector3 targetMoonCameraPos = moonCameraTarget.position + moonCameraOffset;

        // Mover cámara hacia la luna
        float elapsedTime = 0f;
        while (elapsedTime < cameraMoveDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / cameraMoveDuration;
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            // Interpolar posición
            Vector3 currentPos = Vector3.Lerp(originalCameraPos, targetMoonCameraPos, smoothProgress);
            mainCamera.transform.position = currentPos;
            
            // Usar LookAt para mirar hacia la luna
            mainCamera.transform.LookAt(moonObject.transform.position);

            yield return null;
        }

        // Esperar antes de destruir la luna
        yield return new WaitForSeconds(moonDestructionDelay);

        // Destruir la luna
        if (moonObject != null)
        {
            Debug.Log("Destruyendo la luna");
    
            // Create moon destruction particle effect
            CreateMoonDestructionParticles(moonObject.transform.position);
    
            Destroy(moonObject);
            AudioManager.Instance.PlaySFX("Explosion");
        }

        GameObject skyboxObject = GameObject.FindWithTag("skybox");
        Skybox skyboxComponent = skyboxObject.GetComponent<Skybox>();
        skyboxComponent.material = Resources.Load<Material>("Materials/Skybox/skybox dia");

        // Force refresh
        RenderSettings.skybox = skyboxComponent.material;
        DynamicGI.UpdateEnvironment();
        
        // Esperar el tiempo restante de la secuencia de luna
        float remainingMoonTime = moonSequenceDuration - moonDestructionDelay;
        if (remainingMoonTime > 0)
        {
            yield return new WaitForSeconds(remainingMoonTime);
        }
    }

    private IEnumerator DoorSequence(Vector3 originalCameraPos, Quaternion originalCameraRot)
    {
        Debug.Log("Iniciando secuencia de puertas");

        // Guardar rotación original de la puerta
        Quaternion originalDoorRotation = doorObject.transform.rotation;
        
        // Calcular el punto de pivote (base inferior de la puerta)
        Bounds doorBounds = doorObject.GetComponent<Renderer>().bounds;
        Vector3 pivotPoint = new Vector3(
            doorBounds.center.x,
            doorBounds.min.y, // Parte inferior
            doorBounds.center.z
        );

        // Calcular posición objetivo de la cámara para las puertas
        Vector3 targetDoorCameraPos = doorCameraTarget.position + cameraOffset;

        float elapsedTime = 0f;
        float maxDuration = Mathf.Max(doorOpenDuration, cameraMoveDuration);

        AudioManager.Instance.PlaySFX("FallingNoise");
        while (elapsedTime < maxDuration)
        {
            elapsedTime += Time.deltaTime;
            
            // Rotar puerta desde la base
            if (elapsedTime <= doorOpenDuration)
            {
                float doorProgress = elapsedTime / doorOpenDuration;
                float smoothDoorProgress = Mathf.SmoothStep(0f, 1f, doorProgress);

                // Calcular la rotación objetivo (90 grados en X)
                float targetRotationX = 90f * smoothDoorProgress;
                
                // Aplicar rotación desde el punto de pivote
                doorObject.transform.rotation = originalDoorRotation;
                doorObject.transform.RotateAround(pivotPoint, doorObject.transform.right, targetRotationX);
            }

            // Mover cámara hacia las puertas
            if (elapsedTime <= cameraMoveDuration)
            {
                float cameraProgress = elapsedTime / cameraMoveDuration;
                float smoothCameraProgress = Mathf.SmoothStep(0f, 1f, cameraProgress);

                Vector3 currentCameraPos = mainCamera.transform.position;
                
                // Interpolar posición
                Vector3 newPos = Vector3.Lerp(currentCameraPos, targetDoorCameraPos, smoothCameraProgress);
                mainCamera.transform.position = newPos;
                
                // Usar LookAt para mirar hacia las puertas
                mainCamera.transform.LookAt(doorObject.transform.position);
            }

            yield return null;
        }
        
        doorObject.transform.position = new Vector3(43.5f, 3.5f, 0.5f);

        // Esperar el tiempo restante si es necesario
        float remainingTime = totalSequenceDuration - maxDuration;
        if (remainingTime > 0)
        {
            yield return new WaitForSeconds(remainingTime);
        }
    }
    
    private void CreateMoonDestructionParticles(Vector3 position)
    {
        // Create multiple particle systems for different effects
        
        // 1. Large debris chunks
        CreateDebrisParticles(position, "LargeDebris", 50, 10f, 30f, 16f, 30f, true);
        
        // 2. Small debris particles
        CreateDebrisParticles(position, "SmallDebris", 150, 2f, 10f, 10f, 40f, false);
        
        // 3. Dust cloud
        CreateDustCloud(position);
    }

    private void CreateDebrisParticles(Vector3 position, string name, int count, float minSize, float maxSize, float minSpeed, float maxSpeed, bool is3d)
    {
        GameObject particleObject = new GameObject($"Moon{name}");
        particleObject.transform.position = position;

        ParticleSystem particles = particleObject.AddComponent<ParticleSystem>();

        // Get the renderer and create URP-compatible material
        var renderer = particles.GetComponent<ParticleSystemRenderer>();
        
        // Create a URP-compatible material for particles
        Material particleMaterial = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        
        // Try to load your luna texture
        Material lunaTexture = Resources.Load<Material>("Materials/MaterialParticulaLuna"); // Adjust path as needed
        if (lunaTexture != null)
        {
            particleMaterial = lunaTexture;
        }
        
        // Set material properties for URP
        particleMaterial.SetFloat("_Surface", 1); // Transparent
        particleMaterial.SetFloat("_Blend", 0); // Alpha blend
        particleMaterial.SetFloat("_AlphaClip", 0);
        particleMaterial.SetFloat("_SrcBlend", 5); // SrcAlpha
        particleMaterial.SetFloat("_DstBlend", 10); // OneMinusSrcAlpha
        particleMaterial.SetFloat("_ZWrite", 0);
        particleMaterial.renderQueue = 3000;
        
        renderer.material = particleMaterial;
        MeshFilter meshFilter = moonObject.GetComponent<MeshFilter>();
        if (is3d && meshFilter != null && meshFilter.mesh != null) {
            renderer.renderMode = ParticleSystemRenderMode.Mesh;
            renderer.mesh = meshFilter.mesh;
        }
        else {
            renderer.alignment = ParticleSystemRenderSpace.Facing; // Face camera
        }

        renderer.sortMode = ParticleSystemSortMode.Distance;
        
        var main = particles.main;
        main.startLifetime = 8f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(minSpeed, maxSpeed);
        main.startSize = new ParticleSystem.MinMaxCurve(minSize, maxSize);
        main.startColor = new ParticleSystem.MinMaxGradient(Color.gray, new Color(0.8f, 0.8f, 0.7f));
        main.maxParticles = count;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.loop = false;
        
        // IMPORTANT: Set particle shape to sphere for rounder appearance
        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 3f;
        
        var emission = particles.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0.0f, count)
        });
        
        var velocityOverLifetime = particles.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(5f, 10f);
        
        var forceOverLifetime = particles.forceOverLifetime;
        forceOverLifetime.enabled = true;
        forceOverLifetime.y = -9.81f;
        
        // Add 3D rotation for more natural tumbling
        var rotationOverLifetime = particles.rotationOverLifetime;
        rotationOverLifetime.enabled = true;
        rotationOverLifetime.separateAxes = true;
        rotationOverLifetime.x = new ParticleSystem.MinMaxCurve(-2f, 2f);
        rotationOverLifetime.y = new ParticleSystem.MinMaxCurve(-2f, 2f);
        rotationOverLifetime.z = new ParticleSystem.MinMaxCurve(-3f, 3f);
        
        // Add size variation over lifetime for more organic shapes
        var sizeOverLifetime = particles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(0.3f, UnityEngine.Random.Range(0.8f, 1.2f));
        sizeCurve.AddKey(0.7f, UnityEngine.Random.Range(0.9f, 1.1f));
        sizeCurve.AddKey(1f, 0.95f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        
        // Enhanced noise for more organic movement
        var noise = particles.noise;
        noise.enabled = true;
        noise.strength = 0.4f;
        noise.frequency = 0.3f;
        noise.octaveCount = 3;
        noise.scrollSpeed = 0.2f;
        noise.damping = true;
        
        Destroy(particleObject, 8f);
    }

    private void CreateDustCloud(Vector3 position)
    {
        GameObject dustObject = new GameObject("MoonDustCloud");
        dustObject.transform.position = position;
        
        ParticleSystem dust = dustObject.AddComponent<ParticleSystem>();
        
        // Get renderer for dust and create URP-compatible material
        var renderer = dust.GetComponent<ParticleSystemRenderer>();
        
        // Create URP-compatible dust material
        Material dustMaterial = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        
        // Set up for soft, transparent dust
        dustMaterial.SetFloat("_Surface", 1); // Transparent
        dustMaterial.SetFloat("_Blend", 1); // Additive for soft glow
        dustMaterial.SetFloat("_SrcBlend", 1); // One
        dustMaterial.SetFloat("_DstBlend", 1); // One
        dustMaterial.SetFloat("_ZWrite", 0);
        dustMaterial.renderQueue = 3000;
        dustMaterial.color = new Color(0.7f, 0.7f, 0.6f, 0.3f);
        
        renderer.material = dustMaterial;
        
        // Make dust particles face the camera and be soft
        renderer.alignment = ParticleSystemRenderSpace.Facing;
        
        var main = dust.main;
        main.startLifetime = 10f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 8f);
        main.startSize = new ParticleSystem.MinMaxCurve(1f, 4f);
        main.startColor = new Color(0.7f, 0.7f, 0.6f, 0.3f);
        main.maxParticles = 100;
        main.loop = false;
        
        var emission = dust.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0.0f, 100)
        });
        
        var shape = dust.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 5f;
        
        // Dust expands slowly
        var velocityOverLifetime = dust.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(2f, 5f);
        
        // Fade out over time
        var colorOverLifetime = dust.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.gray, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0.8f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
        );
        colorOverLifetime.color = gradient;
        
        // Add noise to dust for more natural movement
        var noise = dust.noise;
        noise.enabled = true;
        noise.strength = 0.2f;
        noise.frequency = 0.3f;
        noise.octaveCount = 1;
        
        Destroy(dustObject, 8f);
    }
}