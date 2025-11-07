using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Default")]
    [Tooltip("Pista que sonará al inicio si no hay otra solicitud.")]
    public AudioClip defaultClip;

    [Range(0f, 1f)]
    public float volume = 0.6f;

    [Header("Crossfade")]
    [Tooltip("Duración del fundido entre pistas.")]
    public float defaultFadeSeconds = 1.5f;

    private AudioSource _a;   // activo
    private AudioSource _b;   // siguiente (para crossfade)
    private AudioSource _current;

    void Awake()
    {
        // Singleton + persistencia
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Configurar dos AudioSource para crossfade
        _a = gameObject.AddComponent<AudioSource>();
        _b = gameObject.AddComponent<AudioSource>();

        foreach (var src in new[] { _a, _b })
        {
            src.playOnAwake = false;
            src.loop = true;
            src.volume = 0f; // empezamos silenciados
        }

        _current = _a;
    }

    void Start()
    {
        if (defaultClip != null)
        {
            PlayMusic(defaultClip, defaultFadeSeconds);
        }
    }

    /// <summary>
    /// Reproduce una pista con loop. Si ya está sonando esa misma, no hace nada.
    /// </summary>
    public void PlayMusic(AudioClip clip, float fadeSeconds = -1f)
    {
        if (clip == null) return;
        if (fadeSeconds < 0f) fadeSeconds = defaultFadeSeconds;

        // Si ya suena ese clip, salir
        if (_current.clip == clip) return;

        // Elegir el "otro" source para el crossfade
        AudioSource next = (_current == _a) ? _b : _a;

        next.clip = clip;
        next.volume = 0f;
        next.loop = true;
        next.Play();

        StopAllCoroutines();
        StartCoroutine(Crossfade(_current, next, fadeSeconds));

        _current = next;
    }

    /// <summary>
    /// Cambia el volumen master de la música [0..1]
    /// </summary>
    public void SetVolume(float v)
    {
        volume = Mathf.Clamp01(v);
        // Ajusta el volumen del source activo; el crossfade maneja los dos si está ocurriendo
        if (_current != null) _current.volume = volume;
    }

    private IEnumerator Crossfade(AudioSource from, AudioSource to, float seconds)
    {
        float t = 0f;
        while (t < seconds)
        {
            t += Time.unscaledDeltaTime; // para que no le afecte el timescale
            float k = Mathf.Clamp01(t / seconds);
            if (from != null) from.volume = Mathf.Lerp(volume, 0f, k);
            if (to   != null) to.volume   = Mathf.Lerp(0f, volume, k);
            yield return null;
        }

        // Terminar estados
        if (from != null)
        {
            from.Stop();
            from.volume = 0f;
            from.clip = null;
        }
        if (to != null)
        {
            to.volume = volume;
        }
    }
}
