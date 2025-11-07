using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonSFX : MonoBehaviour,
    IPointerEnterHandler,        // mouse hover
    IPointerClickHandler,        // mouse click
    ISelectHandler,              // selección por teclado/control
    ISubmitHandler               // Enter/A botón
{
    [Header("Clips")]
    public AudioClip hoverClip;   // sonido al pasar o seleccionar
    public AudioClip clickClip;   // sonido al hacer clic/submit

    [Header("Volúmenes")]
    [Range(0f, 1f)] public float hoverVolume = 0.8f;
    [Range(0f, 1f)] public float clickVolume = 1f;

    // Mouse entra al botón
    public void OnPointerEnter(PointerEventData eventData)
    {
        Play(hoverClip, hoverVolume);
    }

    // Mouse clic
    public void OnPointerClick(PointerEventData eventData)
    {
        Play(clickClip, clickVolume);
    }

    // Selección por teclado/control (cuando el foco llega al botón)
    public void OnSelect(BaseEventData eventData)
    {
        Play(hoverClip, hoverVolume);
    }

    // Confirmación por teclado/control (Enter/Space o A en gamepad)
    public void OnSubmit(BaseEventData eventData)
    {
        Play(clickClip, clickVolume);
    }

    private void Play(AudioClip clip, float vol)
    {
        if (UIAudioHub.Instance != null && clip != null)
            UIAudioHub.Instance.PlayOneShot(clip, vol);
    }
}
