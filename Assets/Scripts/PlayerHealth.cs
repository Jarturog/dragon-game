using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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
    
    [Header("Auto Healing")]
    public float autoHealDelay = 5f;         // Tiempo sin recibir daño antes de comenzar a curarse
    public float autoHealRate = 2f;          // Cantidad de salud recuperada por segundo
    private float lastDamageTime;            // Momento en que se recibió el último daño
    private bool isAutoHealing = false;      // Indica si la curación automática está activa
    
    private PlayerController playerController;
    private PlayerHealthUI healthUI;
    
    void Start()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        playerController = GetComponent<PlayerController>();
        
        // Inicializar el tiempo del último daño
        lastDamageTime = -autoHealDelay;  // Para permitir curación desde el inicio si no hay daño
        
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
        
        // Verificar si debe comenzar la curación automática
        CheckAutoHeal();
    }
    
    private void CheckAutoHeal()
    {
        // Si ha pasado suficiente tiempo desde el último daño
        if (Time.time - lastDamageTime >= autoHealDelay)
        {
            // Si no estamos sanando y la salud no está al máximo, activar indicador
            if (!isAutoHealing && currentHealth < maxHealth)
            {
                isAutoHealing = true;
                Debug.Log("Auto healing activated");
            }
        
            // Aplicar curación si la salud no está al máximo
            if (currentHealth < maxHealth)
            {
                float healAmount = autoHealRate * Time.deltaTime;
                currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
                healthUI.UpdateHealthBar(currentHealth / maxHealth);
            
                // Si llegamos a la salud máxima, desactivar la curación automática
                if (currentHealth >= maxHealth)
                {
                    isAutoHealing = false;
                    Debug.Log("Auto healing completed - health is full");
                }
            }
        }
        else
        {
            // Si estábamos sanando y ahora no, desactivar indicador
            if (isAutoHealing)
            {
                isAutoHealing = false;
                Debug.Log("Auto healing deactivated");
            }
        }
    }
    
    public void TakeDamage(float damage)
    {
        if (currentHealth > 0) {
            // Registrar el momento del daño para reiniciar el temporizador de curación
            lastDamageTime = Time.time;
            
            // Desactivar la curación automática
            isAutoHealing = false;
            
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
    public class PlayerHealthUI
    {
        private GameObject healthBarContainer;
        private GameObject healthBarBackground;
        private GameObject healthBarFill;
        private GameObject healthBarBorder;
        private GameObject healthBarHighlight;
        private GameObject healthBarSegments;
        
        private GameObject staminaBarContainer;
        private GameObject staminaBarBackground;
        private GameObject staminaBarFill;
        private GameObject staminaBarBorder;
        private GameObject staminaBarHighlight;
        private GameObject staminaBarPulse;
        
        private Canvas uiCanvas;
        private float pulseTime = 0f;
        
        // Variables para controlar el estado de los efectos
        private bool isHealthCritical = false;
        private bool isStaminaCritical = false;
        
        public PlayerHealthUI()
        {
            // Create Canvas for UI elements
            GameObject canvasObject = new GameObject("PlayerHealthCanvas");
            uiCanvas = canvasObject.AddComponent<Canvas>();
            uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            // Add canvas scaler for proper resolution handling
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObject.AddComponent<GraphicRaycaster>();
            
            // Create root container for all UI
            GameObject uiContainer = new GameObject("UIContainer");
            uiContainer.transform.SetParent(uiCanvas.transform, false);
            RectTransform containerRect = uiContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = Vector2.zero;
            containerRect.anchorMax = Vector2.one;
            containerRect.offsetMin = Vector2.zero;
            containerRect.offsetMax = Vector2.zero;
            
            // Create health bar container in top right corner
            healthBarContainer = CreateUIContainer("HealthBarContainer", new Vector2(-20, -20), new Vector2(200, 25));
            healthBarContainer.transform.SetParent(uiContainer.transform, false);
            
            // Create drop shadow effect
            GameObject healthDropShadow = CreateUIElement("HealthDropShadow", healthBarContainer.transform, new Color(0, 0, 0, 0.5f), new Vector2(3, -3));
            RectTransform shadowRect = healthDropShadow.GetComponent<RectTransform>();
            shadowRect.sizeDelta = new Vector2(6, 6);
            
            // Create health bar background with gradient
            healthBarBackground = CreateUIElement("HealthBarBackground", healthBarContainer.transform, Color.black, Vector2.zero);
            AddGradientEffect(healthBarBackground, new Color(0.1f, 0.1f, 0.1f), new Color(0.2f, 0.2f, 0.2f));
            
            // Create health bar fill with texture effect
            healthBarFill = CreateUIElement("HealthBarFill", healthBarBackground.transform, Color.green, Vector2.zero);
            AddTextureEffect(healthBarFill);
            
            // Create health bar segments
            healthBarSegments = CreateSegments("HealthBarSegments", healthBarFill.transform, 10, new Color(0, 0, 0, 0.15f));
            
            // Create highlight at the top of the health bar
            healthBarHighlight = CreateUIElement("HealthBarHighlight", healthBarFill.transform, new Color(1, 1, 1, 0.3f), new Vector2(0, 0));
            RectTransform highlightRect = healthBarHighlight.GetComponent<RectTransform>();
            highlightRect.anchorMin = new Vector2(0, 0.8f);
            highlightRect.anchorMax = new Vector2(1, 1);
            highlightRect.offsetMin = Vector2.zero;
            highlightRect.offsetMax = Vector2.zero;
            
            // Create border for health bar
            healthBarBorder = CreateBorder("HealthBarBorder", healthBarBackground.transform, 2f, new Color(0.3f, 0.3f, 0.3f));
            
            // Create stamina bar container below health bar
            staminaBarContainer = CreateUIContainer("StaminaBarContainer", new Vector2(-20, -50), new Vector2(200, 20));
            staminaBarContainer.transform.SetParent(uiContainer.transform, false);
            
            // Create drop shadow effect for stamina bar
            GameObject staminaDropShadow = CreateUIElement("StaminaDropShadow", staminaBarContainer.transform, new Color(0, 0, 0, 0.5f), new Vector2(3, -3));
            RectTransform staminaShadowRect = staminaDropShadow.GetComponent<RectTransform>();
            staminaShadowRect.sizeDelta = new Vector2(6, 6);
            
            // Create stamina bar background with gradient
            staminaBarBackground = CreateUIElement("StaminaBarBackground", staminaBarContainer.transform, Color.black, Vector2.zero);
            AddGradientEffect(staminaBarBackground, new Color(0.1f, 0.1f, 0.15f), new Color(0.15f, 0.15f, 0.2f));
            
            // Create stamina bar fill with texture effect
            staminaBarFill = CreateUIElement("StaminaBarFill", staminaBarBackground.transform, Color.blue, Vector2.zero);
            AddTextureEffect(staminaBarFill);
            
            // Create pulse effect overlay - inicialmente transparente
            staminaBarPulse = CreateUIElement("StaminaBarPulse", staminaBarFill.transform, new Color(1f, 1f, 1f, 0f), Vector2.zero);
            
            // Create highlight at the top of the stamina bar
            staminaBarHighlight = CreateUIElement("StaminaBarHighlight", staminaBarFill.transform, new Color(1, 1, 1, 0.3f), new Vector2(0, 0));
            RectTransform staminaHighlightRect = staminaBarHighlight.GetComponent<RectTransform>();
            staminaHighlightRect.anchorMin = new Vector2(0, 0.8f);
            staminaHighlightRect.anchorMax = new Vector2(1, 1);
            staminaHighlightRect.offsetMin = Vector2.zero;
            staminaHighlightRect.offsetMax = Vector2.zero;
            
            // Create border for stamina bar
            staminaBarBorder = CreateBorder("StaminaBarBorder", staminaBarBackground.transform, 1.5f, new Color(0.3f, 0.3f, 0.5f));
            
            // Start the animation coroutine for pulse effects
            GameObject coroutineHandler = new GameObject("UIAnimationHandler");
            coroutineHandler.transform.SetParent(canvasObject.transform);
            UIAnimationHandler animHandler = coroutineHandler.AddComponent<UIAnimationHandler>();
            animHandler.Initialize(this);
        }
        
        private GameObject CreateUIContainer(string name, Vector2 position, Vector2 size)
        {
            GameObject container = new GameObject(name);
            
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
        
        private GameObject CreateBorder(string name, Transform parent, float thickness, Color color)
        {
            GameObject border = new GameObject(name);
            border.transform.SetParent(parent, false);
            
            RectTransform rect = border.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(1, 1);
            rect.offsetMin = new Vector2(-thickness, -thickness);
            rect.offsetMax = new Vector2(thickness, thickness);
            
            // Use the outline image effect for the border
            Outline outline = border.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = new Vector2(thickness, thickness);
            
            return border;
        }
        
        private GameObject CreateSegments(string name, Transform parent, int segmentCount, Color color)
        {
            GameObject segmentsContainer = new GameObject(name);
            segmentsContainer.transform.SetParent(parent, false);
            
            RectTransform containerRect = segmentsContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0, 0);
            containerRect.anchorMax = new Vector2(1, 1);
            containerRect.offsetMin = Vector2.zero;
            containerRect.offsetMax = Vector2.zero;
            
            float segmentWidth = 1f / segmentCount;
            
            for (int i = 1; i < segmentCount; i++)
            {
                GameObject segment = new GameObject("Segment" + i);
                segment.transform.SetParent(segmentsContainer.transform, false);
                
                RectTransform rect = segment.AddComponent<RectTransform>();
                rect.anchorMin = new Vector2(segmentWidth * i, 0);
                rect.anchorMax = new Vector2(segmentWidth * i + 0.005f, 1);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                
                Image image = segment.AddComponent<Image>();
                image.color = color;
            }
            
            return segmentsContainer;
        }
        
        private void AddGradientEffect(GameObject target, Color topColor, Color bottomColor)
        {
            Image image = target.GetComponent<Image>();
            if (image != null)
            {
                // Create a vertical gradient
                Gradient gradient = new Gradient();
                gradient.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(bottomColor, 0.0f), new GradientColorKey(topColor, 1.0f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) }
                );
                
                // Add a custom material with shader to apply the gradient
                // Since we can't directly use shaders, we'll use a workaround with UI Vertex helper
                VerticalGradient vertGradient = target.AddComponent<VerticalGradient>();
                vertGradient.topColor = topColor;
                vertGradient.bottomColor = bottomColor;
            }
        }
        
        private void AddTextureEffect(GameObject target)
        {
            // Add a custom material with a noise texture effect
            // We'll use a procedural pattern generator
            PatternGenerator patternGen = target.AddComponent<PatternGenerator>();
            patternGen.patternScale = 4f;
            patternGen.patternOpacity = 0.1f;
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
                
                // Verifica si la salud está en estado crítico
                bool shouldBeCritical = healthPercent < 0.25f;
                
                // Solo actualizamos el estado si hay un cambio
                if (shouldBeCritical != isHealthCritical)
                {
                    isHealthCritical = shouldBeCritical;
                    
                    // Si la salud ya no es crítica, restablecemos el color normal con la opacidad completa
                    if (!isHealthCritical)
                    {
                        image.color = Color.Lerp(Color.red, Color.green, healthPercent);
                    }
                }
                else
                {
                    // Actualización regular del color basado en el porcentaje de salud
                    if (!isHealthCritical)
                    {
                        image.color = Color.Lerp(Color.red, Color.green, healthPercent);
                    }
                }
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
                image.color = Color.Lerp(new Color(0.5f, 0.5f, 0.8f), new Color(0.2f, 0.4f, 1f), staminaPercent);
                
                // Verifica si la estamina está en estado crítico
                bool shouldBeCritical = staminaPercent < 0.15f;
                
                // Solo actualizamos el estado si hay un cambio
                if (shouldBeCritical != isStaminaCritical)
                {
                    isStaminaCritical = shouldBeCritical;
                    
                    // Si ya no es crítico, hacemos el pulso completamente transparente
                    if (!isStaminaCritical)
                    {
                        Image pulseImage = staminaBarPulse.GetComponent<Image>();
                        pulseImage.color = new Color(1f, 1f, 1f, 0f);
                    }
                }
            }
        }
        
        public void UpdatePulseEffects()
        {
            // Actualizar el tiempo de pulso
            pulseTime += Time.deltaTime * 5f;
            
            // Solo actualizar el pulso de salud si está en estado crítico
            if (isHealthCritical && healthBarFill != null)
            {
                Image healthImage = healthBarFill.GetComponent<Image>();
                float alpha = Mathf.Lerp(0.8f, 1f, (Mathf.Sin(pulseTime) + 1f) * 0.5f);
                Color baseColor = Color.Lerp(Color.red, new Color(1f, 0.3f, 0.3f), (Mathf.Sin(pulseTime) + 1f) * 0.5f);
                healthImage.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
            }
            
            // Solo actualizar el pulso de estamina si está en estado crítico
            if (isStaminaCritical && staminaBarPulse != null)
            {
                Image pulseImage = staminaBarPulse.GetComponent<Image>();
                float alpha = Mathf.Lerp(0f, 0.3f, (Mathf.Sin(pulseTime * 1.5f) + 1f) * 0.5f);
                pulseImage.color = new Color(1f, 1f, 1f, alpha);
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

// This class handles the UI animations
public class UIAnimationHandler : MonoBehaviour
{
    private PlayerHealth.PlayerHealthUI healthUI;
    
    public void Initialize(PlayerHealth.PlayerHealthUI ui)
    {
        healthUI = ui;
    }
    
    void Update()
    {
        if (healthUI != null)
        {
            healthUI.UpdatePulseEffects();
        }
    }
}

// Custom component for vertical gradient effect
public class VerticalGradient : BaseMeshEffect
{
    public Color topColor = Color.white;
    public Color bottomColor = Color.black;
    
    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive())
            return;
            
        List<UIVertex> vertices = new List<UIVertex>();
        vh.GetUIVertexStream(vertices);
        
        float bottomY = vertices[0].position.y;
        float topY = vertices[0].position.y;
        
        // Find the top and bottom y positions
        for (int i = 1; i < vertices.Count; i++)
        {
            float y = vertices[i].position.y;
            if (y > topY)
                topY = y;
            else if (y < bottomY)
                bottomY = y;
        }
        
        float height = topY - bottomY;
        
        // Apply gradient colors based on y position
        for (int i = 0; i < vertices.Count; i++)
        {
            UIVertex vertex = vertices[i];
            float normalizedY = Mathf.Clamp01((vertex.position.y - bottomY) / height);
            vertex.color = Color.Lerp(bottomColor, topColor, normalizedY);
            vertices[i] = vertex;
        }
        
        vh.Clear();
        vh.AddUIVertexTriangleStream(vertices);
    }
}

// Custom component for generating pattern texture effect
public class PatternGenerator : BaseMeshEffect
{
    public float patternScale = 10f;
    public float patternOpacity = 0.2f;
    
    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive())
            return;
            
        List<UIVertex> vertices = new List<UIVertex>();
        vh.GetUIVertexStream(vertices);
        
        // Apply a procedural pattern to each vertex
        for (int i = 0; i < vertices.Count; i++)
        {
            UIVertex vertex = vertices[i];
            
            // Create a noise-like pattern based on position
            float x = vertex.position.x / patternScale;
            float y = vertex.position.y / patternScale;
            
            // Simple pattern formula (can be adjusted for different effects)
            float pattern = Mathf.Sin(x) * Mathf.Sin(y) * patternOpacity;
            
            // Apply pattern to vertex color
            Color baseColor = vertex.color;
            vertex.color = new Color(
                Mathf.Clamp01(baseColor.r + pattern),
                Mathf.Clamp01(baseColor.g + pattern),
                Mathf.Clamp01(baseColor.b + pattern),
                baseColor.a
            );
            
            vertices[i] = vertex;
        }
        
        vh.Clear();
        vh.AddUIVertexTriangleStream(vertices);
    }
}