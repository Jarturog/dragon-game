using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    private Canvas canvas;
    private Image healthFill;
    private RectTransform fillRect;
    
    [Header("Health Bar Settings")]
    public float heightOffset = 0.5f; // Additional offset above the enemy
    public bool useRendererBounds = true; // Use renderer bounds vs collider bounds
    
    public void Initialize()
    {
        // Create canvas
        GameObject canvasObj = new GameObject("HealthBarCanvas");
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        
        // Configure canvas as child of enemy
        canvasObj.transform.SetParent(transform, false);
        
        // Calculate dynamic position based on enemy height
        Vector3 healthBarPosition = CalculateHealthBarPosition();
        canvasObj.transform.localPosition = healthBarPosition;
        canvasObj.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        
        // Create health bar directly in canvas
        GameObject fillObj = new GameObject("HealthFill");
        fillObj.transform.SetParent(canvas.transform, false);
        
        // Configure RectTransform
        fillRect = fillObj.AddComponent<RectTransform>();
        fillRect.sizeDelta = new Vector2(100, 10);
        fillRect.pivot = new Vector2(0.5f, 0.5f);
        fillRect.anchorMin = new Vector2(0.5f, 0.5f);
        fillRect.anchorMax = new Vector2(0.5f, 0.5f);
        fillRect.anchoredPosition = Vector2.zero;
        
        // Configure health bar image
        healthFill = fillObj.AddComponent<Image>();
        healthFill.color = Color.green;
    }
    
    private Vector3 CalculateHealthBarPosition()
    {
        float enemyHeight = GetEnemyHeight();
        return new Vector3(0, enemyHeight + heightOffset, 0);
    }
    
    private float GetEnemyHeight()
    {
        float height = 2f; // Default fallback height
        
        if (useRendererBounds)
        {
            // Method 1: Use Renderer bounds (most accurate for visual representation)
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                height = renderer.bounds.size.y;
            }
            else
            {
                // Fallback to checking child renderers
                Renderer[] childRenderers = GetComponentsInChildren<Renderer>();
                if (childRenderers.Length > 0)
                {
                    Bounds combinedBounds = childRenderers[0].bounds;
                    foreach (Renderer r in childRenderers)
                    {
                        combinedBounds.Encapsulate(r.bounds);
                    }
                    height = combinedBounds.size.y;
                }
            }
        }
        else
        {
            // Method 2: Use Collider bounds
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                height = col.bounds.size.y;
            }
        }
        
        return height;
    }
    
    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthFill != null && fillRect != null)
        {
            float healthPercent = currentHealth / maxHealth;
            
            // Reduce RectTransform width
            fillRect.sizeDelta = new Vector2(100 * healthPercent, 10);
            
            // Change color gradually
            healthFill.color = Color.Lerp(Color.red, Color.green, healthPercent);
        }
    }
    
    void Update()
    {
        // Rotate canvas to look at camera
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