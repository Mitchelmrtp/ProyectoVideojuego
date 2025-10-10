using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuPlayFlow : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Panel que se mostrará antes de cambiar de escena (ej. 'Preparándote...').")]
    public GameObject preTransitionPanel;

    [Header("Transición")]
    [Tooltip("Segundos a esperar antes de cargar la siguiente escena.")]
    [Min(0f)] public float waitSeconds = 5f;

    [Tooltip("Si lo activas, cargará la escena por nombre. Si lo desactivas, usará build index + 1.")]
    public bool useSceneName = false;

    [Tooltip("Nombre de la escena a cargar (si 'useSceneName' está activo).")]
    public string nextSceneName;

    [Tooltip("Permitir saltar la espera con cualquier tecla/clic táctil/clic mouse.")]
    public bool allowSkip = false;

    private bool _inProgress = false;

    // Llama este método desde el botón Play del menú
    public void OnPlayPressed()
    {
        if (_inProgress) return;
        _inProgress = true;

        // Mostrar panel si existe
        if (preTransitionPanel != null)
            preTransitionPanel.SetActive(true);

        // Opcional: bloquear tiempo o input adicional aquí si lo necesitas
        StartCoroutine(FlowCoroutine());
    }

    private System.Collections.IEnumerator FlowCoroutine()
    {
        float elapsed = 0f;

        while (elapsed < waitSeconds)
        {
            if (allowSkip && (Input.anyKeyDown || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)))
            {
                break; // Salta la espera
            }

            elapsed += Time.unscaledDeltaTime; // usa unscaled por si pausas Time.timeScale
            yield return null;
        }

        LoadNextScene();
    }

    private void LoadNextScene()
    {
        if (useSceneName && !string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            // Cargar la siguiente en el Build Settings (asegúrate del orden)
            int current = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(current + 1);
        }
    }
}
