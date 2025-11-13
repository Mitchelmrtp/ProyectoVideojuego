using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class MenuPausa : MonoBehaviour
{
    [SerializeField] private GameObject menuPausa;
    [SerializeField] private GameObject botonPausa;

    private bool juegoPausado = false;
    private InputAction pauseAction;

    private void Start()
    {
        pauseAction = new InputAction("Pause", InputActionType.Button, "<Keyboard>/escape");
        pauseAction.Enable();
    }

    private void Update()
    {
        if (pauseAction.WasPressedThisFrame())
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
        juegoPausado = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void CerrarJuego()
    {
        Debug.Log("Cerrando juego...");
        Application.Quit();
    }

    private void OnDestroy()
    {
        if (pauseAction != null)
        {
            pauseAction.Disable();
            pauseAction.Dispose();
        }
    }

}
