using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEditor.VersionControl;

public class DeathScreen : MonoBehaviour
{
    // Static reference to the instance
    private static DeathScreen instance;
    
    // References will be assigned at runtime
    private CanvasGroup canvasGroup;
    private Text youDiedText;
    
    // Animation settings
    private float fadeInDuration = 2f;
    private float textDelayTime = 1f;
    private float textFadeInDuration = 3f;
    private float restartDelayTime = 5f;
    
    // Call this from PlayerHealth.Die()
    public static void ShowDeathScreen()
    {
        // Create the death screen if it doesn't exist
        if (instance == null)
        {
            GameObject deathScreenObj = new GameObject("DeathScreen");
            instance = deathScreenObj.AddComponent<DeathScreen>();
            instance.CreateDeathScreenUI();
        }
        
        // Show the death screen
        instance.StartCoroutine(instance.ShowDeathSequence());
    }
    
    private void CreateDeathScreenUI()
    {
        // Create canvas
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        
        // Add canvas scaler
        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        // Add canvas group
        canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
        
        // Create black background
        GameObject panel = new GameObject("BlackPanel");
        panel.transform.SetParent(transform, false);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = Color.black;
        
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        
        // Create text
        GameObject textObj = new GameObject("YouDiedText");
        textObj.transform.SetParent(transform, false);
        youDiedText = textObj.AddComponent<Text>();
        youDiedText.text = "HAS MUERTO";
        youDiedText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        youDiedText.fontSize = 72;
        youDiedText.alignment = TextAnchor.MiddleCenter;
        youDiedText.color = new Color(0.8f, 0.1f, 0.1f, 0);
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        
        // Don't destroy on scene load
        DontDestroyOnLoad(gameObject);
    }
    
    private IEnumerator ShowDeathSequence()
    {
        
        // Disable player controls
        PlayerController playerController = FindFirstObjectByType<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
        // Fade in background
        float elapsedTime = 0;
        while (elapsedTime < fadeInDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(0, 1, elapsedTime / fadeInDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 1;
        
        // Wait before showing text
        yield return new WaitForSeconds(textDelayTime);
        
        // play sound
        AudioManager.Instance.PlaySFX("DeathSound");
        
        // Fade in text
        elapsedTime = 0;
        while (elapsedTime < textFadeInDuration)
        {
            float alpha = Mathf.Lerp(0, 1, elapsedTime / textFadeInDuration);
            youDiedText.color = new Color(youDiedText.color.r, youDiedText.color.g, youDiedText.color.b, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        youDiedText.color = new Color(youDiedText.color.r, youDiedText.color.g, youDiedText.color.b, 1);
        
        // Wait before restarting
        yield return new WaitForSeconds(restartDelayTime);
        
        // Volver al menú principal en lugar de reiniciar la escena
        MainMenuManager.ReturnToMainMenu();
        
        // Destruir la pantalla de muerte
        Destroy(gameObject);
    }
}