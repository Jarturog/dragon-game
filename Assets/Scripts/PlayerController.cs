using System;
using UnityEngine;

public class PlayerController : MonoBehaviour 
{
    [Header("Movimiento Básico")]
    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    public float jumpHeight = 2f;
    public float gravity = 9.81f;
    public Transform cameraTransform;
    public float rotationSpeed = 10f; // Velocidad de rotación del personaje
    
    [Header("Ataques")]
    public float attackCooldown = 0.5f;
    public float spearAttackRange = 2f;
    public float fireAttackRange = 5f;
    public float fireAttackDamage = 25f;
    public float spearAttackDamage = 15f;
    
    [Header("Efectos")]
    public ParticleSystem fireParticleSystem;
    public float fireEffectDuration = 2f;
    
    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private float lastAttackTime;
    [HideInInspector] public bool isRunning = false;
    
    private PlayerHealth playerHealth;
    private MainMenuManager menuManager;

    private Animator _animator;
    private bool _estaCaminandoAnimacion;
    
    void Start() {
        menuManager = FindFirstObjectByType<MainMenuManager>();
        
        controller = GetComponent<CharacterController>();
        playerHealth = GetComponent<PlayerHealth>();
        
        if (playerHealth == null)
        {
            Debug.LogWarning("PlayerHealth component not found. Adding one automatically.");
            playerHealth = gameObject.AddComponent<PlayerHealth>();
        }
        
        _animator = GetComponentInChildren<Animator>();
        _estaCaminandoAnimacion = false;
        
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;
            
        if (fireParticleSystem == null)
        {
            Debug.LogWarning("No se ha asignado un sistema de partículas para la llamarada. Crea uno en la escena y asígnalo al script.");
        }
    }
    
    void Update() 
    {
        HandleMovement();
        HandleAttacks();
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            menuManager.ShowPauseMenu();
        }
    }
    
    void HandleMovement()
    {
        // Verificar si está en el suelo
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0) 
        {
            velocity.y = -2f;
        }
        
        // Correr (mantener shift)
        bool wantsToRun = Input.GetKey(KeyCode.LeftShift);
        
        // Only run if we have enough stamina
        if (wantsToRun && playerHealth.currentStamina > Math.Min(1, playerHealth.maxStamina / 100))
        {
            isRunning = true;
            // Consume stamina while running (only if actually moving)
            if ((Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0))
            {
                playerHealth.UseStamina(playerHealth.runStaminaCost * Time.deltaTime);
            }
        }
        else
        {
            isRunning = false;
        }
        
        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        
        // Movimiento horizontal relativo a la cámara
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        
        // Crear vector de movimiento basado en orientación de cámara
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();
        
        Vector3 move = right * moveX + forward * moveZ;

        if (move != Vector3.zero) {
            if (!isRunning && !_estaCaminandoAnimacion) {
                _animator.SetTrigger("Caminar");
            }
            // Aplicar movimiento
            controller.Move(move * (currentSpeed * Time.deltaTime));
        }
        
        
        // MODIFICACIÓN: Siempre rotar el personaje para dar la espalda a la cámara
        // Obtenemos la dirección de la cámara al jugador en el plano horizontal
        Vector3 directionFromCamera = transform.position - cameraTransform.position;
        directionFromCamera.y = 0;
        directionFromCamera.Normalize();
        
        // Calculamos la rotación para dar la espalda a la cámara
        Quaternion targetRotation = Quaternion.LookRotation(directionFromCamera);
        
        // Aplicamos la rotación suavemente
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        
        // Salto
        if (Input.GetButtonDown("Jump") && isGrounded) 
        {
            // Check if we have enough stamina to jump
            if (playerHealth.UseStamina(playerHealth.jumpStaminaCost))
            {
                velocity.y = Mathf.Sqrt(jumpHeight * 2f * gravity);
            }
        }
        
        // Aplicar gravedad
        velocity.y -= gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
    
    void HandleAttacks()
    {
        // Verificar si podemos atacar (cooldown)
        if (Time.time < lastAttackTime + attackCooldown)
            return;
            
        // Ataque de lanza (clic izquierdo)
        if (Input.GetMouseButtonDown(0))
        {
            // Check if we have enough stamina to attack
            if (playerHealth.UseStamina(playerHealth.attackStaminaCost))
            {
                SpearAttack();
            }
        }
        
        // Ataque de fuego (clic derecho)
        if (Input.GetMouseButtonDown(1))
        {
            // Check if we have enough stamina to attack
            if (playerHealth.UseStamina(playerHealth.attackStaminaCost * 2)) // Fire attack costs more
            {
                FireAttack();
            }
        }
    }
    
    void SpearAttack()
    {
        lastAttackTime = Time.time;
        Debug.Log("¡Ataque con lanza!");
        
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, spearAttackRange))
        {
            Debug.Log("Golpeando con lanza a " + hit.collider.name);
            
            Enemy enemy = hit.collider.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(spearAttackDamage);
            }
        }
        
        Debug.DrawRay(transform.position, transform.forward * spearAttackRange, Color.red, 0.5f);
    }
    
    void FireAttack()
    {
        lastAttackTime = Time.time;
        Debug.Log("¡Llamarada de fuego!");
        _animator.SetTrigger("Atacar 2");
        
        if (fireParticleSystem != null)
        {
            fireParticleSystem.transform.position = transform.position + transform.forward * 0.5f + Vector3.up * 1f;
            fireParticleSystem.transform.rotation = transform.rotation;
            fireParticleSystem.Play();
            Invoke("StopFireEffect", fireEffectDuration);
        }
        
        RaycastHit[] hits = Physics.RaycastAll(transform.position, transform.forward, fireAttackRange);
        foreach (RaycastHit hit in hits)
        {
            Enemy enemy = hit.collider.GetComponent<Enemy>();
            if (enemy != null)
            {
                Debug.Log("Golpeando con fuego a " + hit.collider.name);
                enemy.TakeDamage(fireAttackDamage);
            }
        }
    }
    
    void StopFireEffect()
    {
        if (fireParticleSystem != null)
        {
            fireParticleSystem.Stop();
        }
    }

    public void setEstaCaminandoAnimacion(bool b) {
        _estaCaminandoAnimacion = b;
    }
}