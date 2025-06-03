using UnityEngine;
using System.Collections;

public class CreditsTrigger : MonoBehaviour
{
    [Header("Credits Settings")]
    public GameObject creditsPanel;
    public float fadeDuration = 2f;
    public CanvasGroup fadeCanvasGroup;
    
    private bool creditsTriggered = false;
    private PlayerController playerController;
    private ThirdPersonCamera cameraController;

    private void Start()
    {
        playerController = FindFirstObjectByType<PlayerController>();
        cameraController = FindFirstObjectByType<ThirdPersonCamera>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (creditsTriggered) return;
        
        if (other.CompareTag("Player"))
        {
            creditsTriggered = true;
            StartCoroutine(StartCreditsSequence());
        }
    }

    private IEnumerator StartCreditsSequence()
    {
        // Deshabilitar controles
        if (playerController != null)
            playerController.enabled = false;

        if (cameraController != null)
            cameraController.enabled = false;

        // Desbloquear cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Difuminar a blanco
        yield return StartCoroutine(FadeToWhite());

        // Mostrar créditos
        if (creditsPanel != null)
        {
            creditsPanel.SetActive(true);
        }
    }

    private IEnumerator FadeToWhite()
    {
        if (fadeCanvasGroup == null) yield break;

        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            fadeCanvasGroup.alpha = alpha;
            yield return null;
        }
        
        fadeCanvasGroup.alpha = 1f;
    }
}