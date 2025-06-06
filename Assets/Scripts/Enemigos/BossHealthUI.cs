using UnityEngine;
using UnityEngine.UI;

public class BossHealthUI
{
    private GameObject bossHealthContainer;
    private Image bossHealthFill;
    private Text bossNameText;
    private BossEnemy boss;
    private RectTransform fillRect; // Add reference to the fill RectTransform
    private float maxHealthBarWidth = 600f; // Store the maximum width
    
    public BossHealthUI(Canvas parentCanvas, BossEnemy bossEnemy)
    {
        this.boss = bossEnemy;
        CreateBossHealthBar(parentCanvas);
    }
    
    private void CreateBossHealthBar(Canvas parentCanvas)
    {
        // Create main container
        bossHealthContainer = new GameObject("BossHealthContainer");
        bossHealthContainer.transform.SetParent(parentCanvas.transform, false);
        
        RectTransform containerRect = bossHealthContainer.AddComponent<RectTransform>();
        
        // Position at center bottom
        containerRect.anchorMin = new Vector2(0.5f, 0f);
        containerRect.anchorMax = new Vector2(0.5f, 0f);
        containerRect.pivot = new Vector2(0.5f, 0f);
        containerRect.anchoredPosition = new Vector2(0f, 50f); // 50 pixels from bottom
        containerRect.sizeDelta = new Vector2(maxHealthBarWidth, 60f);
        
        // Create boss name text
        GameObject nameTextObj = new GameObject("BossName");
        nameTextObj.transform.SetParent(bossHealthContainer.transform, false);
        
        RectTransform nameRect = nameTextObj.AddComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0f, 1f);
        nameRect.anchorMax = new Vector2(1f, 2f);
        nameRect.offsetMin = Vector2.zero;
        nameRect.offsetMax = Vector2.zero;
        
        bossNameText = nameTextObj.AddComponent<Text>();
        bossNameText.text = "Tarmox Oathbound Devourer";
        bossNameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        bossNameText.fontSize = 36;
        bossNameText.color = Color.red;
        bossNameText.alignment = TextAnchor.MiddleCenter;
        bossNameText.fontStyle = FontStyle.Bold;
        
        // Create health bar background
        GameObject healthBgObj = new GameObject("BossHealthBackground");
        healthBgObj.transform.SetParent(bossHealthContainer.transform, false);
        
        RectTransform bgRect = healthBgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0f, 0f);
        bgRect.anchorMax = new Vector2(1f, 0.6f);
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        
        Image healthBg = healthBgObj.AddComponent<Image>();
        healthBg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        // Create health bar fill
        GameObject healthFillObj = new GameObject("BossHealthFill");
        healthFillObj.transform.SetParent(healthBgObj.transform, false);
    
        fillRect = healthFillObj.AddComponent<RectTransform>(); // Store reference
        // Change anchoring to left-aligned so it shrinks from right to left
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(0f, 1f);
        fillRect.pivot = new Vector2(0f, 0.5f); // Pivot on the left side
        fillRect.offsetMin = new Vector2(2f, 2f); // Small padding
        fillRect.offsetMax = new Vector2(2f, -2f);
        // Set initial size (full width minus padding)
        fillRect.sizeDelta = new Vector2(maxHealthBarWidth - 4f, 0f);
    
        bossHealthFill = healthFillObj.AddComponent<Image>();
        bossHealthFill.color = Color.green; // Start with green
        // Remove the filled image type settings since we're using size changes
        // bossHealthFill.type = Image.Type.Filled;
        // bossHealthFill.fillMethod = Image.FillMethod.Horizontal;
        // bossHealthFill.fillAmount = 1f;
    
        // Add pattern effect
        PatternGenerator patternGen = bossHealthFill.gameObject.AddComponent<PatternGenerator>();
        patternGen.patternScale = 6f;
        patternGen.patternOpacity = 0.15f;
    }
    
    public void UpdateBossHealthBar(float healthPercent)
    {
        if (bossHealthFill != null && fillRect != null)
        {
            healthPercent = Mathf.Clamp01(healthPercent);
        
            // Update the size instead of fillAmount
            float currentWidth = (maxHealthBarWidth - 4f) * healthPercent; // Subtract padding
            fillRect.sizeDelta = new Vector2(currentWidth, fillRect.sizeDelta.y);
        
            // Update color from green to red as health decreases
            bossHealthFill.color = Color.Lerp(Color.red, Color.green, healthPercent);
        }
    }
    
    public void HideBossHealthBar()
    {
        if (bossHealthContainer != null)
        {
            bossHealthContainer.SetActive(false);
        }
    }
    
    public void ShowBossHealthBar()
    {
        if (bossHealthContainer != null)
        {
            bossHealthContainer.SetActive(true);
        }
    }
    
    public void DestroyBossHealthBar()
    {
        if (bossHealthContainer != null)
        {
            Object.Destroy(bossHealthContainer);
        }
    }
}