using UnityEngine;
using UnityEngine.Audio;

public class UIAudioHub : MonoBehaviour
{
    public static UIAudioHub Instance { get; private set; }

    [Header("Output (opcional)")]
    public AudioMixerGroup outputMixerGroup;

    private AudioSource _src;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _src = GetComponent<AudioSource>();
        if (_src == null) _src = gameObject.AddComponent<AudioSource>();
        _src.playOnAwake = false;
        _src.spatialBlend = 0f;
        if (outputMixerGroup) _src.outputAudioMixerGroup = outputMixerGroup;
    }

    public void PlayOneShot(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        _src.PlayOneShot(clip, Mathf.Clamp01(volume));
    }
}
