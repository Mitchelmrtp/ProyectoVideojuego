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

    [Header("Zoom Settings")]
    [Tooltip("Target orthographic size when zoomed out. If <= 0, the defaultSize * zoomOutMultiplier is used.")]
    public float zoomOutSize = -1f;
    [Tooltip("When zoomOutSize <= 0, camera orthographic size is multiplied by this value to compute zoom-out size.")]
    public float zoomOutMultiplier = 1.5f;
    [Tooltip("How quickly the camera interpolates to the target size (higher = faster)")]
    public float smoothSpeed = 6f;

    [Header("Runtime")]
    [Tooltip("If true, script will only operate when the camera is orthographic. Otherwise no-op.")]
    public bool requireOrthographic = true;

    Camera cam;
    float defaultSize;
    float targetSize;
    bool isZoomedOut = false;

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

        defaultSize = cam.orthographicSize;
        // compute target size if not provided
        if (zoomOutSize <= 0f)
            targetSize = Mathf.Max(0.1f, defaultSize * zoomOutMultiplier);
        else
            targetSize = zoomOutSize;
    }

    void Update()
    {
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
}
