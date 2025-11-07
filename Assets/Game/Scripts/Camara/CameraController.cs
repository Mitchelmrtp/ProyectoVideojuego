using UnityEngine;

/// <summary>
/// CameraController: Sistema completo de cámara que combina seguimiento del jugador,
/// efectos de parallax, y zoom configurable desde el inspector.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("Target Following")]
    [Tooltip("GameObject a seguir (generalmente el player)")]
    public Transform target;
    
    [Tooltip("Activar seguimiento automático del target")]
    public bool followTarget = true;
    
    [Tooltip("Velocidad de suavizado (0-1, donde 0 es instantáneo)")]
    [Range(0f, 1f)]
    public float smoothSpeed = 0.125f;
    
    [Tooltip("Offset de la cámara respecto al target")]
    public Vector3 offset = new Vector3(0, 0, -10);

    [Header("Camera Bounds")]
    [Tooltip("Usar límites para la cámara")]
    public bool useBounds = false;
    
    [Tooltip("Límites de movimiento de la cámara")]
    public Bounds cameraBounds = new Bounds(Vector3.zero, Vector3.one * 20f);

    [Header("Look Ahead")]
    [Tooltip("Activar look-ahead (la cámara se adelanta en la dirección del movimiento)")]
    public bool useLookAhead = false;
    
    [Tooltip("Distancia del look-ahead")]
    public float lookAheadDistance = 2f;
    
    [Tooltip("Velocidad del look-ahead")]
    public float lookAheadSpeed = 2f;

    [Header("Zoom Integration")]
    [Tooltip("Script de zoom de la cámara (opcional)")]
    public CameraZoom cameraZoom;

    // Parallax system
    public delegate void ParallaxCameraDelegate(float deltaMovementX, float deltaMovementY);
    public ParallaxCameraDelegate onCameraTranslate;

    // Private variables
    private Camera cam;
    private Vector3 lastPosition;
    private Vector3 lookAheadPos;
    private Vector3 velocity;

    void Start()
    {
        cam = GetComponent<Camera>();
        
        // Auto-find target if not assigned
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }

        // Auto-find zoom script if not assigned
        if (cameraZoom == null)
            cameraZoom = GetComponent<CameraZoom>();

        lastPosition = transform.position;
        lookAheadPos = Vector3.zero;

        if (target != null)
        {
            // Position camera initially
            Vector3 initialPos = target.position + offset;
            if (useBounds)
                initialPos = ClampToBounds(initialPos);
            
            transform.position = initialPos;
            lastPosition = initialPos;
        }
    }

    void LateUpdate()
    {
        if (target != null && followTarget)
        {
            UpdateCameraPosition();
        }

        HandleParallaxMovement();
    }

    void UpdateCameraPosition()
    {
        // Calculate target position
        Vector3 targetPos = target.position + offset;

        // Look ahead logic
        if (useLookAhead)
        {
            // Calculate velocity for look ahead
            Vector3 targetVelocity = (target.position - lastPosition) / Time.deltaTime;
            Vector3 lookAheadTarget = new Vector3(targetVelocity.x * lookAheadDistance, 0, 0);
            
            lookAheadPos = Vector3.Lerp(lookAheadPos, lookAheadTarget, lookAheadSpeed * Time.deltaTime);
            targetPos += lookAheadPos;
        }

        // Apply bounds if enabled
        if (useBounds)
        {
            targetPos = ClampToBounds(targetPos);
        }

        // Smooth movement
        if (smoothSpeed > 0)
        {
            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothSpeed);
        }
        else
        {
            transform.position = targetPos;
        }
    }

    Vector3 ClampToBounds(Vector3 position)
    {
        // Get camera bounds in world space
        float cameraHeight = cam.orthographicSize * 2f;
        float cameraWidth = cameraHeight * cam.aspect;

        // Calculate bounds
        float minX = cameraBounds.min.x + cameraWidth / 2f;
        float maxX = cameraBounds.max.x - cameraWidth / 2f;
        float minY = cameraBounds.min.y + cameraHeight / 2f;
        float maxY = cameraBounds.max.y - cameraHeight / 2f;

        // Clamp position
        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.y = Mathf.Clamp(position.y, minY, maxY);

        return position;
    }

    void HandleParallaxMovement()
    {
        if (transform.position != lastPosition)
        {
            if (onCameraTranslate != null)
            {
                float deltaX = lastPosition.x - transform.position.x;
                float deltaY = lastPosition.y - transform.position.y;
                onCameraTranslate(deltaX, deltaY);
            }

            lastPosition = transform.position;
        }
    }

    // Public methods for external control
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void SetFollowEnabled(bool enabled)
    {
        followTarget = enabled;
    }

    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
    }

    public void FocusOnTarget()
    {
        if (target != null)
        {
            Vector3 targetPos = target.position + offset;
            if (useBounds)
                targetPos = ClampToBounds(targetPos);
            
            transform.position = targetPos;
            lastPosition = targetPos;
        }
    }

    // Gizmos for visualization in editor
    void OnDrawGizmosSelected()
    {
        if (useBounds)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(cameraBounds.center, cameraBounds.size);
        }

        if (target != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(target.position, 0.5f);
            
            Gizmos.color = Color.green;
            Vector3 targetPos = target.position + offset;
            Gizmos.DrawWireSphere(targetPos, 0.3f);
            Gizmos.DrawLine(target.position, targetPos);
        }
    }
}