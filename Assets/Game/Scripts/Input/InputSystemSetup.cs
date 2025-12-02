using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Script auxiliar para configurar automáticamente el InputActionAsset en PlayerController y MenuPausa
/// Asegura que ambos scripts usen el mismo InputActionAsset para consistencia
/// </summary>
public class InputSystemSetup : MonoBehaviour
{
    [Header("Input Configuration")]
    [Tooltip("InputActionAsset principal del juego")]
    public InputActionAsset mainInputActions;
    
    [Header("Referencias Automáticas")]
    [Tooltip("Se detectará automáticamente si no se asigna")]
    public PlayerController playerController;
    
    [Tooltip("Se detectará automáticamente si no se asigna")]
    public MenuPausa menuPausa;

    [Header("Debug")]
    [Tooltip("Mostrar información de debug en consola")]
    public bool showDebugInfo = true;

    void Start()
    {
        // Auto-detect components if not assigned
        if (playerController == null)
            playerController = FindFirstObjectByType<PlayerController>();
            
        if (menuPausa == null)
            menuPausa = FindFirstObjectByType<MenuPausa>();

        // Auto-detect InputActionAsset if not assigned
        if (mainInputActions == null)
        {
            // Try to find it in the project
            mainInputActions = Resources.LoadAll<InputActionAsset>("")[0];
            if (mainInputActions == null)
            {
                Debug.LogError("InputSystemSetup: No se pudo encontrar un InputActionAsset en el proyecto");
                return;
            }
        }

        SetupInputSystems();
    }

    private void SetupInputSystems()
    {
        bool setupSuccess = false;

        // Setup PlayerController
        if (playerController != null && mainInputActions != null)
        {
            playerController.inputActions = mainInputActions;
            setupSuccess = true;
            
            if (showDebugInfo)
                Debug.Log($"✅ InputSystemSetup: PlayerController configurado con {mainInputActions.name}");
        }
        else if (showDebugInfo)
        {
            Debug.LogWarning("InputSystemSetup: PlayerController no encontrado o InputActionAsset faltante");
        }

        // Setup MenuPausa
        if (menuPausa != null && mainInputActions != null)
        {
            menuPausa.inputActions = mainInputActions;
            setupSuccess = true;
            
            if (showDebugInfo)
                Debug.Log($"✅ InputSystemSetup: MenuPausa configurado con {mainInputActions.name}");
        }
        else if (showDebugInfo)
        {
            Debug.LogWarning("InputSystemSetup: MenuPausa no encontrado o InputActionAsset faltante");
        }

        if (setupSuccess && showDebugInfo)
        {
            Debug.Log("🎮 InputSystemSetup: Configuración de Input System completada exitosamente");
            LogControlScheme();
        }
    }

    private void LogControlScheme()
    {
        if (!showDebugInfo) return;

        Debug.Log("🎮 CONTROLES CONFIGURADOS:");
        Debug.Log("   🎮 Gamepad:");
        Debug.Log("      • Stick Izquierdo: Movimiento");
        Debug.Log("      • A (ButtonSouth): Saltar");
        Debug.Log("      • R2 (RightTrigger): Atacar");
        Debug.Log("      • Y (ButtonNorth): Cambiar Gravedad + Zoom");
        Debug.Log("      • Start/Options: Menú de Pausa");
        Debug.Log("   ⌨️ Teclado:");
        Debug.Log("      • WASD / Flechas: Movimiento");
        Debug.Log("      • Espacio: Saltar");
        Debug.Log("      • Click Izquierdo: Atacar");
        Debug.Log("      • Q / Click Derecho: Cambiar Gravedad + Zoom");
        Debug.Log("      • Enter / Escape: Menú de Pausa");
    }

    /// <summary>
    /// Método público para reconfigurar el sistema si es necesario
    /// </summary>
    [ContextMenu("Reconfigurar Input System")]
    public void ReconfigureInputSystem()
    {
        SetupInputSystems();
    }

    /// <summary>
    /// Verificar que todos los componentes estén configurados correctamente
    /// </summary>
    [ContextMenu("Verificar Configuración")]
    public void VerifyConfiguration()
    {
        Debug.Log("🔍 VERIFICANDO CONFIGURACIÓN DEL INPUT SYSTEM:");
        
        if (mainInputActions == null)
        {
            Debug.LogError("❌ InputActionAsset no asignado");
            return;
        }

        var playerMap = mainInputActions.FindActionMap("Player");
        if (playerMap == null)
        {
            Debug.LogError("❌ Action Map 'Player' no encontrado");
            return;
        }

        // Verificar acciones requeridas
        string[] requiredActions = { "Move", "Jump", "Attack", "ChangeGravity", "Pause" };
        foreach (string actionName in requiredActions)
        {
            var action = playerMap.FindAction(actionName);
            if (action != null)
                Debug.Log($"✅ Acción '{actionName}' encontrada");
            else
                Debug.LogWarning($"⚠️ Acción '{actionName}' no encontrada");
        }

        // Verificar componentes
        if (playerController != null && playerController.inputActions == mainInputActions)
            Debug.Log("✅ PlayerController correctamente configurado");
        else
            Debug.LogWarning("⚠️ PlayerController no configurado o InputActionAsset diferente");

        if (menuPausa != null && menuPausa.inputActions == mainInputActions)
            Debug.Log("✅ MenuPausa correctamente configurado");
        else
            Debug.LogWarning("⚠️ MenuPausa no configurado o InputActionAsset diferente");
            
        Debug.Log("🔍 Verificación completada");
    }
}