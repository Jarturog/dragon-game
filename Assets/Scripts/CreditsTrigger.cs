using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class CreditsTrigger : MonoBehaviour
{
    [Header("Settings")]
    public float fadeDuration = 2f;
    public float timeBetweenCredits = 2.5f;
    public Font textFont;
    public Material particleMaterial;

    private bool creditsTriggered = false;
    private PlayerController playerController;
    private ThirdPersonCamera cameraController;

    private Canvas canvas;
    private CanvasGroup fadeCanvasGroup;
    private GameObject creditsPanel;
    private List<GameObject> creditLines = new List<GameObject>();
    
    private readonly List<KeyValuePair<string, int>> creditEntries = new List<KeyValuePair<string, int>>
    {
        new KeyValuePair<string, int>("MOON'S SHADOW", 36),
        new KeyValuePair<string, int>("The Last Flame", 36),
        new KeyValuePair<string, int>("", 16),
        new KeyValuePair<string, int>("Informática gráfica 2024-2025", 14),
        new KeyValuePair<string, int>("Juan Arturo Abaurrea Calafell: Encargado del videojuego en Unity", 14),
        new KeyValuePair<string, int>("Antoni Navarro Moreno: Encargado del coliseo", 14),
        new KeyValuePair<string, int>("Laura Rodríguez López: Encargada del dragón protagonista", 14),
        new KeyValuePair<string, int>("Lucas Sabater Margarit: Encargado de los enemigos slime y esqueleto", 14),
        new KeyValuePair<string, int>("Hugo Valls Sabater: Encargado del jefe final enemigo golem", 14),
        new KeyValuePair<string, int>("", 16),
        new KeyValuePair<string, int>("¡Gracias por jugar!", 16)
    };

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
        // Setup UI when credits are triggered
        SetupUI();
        
        if (playerController != null)
            playerController.enabled = false;

        if (cameraController != null)
            cameraController.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        yield return StartCoroutine(FadeToWhite());
        yield return StartCoroutine(ShowCredits());
    }

    private IEnumerator FadeToWhite()
    {
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

    private IEnumerator ShowCredits()
    {
        creditsPanel.SetActive(true);

        foreach (GameObject credit in creditLines)
        {
            credit.SetActive(true);
            var ps = credit.GetComponentInChildren<ParticleSystem>();
            if (ps != null) ps.Play();

            yield return new WaitForSeconds(timeBetweenCredits);
        }
    }

    private void SetupUI()
    {
        // Canvas
        GameObject canvasObj = new GameObject("CreditsCanvas");
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.AddComponent<GraphicRaycaster>();

        // Fade panel
        GameObject fadePanel = new GameObject("FadePanel", typeof(Image), typeof(CanvasGroup));
        fadePanel.transform.SetParent(canvas.transform, false);
        RectTransform fadeRT = fadePanel.GetComponent<RectTransform>();
        fadeRT.anchorMin = Vector2.zero;
        fadeRT.anchorMax = Vector2.one;
        fadeRT.offsetMin = Vector2.zero;
        fadeRT.offsetMax = Vector2.zero;
        fadePanel.GetComponent<Image>().color = Color.white;
        fadeCanvasGroup = fadePanel.GetComponent<CanvasGroup>();
        fadeCanvasGroup.alpha = 0f;

        // Credits panel
        creditsPanel = new GameObject("CreditsPanel");
        creditsPanel.transform.SetParent(canvas.transform, false);
        RectTransform panelRT = creditsPanel.AddComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;
        creditsPanel.SetActive(false);

        // Crear líneas de texto usando creditEntries
        for (int i = 0; i < creditEntries.Count; i++)
        {
            GameObject creditLine = CreateCreditLine(creditEntries[i].Key, creditEntries[i].Value, i);
            creditLine.SetActive(false);
            creditLines.Add(creditLine);
        }
    }

    private GameObject CreateCreditLine(string text, int fontSize, int index)
    {
        GameObject go = new GameObject($"CreditLine_{index}");
        go.transform.SetParent(creditsPanel.transform, false);

        // Texto
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize; // Usar el tamaño de fuente específico
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.black;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(1600, 100);
        rt.anchoredPosition = new Vector2(0, 250 - index * 80);

        // Partículas
        GameObject psObj = new GameObject("Particles");
        psObj.transform.SetParent(go.transform, false);
        ParticleSystem ps = psObj.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.duration = 1.5f;
        main.startLifetime = 1f;
        main.startSpeed = 0.1f;
        main.startSize = 0.1f;
        main.loop = false;
        main.playOnAwake = false;

        var emission = ps.emission;
        emission.rateOverTime = 50;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Rectangle;
        shape.scale = new Vector3(10f, 1f, 1f);

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = particleMaterial;

        return go;
    }
}