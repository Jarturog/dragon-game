using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    private Canvas canvas;
    private Image healthFill;
    private RectTransform fillRect; // Referencia al RectTransform de la barra

    public void Initialize()
    {
        // Create canvas
        GameObject canvasObj = new GameObject("HealthBarCanvas");
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        
        // Configurar canvas como hijo del enemigo
        canvasObj.transform.SetParent(transform, false);
        canvasObj.transform.localPosition = new Vector3(0, 2, 0);
        canvasObj.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        
        // Crear barra de vida directamente en el canvas
        GameObject fillObj = new GameObject("HealthFill");
        fillObj.transform.SetParent(canvas.transform, false);
        
        // Configurar RectTransform
        fillRect = fillObj.AddComponent<RectTransform>();
        fillRect.sizeDelta = new Vector2(100, 10); // Ancho inicial completo
        fillRect.pivot = new Vector2(0.5f, 0.5f); // Pivote en el extremo derecho
        fillRect.anchorMin = new Vector2(0.5f, 0.5f);
        fillRect.anchorMax = new Vector2(0.5f, 0.5f);
        fillRect.anchoredPosition = Vector2.zero; // Centrado en el canvas
        
        // Configurar imagen de la barra
        healthFill = fillObj.AddComponent<Image>();
        healthFill.color = Color.green;
        
        
    }
    
    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthFill != null && fillRect != null)
        {
            float healthPercent = currentHealth / maxHealth;
            
            // Reducir la anchura del RectTransform
            fillRect.sizeDelta = new Vector2(100 * healthPercent, 10);
            
            // Cambiar color gradualmente
            healthFill.color = Color.Lerp(Color.red, Color.green, healthPercent);
        }
    }
    
    void Update()
    {
        // Rotar el canvas para mirar a la cámara
        if (canvas != null && Camera.main != null)
        {
            canvas.transform.rotation = Camera.main.transform.rotation;
        }
    }
    
    void OnDestroy()
    {
        if (canvas != null)
        {
            Destroy(canvas.gameObject);
        }
    }
}