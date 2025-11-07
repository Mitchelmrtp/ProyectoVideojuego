using UnityEngine;
// Usamos la nueva Input System para detectar teclas cuando el proyecto la tiene activa
using UnityEngine.InputSystem;

/// <summary>
/// CameraZoom: simple, inspector-configurable zoom out feature for 2D orthographic camera.
/// - Works in Toggle mode (press key to toggle zoom out / back) or Hold mode (hold key to zoom out).
/// - Smoothly interpolates the camera's orthographic size.
/// - Drop this script on the main Camera (or any Camera) and configure in the Inspector.
/// </summary>
[DisallowMultipleComponent]
public class CameraZoom : MonoBehaviour
{
    [Header("Input")]
    [Tooltip("Tecla usada para activar el zoom out (toggle o hold según el modo). Usa la nueva Input System.")]
    public Key zoomKey = Key.Z;
    [Tooltip("If true, pressing the key toggles zoom state. If false, holding the key zooms out while held.")]
    public bool toggleMode = true;

    [Header("Camera Position Controls")]
    [Tooltip("Posición inicial X de la cámara")]
    public float initialPositionX = 0f;
    [Tooltip("Posición inicial Y de la cámara")]  
    public float initialPositionY = 0f;
    [Tooltip("Aplicar posición inicial al empezar")]
    public bool applyInitialPosition = false;
    
    [Header("Runtime Position Controls")]
    [Tooltip("Control de posición X en tiempo real")]
    [Range(-50f, 50f)]
    public float runtimePositionX = 0f;
    [Tooltip("Control de posición Y en tiempo real")]
    [Range(-50f, 50f)] 
    public float runtimePositionY = 0f;
    [Tooltip("Aplicar controles de posición en runtime")]
    public bool enableRuntimePositionControl = false;

    [Header("Zoom Settings")]
    [Tooltip("Aplicar zoom inicial al empezar el juego")]
    public bool applyInitialZoom = true;
    [Tooltip("Tamaño ortográfico inicial cuando inicia el juego")]
    [Range(1f, 20f)]
    public float initialZoomSize = 5f;
    [Tooltip("Target orthographic size when zoomed out. If <= 0, the defaultSize * zoomOutMultiplier is used.")]
    public float zoomOutSize = -1f;
    [Tooltip("When zoomOutSize <= 0, camera orthographic size is multiplied by this value to compute zoom-out size.")]
    [Range(1.1f, 3.0f)]
    public float zoomOutMultiplier = 1.5f;
    [Tooltip("How quickly the camera interpolates to the target size (higher = faster)")]
    [Range(1f, 20f)]
    public float smoothSpeed = 6f;

    [Header("Runtime")]
    [Tooltip("If true, script will only operate when the camera is orthographic. Otherwise no-op.")]
    public bool requireOrthographic = true;

    Camera cam;
    float defaultSize;
    float targetSize;
    bool isZoomedOut = false;
    
    // Variables para control de posición
    private Vector3 originalPosition;
    private Vector3 lastRuntimePosition;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
            cam = Camera.main;

        if (cam == null)
        {
            Debug.LogError("CameraZoom: no Camera found on GameObject and no Camera.main available.");
            enabled = false;
            return;
        }

        if (requireOrthographic && !cam.orthographic)
        {
            Debug.LogWarning("CameraZoom: camera is not orthographic. Disabling CameraZoom (requireOrthographic=true).");
            enabled = false;
            return;
        }

        // Guardar el tamaño original de la cámara ANTES de cualquier modificación
        float originalSize = cam.orthographicSize;
        
        // Configurar zoom inicial si está habilitado
        if (applyInitialZoom)
        {
            cam.orthographicSize = initialZoomSize;
            defaultSize = initialZoomSize;
            Debug.Log($"🎯 CameraZoom: Zoom inicial aplicado: {originalSize} → {initialZoomSize}");
        }
        else
        {
            defaultSize = originalSize;
            Debug.Log($"🎯 CameraZoom: Usando tamaño original de la cámara: {originalSize}");
        }
        
        // Guardar posición original
        originalPosition = cam.transform.position;
        
        // Configurar posición inicial si está habilitado
        if (applyInitialPosition)
        {
            Vector3 newPosition = new Vector3(initialPositionX, initialPositionY, originalPosition.z);
            cam.transform.position = newPosition;
            Debug.Log($"📍 CameraZoom: Posición inicial aplicada: ({initialPositionX}, {initialPositionY})");
        }
        
        // Inicializar controles runtime
        runtimePositionX = cam.transform.position.x;
        runtimePositionY = cam.transform.position.y;
        lastRuntimePosition = new Vector3(runtimePositionX, runtimePositionY, originalPosition.z);
        
        // Compute target size if not provided
        if (zoomOutSize <= 0f)
            targetSize = Mathf.Max(0.1f, defaultSize * zoomOutMultiplier);
        else
            targetSize = zoomOutSize;
            
