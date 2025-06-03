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
    public float moonSequenceDuration = 3f;
    public float moonDestructionDelay = 2f; // Tiempo antes de destruir la luna
    
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

        // 3. Ejecutar secuencia completa (luna + puertas + cámara)
        yield return StartCoroutine(MoonAndDoorSequence());

        // 4. Restaurar control
        playerController.enabled = true;
        originalCamera.enabled = true;

        Debug.Log("Secuencia de final completada");
        sequenceActive = false;
    }

    private IEnumerator MoonAndDoorSequence()
    {
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
            Destroy(moonObject);
        }

        GameObject.FindWithTag("skybox").GetComponent<Skybox>().material = Resources.Load<Material>("Materials/Skybox/skybox dia");
        
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

        // Guardar posiciones originales de las puertas
        Vector3 originalDoorPosition = doorObject.transform.position;
        

        // Calcular posición objetivo de la cámara para las puertas
        Vector3 targetDoorCameraPos = doorCameraTarget.position + cameraOffset;

        float elapsedTime = 0f;
        float maxDuration = Mathf.Max(doorOpenDuration, cameraMoveDuration);

        while (elapsedTime < maxDuration)
        {
            elapsedTime += Time.deltaTime;
            
            // Mover puertas
            if (elapsedTime <= doorOpenDuration)
            {
                float doorProgress = elapsedTime / doorOpenDuration;
                float smoothDoorProgress = Mathf.SmoothStep(0f, 1f, doorProgress);

                Vector3 targetPos = originalDoorPosition + Vector3.up * doorOpenHeight;
                doorObject.transform.position = Vector3.Lerp(originalDoorPosition, targetPos, smoothDoorProgress);
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

        // Esperar el tiempo restante si es necesario
        float remainingTime = totalSequenceDuration - maxDuration;
        if (remainingTime > 0)
        {
            yield return new WaitForSeconds(remainingTime);
        }
    }
}