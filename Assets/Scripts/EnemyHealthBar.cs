using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    private Canvas canvas;
    private Image healthFill;
    
    public void Initialize()
    {
        // Create canvas
        GameObject canvasObj = new GameObject("HealthBarCanvas");
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        
        // Add canvas to enemy without making it inherit rigidbody physics
        canvasObj.transform.SetParent(transform, false);
        canvasObj.transform.localPosition = new Vector3(0, 2, 0);
        canvasObj.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        
        // Create health bar background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(canvas.transform, false);
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = Color.black;
        RectTransform bgRect = bgImage.rectTransform;
        bgRect.sizeDelta = new Vector2(100, 10);
        
        // Create health bar fill
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(bgRect, false);
        healthFill = fillObj.AddComponent<Image>();
        healthFill.color = Color.green;
        RectTransform fillRect = healthFill.rectTransform;
        fillRect.sizeDelta = new Vector2(100, 10);
        fillRect.anchorMin = new Vector2(0, 0);
        fillRect.anchorMax = new Vector2(1, 1);
        fillRect.sizeDelta = Vector2.zero;
    }
    
    public void UpdateHealthBar(float health, float maxHealth)
    {
        if (healthFill != null)
        {
            healthFill.fillAmount = health / maxHealth;
            healthFill.color = Color.Lerp(Color.red, Color.green, health / maxHealth);
        }
    }
    
    void Update()
    {
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