using UnityEngine;

public class PlayerController : MonoBehaviour 
{
    [Header("Movimiento Básico")]
    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    public float jumpHeight = 2f;
    public float gravity = 9.81f;
    public Transform cameraTransform;
    
    [Header("Ataques")]
    public float attackCooldown = 0.5f;
    public float spearAttackRange = 2f;
    public float fireAttackRange = 5f;
    public float fireAttackDamage = 25f;
    public float spearAttackDamage = 15f;
    
    [Header("Efectos")]
    public ParticleSystem fireParticleSystem; // Sistema de partículas para la llamarada
    public float fireEffectDuration = 2f; // Duración de la llamarada
    
    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private float lastAttackTime;
    private bool isRunning = false;
    
    void Start() 
    {
        controller = GetComponent<CharacterController>();
        
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;
            
        // Si no se ha asignado un sistema de partículas, intentar crearlo
        if (fireParticleSystem == null)
        {
            Debug.LogWarning("No se ha asignado un sistema de partículas para la llamarada. Crea uno en la escena y asígnalo al script.");
        }
    }
    
    void Update() 
    {
        // Movimiento básico
        HandleMovement();
        
        // Ataques
        HandleAttacks();
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
        isRunning = Input.GetKey(KeyCode.LeftShift);
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
        
        // Rotar el personaje hacia la dirección del movimiento
        if (move.magnitude > 0.1f)
        {
            transform.rotation = Quaternion.LookRotation(move);
        }
        
        controller.Move(move * (currentSpeed * Time.deltaTime));
        
        // Salto
        if (Input.GetButtonDown("Jump") && isGrounded) 
        {
            velocity.y = Mathf.Sqrt(jumpHeight * 2f * gravity);
            Debug.Log("Salto");
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
            SpearAttack();
        }
        
        // Ataque de fuego (clic derecho)
        if (Input.GetMouseButtonDown(1))
        {
            FireAttack();
        }
    }
    
    void SpearAttack()
    {
        lastAttackTime = Time.time;
        Debug.Log("¡Ataque con lanza!");
        
        // Usar un Raycast para detectar enemigos delante del jugador
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
        
        // Efecto visual simple
        Debug.DrawRay(transform.position, transform.forward * spearAttackRange, Color.red, 0.5f);
    }
    
    void FireAttack()
    {
        lastAttackTime = Time.time;
        Debug.Log("¡Llamarada de fuego!");
        
        // Activar el sistema de partículas
        if (fireParticleSystem != null)
        {
            // Posicionar y orientar el sistema de partículas en la dirección del personaje
            fireParticleSystem.transform.position = transform.position + transform.forward * 0.5f + Vector3.up * 1f;
            fireParticleSystem.transform.rotation = transform.rotation;
            
            // Iniciar la emisión de partículas
            fireParticleSystem.Play();
            
            // Detener las partículas después de un tiempo
            Invoke("StopFireEffect", fireEffectDuration);
        }
        Debug.Log(transform.position);
        // Raycast para detectar enemigos en el camino de la llamarada
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
}