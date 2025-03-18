using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    
    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float currentStamina;
    public float staminaRegenRate = 10f;
    public float runStaminaCost = 15f;
    public float jumpStaminaCost = 10f;
    public float attackStaminaCost = 5f;
    
    private PlayerController playerController;
    private PlayerHealthUI healthUI;
    
    void Start()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        playerController = GetComponent<PlayerController>();
        
        // Create health UI
        healthUI = new PlayerHealthUI();
    }
    
    void Update()
    {
        // Regenerate stamina when not running
        if (!playerController.isRunning)
        {
            RegenerateStamina();
        }
    }
    
    public void TakeDamage(float damage)
    {
        if (currentHealth > 0) {
            currentHealth -= damage;
            // Ensure health doesn't go below zero
            currentHealth = Mathf.Max(0, currentHealth);
        
            healthUI.UpdateHealthBar(currentHealth / maxHealth);
        
            Debug.Log("Player took " + damage + " damage. Remaining health: " + currentHealth);
        
            if (currentHealth <= 0)
            {
                Die();
            }
        }
        else {
            Debug.Log("Player is already dead");
        }
    }
    
    public bool UseStamina(float amount)
    {
        if (currentStamina >= amount)
        {
            currentStamina -= amount;
            healthUI.UpdateStaminaBar(currentStamina / maxStamina);
            return true;
        }
        return false;
    }
    
    private void RegenerateStamina()
    {
        if (currentStamina < maxStamina)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            currentStamina = Mathf.Min(currentStamina, maxStamina);
            healthUI.UpdateStaminaBar(currentStamina / maxStamina);
        }
    }
    
    private void Die()
    {
        Debug.Log("Player has died!");
    
        // Hide the health UI
        healthUI.HideUI();
    
        // Show the death screen
        DeathScreen.ShowDeathScreen();
    
        // Disable this script to prevent further damage
        enabled = false;
    }
    
    // UI class for health and stamina bars
    class PlayerHealthUI
    {
        private GameObject healthBarContainer;
        private GameObject healthBarBackground;
        private GameObject healthBarFill;
        
        private GameObject staminaBarContainer;
        private GameObject staminaBarBackground;
        private GameObject staminaBarFill;
        
        private Canvas uiCanvas;
        
        public PlayerHealthUI()
        {
            // Create Canvas for UI elements
            GameObject canvasObject = new GameObject("PlayerHealthCanvas");
            uiCanvas = canvasObject.AddComponent<Canvas>();
            uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
            
            // Create health bar container in top right corner
            healthBarContainer = CreateUIContainer("HealthBarContainer", new Vector2(-20, -20), new Vector2(200, 20));
            
            // Create health bar background (black)
            healthBarBackground = CreateUIElement("HealthBarBackground", healthBarContainer.transform, Color.black, new Vector2(0, 0));
            
            // Create health bar fill (green)
            healthBarFill = CreateUIElement("HealthBarFill", healthBarBackground.transform, Color.green, new Vector2(0, 0));
            
            // Create stamina bar container below health bar
            staminaBarContainer = CreateUIContainer("StaminaBarContainer", new Vector2(-20, -45), new Vector2(200, 20));
            
            // Create stamina bar background (black)
            staminaBarBackground = CreateUIElement("StaminaBarBackground", staminaBarContainer.transform, Color.black, new Vector2(0, 0));
            
            // Create stamina bar fill (blue)
            staminaBarFill = CreateUIElement("StaminaBarFill", staminaBarBackground.transform, Color.blue, new Vector2(0, 0));
        }
        
        private GameObject CreateUIContainer(string name, Vector2 position, Vector2 size)
        {
            GameObject container = new GameObject(name);
            container.transform.SetParent(uiCanvas.transform, false);
            
            RectTransform rect = container.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(1, 1);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            
            return container;
        }
        
        private GameObject CreateUIElement(string name, Transform parent, Color color, Vector2 position)
        {
            GameObject element = new GameObject(name);
            element.transform.SetParent(parent, false);
            
            RectTransform rect = element.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = Vector2.zero;
            
            Image image = element.AddComponent<Image>();
            image.color = color;
            
            return element;
        }
        
        public void UpdateHealthBar(float healthPercent)
        {
            if (healthBarFill != null)
            {
                // Clamp health percent between 0 and 1
                healthPercent = Mathf.Clamp01(healthPercent);
                
                // Get the RectTransform
                RectTransform rect = healthBarFill.GetComponent<RectTransform>();
                
                // Change width to represent current health (reducing from left to right)
                rect.offsetMin = new Vector2((1 - healthPercent) * rect.parent.GetComponent<RectTransform>().rect.width, rect.offsetMin.y);
                
                // Update color based on health percentage
                Image image = healthBarFill.GetComponent<Image>();
                image.color = Color.Lerp(Color.red, Color.green, healthPercent);
            }
        }

        public void UpdateStaminaBar(float staminaPercent)
        {
            if (staminaBarFill != null)
            {
                // Clamp stamina percent between 0 and 1
                staminaPercent = Mathf.Clamp01(staminaPercent);
                
                // Get the RectTransform
                RectTransform rect = staminaBarFill.GetComponent<RectTransform>();
                
                // Change width to represent current stamina (reducing from left to right)
                rect.offsetMin = new Vector2((1 - staminaPercent) * rect.parent.GetComponent<RectTransform>().rect.width, rect.offsetMin.y);
                
                // Update color based on stamina percentage
                Image image = staminaBarFill.GetComponent<Image>();
                image.color = Color.Lerp(Color.gray, Color.blue, staminaPercent);
            }
        }
        
        public void HideUI()
        {
            if (healthBarContainer != null)
                healthBarContainer.SetActive(false);
    
            if (staminaBarContainer != null)
                staminaBarContainer.SetActive(false);
        }
    }
}