using UnityEngine;

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
        healthUI = new PlayerHealthUI(transform);
    }
    
    void Update()
    {
        // Regenerate stamina when not running
        if (!playerController.isRunning)
        {
            RegenerateStamina();
        }
        
        healthUI.UpdateUIPosition();
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
    
    // Similar to the Enemy's HealthBar class, but with an additional stamina bar
    class PlayerHealthUI
    {
        private GameObject healthBarPlane;
        private Material healthBarMaterial;
        
        private GameObject staminaBarPlane;
        private Material staminaBarMaterial;
        
        private Transform playerTransform;
        
        public PlayerHealthUI(Transform transform)
        {
            playerTransform = transform;
            
            // Create health bar
            healthBarPlane = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Destroy(healthBarPlane.GetComponent<Collider>());
            healthBarPlane.transform.localScale = new Vector3(1f, 0.1f, 1f);
            healthBarPlane.transform.position = playerTransform.position + new Vector3(0, 2.2f, 0);
            
            healthBarMaterial = new Material(Shader.Find("Unlit/Color"));
            healthBarMaterial.color = Color.green;
            healthBarPlane.GetComponent<Renderer>().material = healthBarMaterial;
            
            // Create stamina bar
            staminaBarPlane = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Destroy(staminaBarPlane.GetComponent<Collider>());
            staminaBarPlane.transform.localScale = new Vector3(1f, 0.1f, 1f);
            staminaBarPlane.transform.position = playerTransform.position + new Vector3(0, 2.0f, 0);
            
            staminaBarMaterial = new Material(Shader.Find("Unlit/Color"));
            staminaBarMaterial.color = Color.blue;
            staminaBarPlane.GetComponent<Renderer>().material = staminaBarMaterial;
        }
        
        public void UpdateHealthBar(float healthPercent)
        {
            if (healthBarMaterial != null)
            {
                // Clamp health percent between 0 and 1
                healthPercent = Mathf.Clamp01(healthPercent);
                healthBarMaterial.color = Color.Lerp(Color.red, Color.green, healthPercent);
                healthBarPlane.transform.localScale = new Vector3(healthPercent, 0.1f, 1f);
            }
        }

        public void UpdateStaminaBar(float staminaPercent)
        {
            if (staminaBarMaterial != null)
            {
                // Clamp stamina percent between 0 and 1
                staminaPercent = Mathf.Clamp01(staminaPercent);
                staminaBarMaterial.color = Color.Lerp(Color.gray, Color.blue, staminaPercent);
                staminaBarPlane.transform.localScale = new Vector3(staminaPercent, 0.1f, 1f);
            }
        }
        
        public void UpdateUIPosition()
        {
            if (healthBarPlane != null && staminaBarPlane != null && Camera.main != null)
            {
                // Update position to follow the player
                healthBarPlane.transform.position = playerTransform.position + new Vector3(0, 2.2f, 0);
                staminaBarPlane.transform.position = playerTransform.position + new Vector3(0, 2.0f, 0);
                
                // Make the bars always face the camera
                healthBarPlane.transform.rotation = Camera.main.transform.rotation;
                staminaBarPlane.transform.rotation = Camera.main.transform.rotation;
            }
        }
        
        public void HideUI()
        {
            if (healthBarPlane != null)
                healthBarPlane.SetActive(false);
    
            if (staminaBarPlane != null)
                staminaBarPlane.SetActive(false);
        }
    }
}