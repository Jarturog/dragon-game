using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [Range(0f, 100f)]
    public float maxHealth = 100f;
    public float currentHealth;
    
    [Header("Stamina Settings")]
    [Range(0f, 100f)]
    public float maxStamina = 100f;
    public float currentStamina;
    public float staminaRegenRate = 10f;
    public float runStaminaCost = 15f;
    public float jumpStaminaCost = 10f;
    public float attackStaminaCost = 5f;
    
    [Header("Auto Healing")]
    public float autoHealDelay = 5f;
    public float autoHealRate = 2f;
    private float _lastDamageTime;
    private bool _isAutoHealing;
    
    [Header("UI References")]
    public Canvas uiCanvas;
    public GameObject healthBarContainer;
    public Image healthBarFill;
    
    public GameObject staminaBarContainer;
    public Image staminaBarFill;
    public Image staminaBarPulse;
    
    private PlayerController _playerController;
    [HideInInspector] public PlayerHealthUI healthUI;
    
    private bool _uiInitialized = false;
    
    void Start()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        _playerController = GetComponent<PlayerController>();
    
        _lastDamageTime = -autoHealDelay;
    
        // Initialize the UI
        InitializeUI();
    }
    
    void InitializeUI() {
        // If UI components are already assigned in inspector
        if (uiCanvas == null || healthBarFill == null || staminaBarFill == null) {
            Debug.LogError("Health UI canvas not set");
            return;
        }
        
        // Create UI wrapper to maintain compatibility with MainMenuManager
        healthUI = new PlayerHealthUI(this);
        _uiInitialized = true;
    }
    
    void Update()
    {
        if (!_uiInitialized)
            InitializeUI();
            
        // Regenerate stamina when not running
        if (_playerController != null && !_playerController.isRunning)
        {
            RegenerateStamina();
        }
        
        // Check auto heal
        CheckAutoHeal();
        
        healthUI.UpdatePulseEffects();
    }
    
    private void CheckAutoHeal()
    {
        // If enough time has passed since the last damage
        if (Time.time - _lastDamageTime >= autoHealDelay)
        {
            // If we're not healing and health isn't at max, activate indicator
            if (!_isAutoHealing && currentHealth < maxHealth)
            {
                _isAutoHealing = true;
                Debug.Log("Auto healing activated");
            }
        
            // Apply healing if health isn't at max
            if (currentHealth < maxHealth)
            {
                float healAmount = autoHealRate * Time.deltaTime;
                currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
                healthUI.UpdateHealthBar(currentHealth / maxHealth);
            
                // If we reach max health, deactivate auto healing
                if (currentHealth >= maxHealth)
                {
                    _isAutoHealing = false;
                    Debug.Log("Auto healing completed - health is full");
                }
            }
        }
        else
        {
            // If we were healing and now we're not, deactivate indicator
            if (_isAutoHealing)
            {
                _isAutoHealing = false;
                Debug.Log("Auto healing deactivated");
            }
        }
    }
    
    public void TakeDamage(float damage)
    {
        if (currentHealth > 0) {
            // Record the time of damage to reset the healing timer
            _lastDamageTime = Time.time;
            
            // Deactivate auto healing
            _isAutoHealing = false;
            
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
    
    // UI class for health and stamina bars - modified to work with either editor or code-created UI
    public class PlayerHealthUI
    {
        private GameObject healthBarContainer;
        private Image healthBarFill;
        
        private GameObject staminaBarContainer;
        private Image staminaBarFill;
        private Image staminaBarPulse;
        
        private float pulseTime = 0f;
        
        // Variables for controlling effect states
        private bool isHealthCritical = false;
        private bool isStaminaCritical = false;

        private GameObject coroutineHandler;
        
        // Constructor for editor-assigned UI
        public PlayerHealthUI(PlayerHealth health)
        {
            // Use UI components assigned in the inspector
            this.healthBarContainer = health.healthBarContainer;
            this.healthBarFill = health.healthBarFill;
            
            this.staminaBarContainer = health.staminaBarContainer;
            this.staminaBarFill = health.staminaBarFill;
            this.staminaBarPulse = health.staminaBarPulse;
            
            // Setup special effects for the editor-assigned UI
            SetupEditorUIEffects();
        }
        
        private void SetupEditorUIEffects()
        {
            if (healthBarFill != null)
            {
                // Add texture effect to health bar
                if (healthBarFill.gameObject.GetComponent<PatternGenerator>() == null)
                {
                    PatternGenerator patternGen = healthBarFill.gameObject.AddComponent<PatternGenerator>();
                    patternGen.patternScale = 4f;
                    patternGen.patternOpacity = 0.1f;
                }
            }
            
            if (staminaBarFill != null)
            {
                // Add texture effect to stamina bar
                if (staminaBarFill.gameObject.GetComponent<PatternGenerator>() == null)
                {
                    PatternGenerator patternGen = staminaBarFill.gameObject.AddComponent<PatternGenerator>();
                    patternGen.patternScale = 4f;
                    patternGen.patternOpacity = 0.1f;
                }
            }
        }
        
        public void UpdateHealthBar(float healthPercent)
        {
            if (healthBarFill != null)
            {
                // Clamp health percent between 0 and 1
                healthPercent = Mathf.Clamp01(healthPercent);
                
                RectTransform rect = healthBarFill.GetComponent<RectTransform>();
                rect.offsetMin = new Vector2((1 - healthPercent) * rect.parent.GetComponent<RectTransform>().rect.width, rect.offsetMin.y);

                // Update color based on health percentage
                // Verifica si la salud está en estado crítico
                bool shouldBeCritical = healthPercent < 0.25f;
                
                // Solo actualizamos el estado si hay un cambio
                if (shouldBeCritical != isHealthCritical)
                {
                    isHealthCritical = shouldBeCritical;
                    
                    // Si la salud ya no es crítica, restablecemos el color normal con la opacidad completa
                    if (!isHealthCritical)
                    {
                        healthBarFill.color = Color.Lerp(Color.red, Color.green, healthPercent);
                    }
                }
                else
                {
                    // Actualización regular del color basado en el porcentaje de salud
                    if (!isHealthCritical)
                    {
                        healthBarFill.color = Color.Lerp(Color.red, Color.green, healthPercent);
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
                
                RectTransform rect = staminaBarFill.GetComponent<RectTransform>();
                rect.offsetMin = new Vector2((1 - staminaPercent) * rect.parent.GetComponent<RectTransform>().rect.width, rect.offsetMin.y);
                
                // Update color based on stamina percentage
                staminaBarFill.color = Color.Lerp(new Color(0.5f, 0.5f, 0.8f), new Color(0.2f, 0.4f, 1f), staminaPercent);
                
                // Verifica si la estamina está en estado crítico
                bool shouldBeCritical = staminaPercent < 0.15f;
                
                // Solo actualizamos el estado si hay un cambio
                if (shouldBeCritical != isStaminaCritical)
                {
                    isStaminaCritical = shouldBeCritical;
                    
                    // Si ya no es crítico, hacemos el pulso completamente transparente
                    if (!isStaminaCritical && staminaBarPulse != null)
                    {
                        staminaBarPulse.color = new Color(1f, 1f, 1f, 0f);
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
                float alpha = Mathf.Lerp(0.8f, 1f, (Mathf.Sin(pulseTime) + 1f) * 0.5f);
                Color baseColor = Color.Lerp(Color.red, new Color(1f, 0.3f, 0.3f), (Mathf.Sin(pulseTime) + 1f) * 0.5f);
                healthBarFill.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
            }
            
            // Solo actualizar el pulso de estamina si está en estado crítico
            if (isStaminaCritical && staminaBarPulse != null)
            {
                float alpha = Mathf.Lerp(0f, 0.3f, (Mathf.Sin(pulseTime * 1.5f) + 1f) * 0.5f);
                staminaBarPulse.color = new Color(1f, 1f, 1f, alpha);
            }
        }
        
        public void HideUI()
        {
            if (healthBarContainer != null)
                healthBarContainer.SetActive(false);
    
            if (staminaBarContainer != null)
                staminaBarContainer.SetActive(false);
        }
        
        public void ShowUI()
        {
            if (healthBarContainer != null)
                healthBarContainer.SetActive(true);
    
            if (staminaBarContainer != null)
                staminaBarContainer.SetActive(true);
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