        Debug.Log($"📷 CameraZoom inicializado:");
        Debug.Log($"   - Tamaño por defecto: {defaultSize}");
        Debug.Log($"   - Tamaño zoom out: {targetSize}");
        Debug.Log($"   - Multiplicador: {zoomOutMultiplier}x");
    }

    void Update()
    {
        // Control de posición en tiempo real desde el Inspector
        HandleRuntimePositionControl();
        
        // Usar la nueva Input System. Si no hay teclado conectado, no hacemos nada.
        if (Keyboard.current == null)
            return;

        bool shouldZoomOut;
        if (toggleMode)
        {
            if (Keyboard.current[zoomKey].wasPressedThisFrame)
            {
                isZoomedOut = !isZoomedOut;
            }
            shouldZoomOut = isZoomedOut;
        }
        else
        {
            // hold mode: zoom while key is pressed
            shouldZoomOut = Keyboard.current[zoomKey].isPressed;
            isZoomedOut = shouldZoomOut;
        }

        float desired = shouldZoomOut ? targetSize : defaultSize;
        // smooth interpolation
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, desired, 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime));
    }
    
    /// <summary>
    /// Maneja los controles de posición en tiempo real desde el Inspector
    /// </summary>
    private void HandleRuntimePositionControl()
    {
        if (!enableRuntimePositionControl || cam == null)
            return;
            
        // Verificar si la posición ha cambiado en el Inspector
        Vector3 currentRuntimePosition = new Vector3(runtimePositionX, runtimePositionY, originalPosition.z);
        
        if (Vector3.Distance(currentRuntimePosition, lastRuntimePosition) > 0.01f)
        {
            // Aplicar nueva posición
            cam.transform.position = currentRuntimePosition;
            lastRuntimePosition = currentRuntimePosition;
            
            // Debug opcional cada segundo
            if (Time.frameCount % 60 == 0)
            {
                Debug.Log($"📍 Posición de cámara actualizada: ({runtimePositionX:F2}, {runtimePositionY:F2})");
            }
        }
    }

    /// <summary>
    /// External API: force zoom out state (true = zoomed out, false = default)
    /// </summary>
    public void SetZoomedOut(bool zoomOut)
    {
        isZoomedOut = zoomOut;
    }

    /// <summary>
    /// External API: immediately set camera size to default and cancel zoom state.
    /// </summary>
    public void ResetZoom()
    {
        isZoomedOut = false;
        if (cam != null)
            cam.orthographicSize = defaultSize;
    }
    
    /// <summary>
    /// External API: cambiar el tamaño de zoom inicial en runtime
    /// </summary>
    public void SetInitialZoom(float newInitialSize)
    {
        if (newInitialSize > 0f)
        {
            initialZoomSize = newInitialSize;
            defaultSize = newInitialSize;
            
            // Si no estamos en zoom out, aplicar inmediatamente
            if (!isZoomedOut && cam != null)
            {
                cam.orthographicSize = newInitialSize;
            }
            
            // Recalcular zoom out size
            if (zoomOutSize <= 0f)
                targetSize = Mathf.Max(0.1f, defaultSize * zoomOutMultiplier);
                
            Debug.Log($"🎯 Zoom inicial cambiado a: {newInitialSize}");
        }
    }
    
    /// <summary>
    /// External API: obtener el tamaño actual de la cámara
    /// </summary>
    public float GetCurrentSize()
    {
        return cam != null ? cam.orthographicSize : 0f;
    }
    
    /// <summary>
    /// External API: obtener el tamaño por defecto configurado
    /// </summary>
    public float GetDefaultSize()
    {
        return defaultSize;
    }
    
    /// <summary>
    /// External API: obtener el tamaño de zoom out
    /// </summary>
    public float GetZoomOutSize()
    {
        return targetSize;
    }
    
    /// <summary>
    /// External API: verificar si está en zoom out
    /// </summary>
    public bool IsZoomedOut()
    {
        return isZoomedOut;
    }
    
    /// <summary>
    /// External API: establecer posición de la cámara
    /// </summary>
    public void SetCameraPosition(float x, float y)
    {
        if (cam != null)
        {
            Vector3 newPosition = new Vector3(x, y, originalPosition.z);
            cam.transform.position = newPosition;
            
            // Actualizar controles del Inspector
            runtimePositionX = x;
            runtimePositionY = y;
            lastRuntimePosition = newPosition;
            
            Debug.Log($"📍 Posición de cámara establecida: ({x:F2}, {y:F2})");
        }
    }
    
    /// <summary>
    /// External API: obtener posición actual de la cámara
    /// </summary>
    public Vector3 GetCameraPosition()
    {
        return cam != null ? cam.transform.position : Vector3.zero;
    }
    
    /// <summary>
    /// External API: resetear posición a la original
    /// </summary>
    public void ResetCameraPosition()
    {
        if (cam != null)
        {
            cam.transform.position = originalPosition;
            runtimePositionX = originalPosition.x;
            runtimePositionY = originalPosition.y;
            lastRuntimePosition = originalPosition;
            
            Debug.Log($"📍 Posición de cámara reseteada a: ({originalPosition.x:F2}, {originalPosition.y:F2})");
        }
    }
    
    /// <summary>
    /// External API: habilitar/deshabilitar control de posición desde Inspector
    /// </summary>
    public void SetRuntimePositionControl(bool enable)
    {
        enableRuntimePositionControl = enable;
        Debug.Log($"🎮 Control de posición en runtime: {(enable ? "HABILITADO" : "DESHABILITADO")}");
    }
}
