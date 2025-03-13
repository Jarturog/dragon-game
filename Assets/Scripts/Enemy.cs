using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
   public float health = 100f;
   public float speed = 3.5f;
   public Transform player;
   
   private NavMeshAgent agent;
   private GameObject healthBarPlane;
   private Material healthBarMaterial;
   private Rigidbody rb;
   
   [Header("Physics Settings")]
   public float moveForce = 10f;
   public float maxSpeed = 5f;
   public float stoppingDistance = 1.5f;
   public float pathUpdateInterval = 0.5f;
   
   private Vector3 targetPosition;
   private float lastPathUpdateTime;
   private bool isPathValid = false;

   void Start() {
       
       if (player == null)
       {
           player = GameObject.FindGameObjectWithTag("Player").transform;
       }
       
       // Get components
       agent = GetComponent<NavMeshAgent>();
       rb = GetComponent<Rigidbody>();
       
       // Setup NavMeshAgent
       agent.updatePosition = false;
       agent.updateRotation = false;
       
       // Setup Rigidbody
       rb.freezeRotation = true;  // Optional: prevents tipping over
       rb.useGravity = true;
       rb.isKinematic = false;
       
       // Initial path calculation
       UpdatePath();
       
       CreateHealthBar();
   }

   void Update()
   {
       // Update path at regular intervals
       if (Time.time >= lastPathUpdateTime + pathUpdateInterval)
       {
           UpdatePath();
       }
       
       UpdateHealthBarPosition();
   }
   
   void FixedUpdate()
   {
       if (isPathValid)
       {
           // Move towards target using physics
           MoveWithPhysics();
       }
   }
   
   void UpdatePath()
   {
       lastPathUpdateTime = Time.time;
       
       if (player != null)
       {
           agent.SetDestination(player.position);
           isPathValid = true;
           
           // Get the next point on the path
           if (agent.path.corners.Length > 1)
           {
               targetPosition = agent.path.corners[1];
           }
           else if (agent.path.corners.Length > 0)
           {
               targetPosition = agent.path.corners[0];
           }
           else
           {
               isPathValid = false;
           }
       }
       
       // Update NavMeshAgent's position to match the rigidbody's position
       agent.nextPosition = transform.position;
   }
   
   void MoveWithPhysics()
   {
       // Calculate direction to target
       Vector3 directionToTarget = targetPosition - transform.position;
       directionToTarget.y = 0; // Keep movement on the horizontal plane
       
       // Check if we're close enough to the current target to update to the next one
       if (directionToTarget.magnitude < stoppingDistance)
       {
           // If we're close to the player, stop moving
           if (Vector3.Distance(transform.position, player.position) < stoppingDistance)
           {
               return;
           }
           
           // Otherwise, update the path to get the next point
           UpdatePath();
           return;
       }
       
       // Normalize direction and apply force
       directionToTarget.Normalize();
       
       // Apply force to move the enemy
       rb.AddForce(directionToTarget * moveForce, ForceMode.Force);
       
       // Limit speed
       Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
       if (horizontalVelocity.magnitude > maxSpeed)
       {
           horizontalVelocity = horizontalVelocity.normalized * maxSpeed;
           rb.velocity = new Vector3(horizontalVelocity.x, rb.velocity.y, horizontalVelocity.z);
       }
       
       // Rotate to face movement direction
       if (directionToTarget != Vector3.zero)
       {
           Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
           transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
       }
   }

   public void TakeDamage(float damage)
   {
       health -= damage;
       Debug.Log(gameObject.name + " recibió " + damage + " puntos de daño. Salud restante: " + health);
       UpdateHealthBar();
       
       if (health <= 0)
       {
           Die();
       }
   }

   void Die()
   {
       Debug.Log(gameObject.name + " ha sido derrotado!");
       Destroy(healthBarPlane);
       Destroy(gameObject);
   }

   void CreateHealthBar()
   {
       healthBarPlane = GameObject.CreatePrimitive(PrimitiveType.Quad);
       // Remove MeshCollider to prevent physics issues
       Destroy(healthBarPlane.GetComponent<Rigidbody>());
       Destroy(healthBarPlane.GetComponent<MeshCollider>());
       
       healthBarPlane.transform.SetParent(transform);
       healthBarPlane.transform.localScale = new Vector3(1f, 0.1f, 1f);
       healthBarPlane.transform.localPosition = new Vector3(0, 2, 0);
       
       healthBarMaterial = new Material(Shader.Find("Unlit/Color"));
       healthBarMaterial.color = Color.green;
       healthBarPlane.GetComponent<Renderer>().material = healthBarMaterial;
   }

   void UpdateHealthBar()
   {
       if (healthBarMaterial != null)
       {
           healthBarMaterial.color = Color.Lerp(Color.red, Color.green, health / 100f);
           healthBarPlane.transform.localScale = new Vector3(health / 100f, 0.1f, 1f);
       }
   }

   void UpdateHealthBarPosition()
   {
       if (healthBarPlane != null && Camera.main != null)
       {
           healthBarPlane.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
       }
   }
}