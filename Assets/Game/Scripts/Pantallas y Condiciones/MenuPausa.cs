using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class MenuPausa : MonoBehaviour
{
    [SerializeField] private GameObject menuPausa;
    [SerializeField] private GameObject botonPausa;

    [Header("Input System")]
    [Tooltip("InputActionAsset que contiene todas las acciones (el mismo que usa PlayerController)")]
    public InputActionAsset inputActions;

    private bool juegoPausado = false;
    private InputAction pauseAction;
    private InputActionMap playerActionMap;

    private void Start()
    {
        InitializeInputSystem();
    }

    private void InitializeInputSystem()
    {
        // Si no hay InputActionAsset asignado, crear acción manual (fallback)
        if (inputActions == null)
        {
            Debug.LogWarning("MenuPausa: No InputActionAsset asignado, usando InputAction manual como fallback");
            CreateManualPauseAction();
            return;
        }

        // Usar el InputActionAsset asignado
        playerActionMap = inputActions.FindActionMap("Player");
        if (playerActionMap != null)
        {
            // Obtener referencia a la acción de pausa
            pauseAction = playerActionMap.FindAction("Pause");
            
            if (pauseAction == null)
            {
                Debug.LogWarning("MenuPausa: Acción 'Pause' no encontrada en el InputActionAsset, usando acción manual");
                CreateManualPauseAction();
                return;
            }

            // Configurar callback
            pauseAction.performed += OnPausePerformed;
            
            // Habilitar el action map si no está habilitado
            if (!playerActionMap.enabled)
                playerActionMap.Enable();
            
            Debug.Log("✅ MenuPausa: Input System inicializado usando InputActionAsset");
        }
        else
        {
            Debug.LogError("MenuPausa: No se encontró el ActionMap 'Player' en el InputActionAsset");
            CreateManualPauseAction();
        }
    }

    private void CreateManualPauseAction()
    {
        // Crear acción manual como fallback
        if (pauseAction != null)
        {
            pauseAction.Disable();
            pauseAction.Dispose();
        }
        
        pauseAction = new InputAction("Pause", InputActionType.Button);
        pauseAction.AddBinding("<Keyboard>/enter");
        pauseAction.AddBinding("<Keyboard>/escape");
        pauseAction.AddBinding("<Gamepad>/start");
        pauseAction.performed += OnPausePerformed;
        pauseAction.Enable();
        
        Debug.Log("✅ MenuPausa: Input System inicializado usando InputAction manual");
    }

    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        if (juegoPausado)
        {
            ReanudarJuego();
        }
        else
        {
            PausarJuego();
        }
    }

    private void Update()
    {
        // Método Update mantenido vacío - ahora usamos callbacks del Input System
        // Solo se mantiene por compatibilidad si hay otros scripts que dependan de él
    }

    public void PausarJuego()
    {   
        juegoPausado = true;
        Time.timeScale = 0f;
        botonPausa.SetActive(false);
        menuPausa.SetActive(true);
    }

    public void ReanudarJuego()
    {
        juegoPausado = false;
        Time.timeScale = 1f;
        botonPausa.SetActive(true);
        menuPausa.SetActive(false);
    }

    public void ReiniciarNivel()
    {
        // Asegurar que el tiempo se restaure antes de cambiar de escena
        juegoPausado = false;
        Time.timeScale = 1f;
        
        // Limpiar cualquier input action que pueda estar activa
        if (pauseAction != null)
        {
            pauseAction.Disable();
        }
        
        // Recargar la escena
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }



    public void CerrarJuego()
    {
        Debug.Log("Cerrando juego...");
        Application.Quit();
    }

    private void OnDestroy()
    {
        // Limpiar callbacks del Input System
        if (pauseAction != null)
        {
            pauseAction.performed -= OnPausePerformed;
            
            // Solo deshabilitar y limpiar si es una acción manual
            if (playerActionMap == null)
            {
                pauseAction.Disable();
                pauseAction.Dispose();
            }
        }
    }

}
