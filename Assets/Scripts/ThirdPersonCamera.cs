using System;
using UnityEngine;
public class ThirdPersonCamera: MonoBehaviour {
   public Transform player; // El transform del jugador a seguir
   public float distance = 5.0f; // Distancia de la cámara al jugador
   public float height = 2.0f; // Altura de la cámara
   public float smoothSpeed = 5.0f; // Velocidad de suavizado del movimiento
   public float mouseSensitivity = 3.0f; // Sensibilidad del ratón
   private float rotationX; // Rotación horizontal
   private float rotationY; // Rotación vertical
   public float minVerticalAngle = -30.0f; // Límite mínimo de rotación vertical
   public float maxVerticalAngle = 60.0f; // Límite máximo de rotación vertical
   public LayerMask collisionLayers; // Capas para detectar colisiones
   public float collisionOffset = 0.5f; // Offset para evitar clipping

   private void Awake() {
       GameObject secondCameraObject = GameObject.FindGameObjectWithTag("SecondCamera");
       transform.position = secondCameraObject.transform.position;
       transform.rotation = secondCameraObject.transform.rotation;
   }

   void Start() {
       
       // Ocultar y bloquear el cursor
       Cursor.lockState = CursorLockMode.Locked;
       Cursor.visible = false;
       
       // Inicializar la rotación actual
       Vector3 angles = transform.eulerAngles;
       rotationX = angles.y;
       rotationY = angles.x;
       
       // Asegurarse de que el PlayerController tenga referencia a esta cámara
       PlayerController playerController = player.GetComponent<PlayerController>();
       if (playerController != null) {
           playerController.cameraTransform = this.transform;
       }
   }
   
   void LateUpdate() {
       if (player == null) return;
       
       // Rotar la cámara basado en el movimiento del ratón
       rotationX += Input.GetAxis("Mouse X") * mouseSensitivity;
       rotationY -= Input.GetAxis("Mouse Y") * mouseSensitivity;
       
       // Limitar la rotación vertical para evitar voltear la cámara
       rotationY = Mathf.Clamp(rotationY, minVerticalAngle, maxVerticalAngle);
       
       // Calcular la rotación de la cámara
       Quaternion rotation = Quaternion.Euler(rotationY, rotationX, 0);
       
       // Calcular la posición deseada de la cámara
       Vector3 targetPosition = player.position - (rotation * Vector3.forward * distance);
       targetPosition.y += height;
       
       // Verificar colisiones
       Vector3 directionToTarget = targetPosition - (player.position + Vector3.up * height/2);
       float distanceToTarget = directionToTarget.magnitude;
       
       RaycastHit hit;
       if (Physics.SphereCast(player.position + Vector3.up * height/2, 0.2f, directionToTarget, out hit, distanceToTarget, collisionLayers)) {
           // Ajustar la posición de la cámara si hay colisión
           float adjustedDistance = hit.distance - collisionOffset;
           targetPosition = player.position + Vector3.up * height/2 + directionToTarget.normalized * adjustedDistance;
       }
       
       // Suavizar el movimiento de la cámara
       transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
       
       // Hacer que la cámara mire al jugador
       transform.LookAt(player.position + Vector3.up * height/2);
   }
   
   // Para desbloquear el cursor cuando el juego termine o se pause
   public void UnlockCursor() {
       Cursor.lockState = CursorLockMode.None;
       Cursor.visible = true;
   }
}