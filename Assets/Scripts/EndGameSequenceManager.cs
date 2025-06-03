using System.Collections;
using UnityEngine;

public class EndGameSequenceManager : MonoBehaviour
{
    [Header("Door Settings")]
    public GameObject[] doorObjects;
    public float doorOpenHeight = 5f;
    public float doorOpenDuration = 3f;
    public Transform doorCameraTarget;
    
    [Header("Camera Settings")]
    public float cameraMoveDuration = 3f;
    public Vector3 cameraOffset = new Vector3(0, 3f, -8f);
    
    [Header("Sequence Settings")]
    public float totalSequenceDuration = 5f;
    
    private ThirdPersonCamera originalCamera;
    private PlayerController playerController;
    private Camera mainCamera;
    private bool sequenceActive = false;

    private void Start()
    {
        originalCamera = FindObjectOfType<ThirdPersonCamera>();
        playerController = FindObjectOfType<PlayerController>();
        mainCamera = Camera.main;
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

        // 3. Ejecutar secuencia de puertas y cámara
        yield return StartCoroutine(DoorAndCameraSequence());

        // 4. Restaurar control
        playerController.enabled = true;
        originalCamera.enabled = true;

        Debug.Log("Secuencia de final completada");
        sequenceActive = false;
    }

    private IEnumerator DoorAndCameraSequence()
    {
        // Guardar posiciones originales de las puertas
        Vector3[] originalDoorPositions = new Vector3[doorObjects.Length];
        for (int i = 0; i < doorObjects.Length; i++)
        {
            if (doorObjects[i] != null)
                originalDoorPositions[i] = doorObjects[i].transform.position;
        }

        // Guardar posición original de la cámara
        Vector3 originalCameraPos = mainCamera.transform.position;
        Quaternion originalCameraRot = mainCamera.transform.rotation;

        // Calcular posición objetivo de la cámara
        Vector3 targetCameraPos = doorCameraTarget.position + cameraOffset;
        Quaternion targetCameraRot = Quaternion.LookRotation(doorCameraTarget.position - targetCameraPos);

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

                for (int i = 0; i < doorObjects.Length; i++)
                {
                    if (doorObjects[i] != null)
                    {
                        Vector3 targetPos = originalDoorPositions[i] + Vector3.up * doorOpenHeight;
                        doorObjects[i].transform.position = Vector3.Lerp(originalDoorPositions[i], targetPos, smoothDoorProgress);
                    }
                }
            }

            // Mover cámara
            if (elapsedTime <= cameraMoveDuration)
            {
                float cameraProgress = elapsedTime / cameraMoveDuration;
                float smoothCameraProgress = Mathf.SmoothStep(0f, 1f, cameraProgress);

                mainCamera.transform.position = Vector3.Lerp(originalCameraPos, targetCameraPos, smoothCameraProgress);
                mainCamera.transform.rotation = Quaternion.Lerp(originalCameraRot, targetCameraRot, smoothCameraProgress);
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