using UnityEngine;

public class CameraScript : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("El GameObject que la cámara debe seguir (generalmente el player)")]
    public GameObject target;
    
    [Header("Camera Follow Settings")]
    [Tooltip("Si true, la cámara seguirá al target automáticamente")]
    public bool followTarget = true;
    
    [Tooltip("Velocidad de suavizado del seguimiento (0 = instantáneo, valores más altos = más suave)")]
    public float smoothSpeed = 0.125f;
    
    [Tooltip("Offset desde la posición del target")]
    public Vector3 offset = new Vector3(0, 0, -10);
    
    [Header("Limits (Optional)")]
    [Tooltip("Activar límites de movimiento de la cámara")]
    public bool useLimits = false;
    
    [Tooltip("Límites de la cámara (minX, minY, maxX, maxY)")]
    public Vector4 limits = new Vector4(-10, -10, 10, 10);

    // Parallax support
    public delegate void ParallaxCameraDelegate(float deltaMovement);
    public ParallaxCameraDelegate onCameraTranslate;

    private float oldPosition;

    void Start()
    {
        // Si no se asigna un target, buscar el player automáticamente
        if (target == null)
        {
            target = GameObject.FindGameObjectWithTag("Player");
            if (target == null)
            {
                Debug.LogWarning("CameraScript: No se encontró un target asignado ni un GameObject con tag 'Player'");
            }
        }

        // Inicializar posición anterior para el parallax
        oldPosition = transform.position.x;
    }

    void LateUpdate()
    {
        if (target != null && followTarget)
        {
            FollowTarget();
        }
        
        // Detectar movimiento para el sistema de parallax
        HandleParallaxMovement();
    }

    void FollowTarget()
    {
        Vector3 desiredPosition = target.transform.position + offset;
        
        // Aplicar límites si están habilitados
        if (useLimits)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, limits.x, limits.z);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, limits.y, limits.w);
        }
        
        // Suavizar el movimiento
        if (smoothSpeed > 0)
        {
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;
        }
        else
        {
            transform.position = desiredPosition;
        }
    }

    void HandleParallaxMovement()
    {
        if (transform.position.x != oldPosition)
        {
            if (onCameraTranslate != null)
            {
                float delta = oldPosition - transform.position.x;
                onCameraTranslate(delta);
            }

            oldPosition = transform.position.x;
        }
    }

    // Método público para cambiar el target dinámicamente
    public void SetTarget(GameObject newTarget)
    {
        target = newTarget;
    }

    // Método público para activar/desactivar el seguimiento
    public void SetFollowTarget(bool follow)
    {
        followTarget = follow;
    }
}
