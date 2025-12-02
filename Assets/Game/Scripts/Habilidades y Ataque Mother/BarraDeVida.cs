using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BarraDeVida : MonoBehaviour
{
    private Slider slider;
    private float cantidadVida;
    public GameObject BarraVida;
    
    [Header("Animación de Daño")]
    public Image fillImage;  // La imagen del fill de la barra
    public Color colorNormal = Color.red;  // Color normal de la barra
    public Color colorHit = Color.white;  // Color al recibir daño
    public float hitFlashDuration = 0.4f;  // Duración del flash
    public float hitShakeAmount = 15f;  // Cantidad de sacudida
    public float hitShakeDuration = 0.3f;  // Duración de la sacudida
    public int shakeIntensity = 3;  // Número de sacudidas por frame
    
    private Color originalColor;
    private Vector3 originalPosition;
    private Coroutine hitAnimationRoutine;
    
    // Start is called before the first frame update
    void Start()
    {
        slider = GetComponent<Slider>();
        
        // Obtener la imagen del fill automáticamente si no está asignada
        if (fillImage == null && slider != null)
        {
            fillImage = slider.fillRect.GetComponent<Image>();
        }
        
        // Guardar color y posición originales
        if (fillImage != null)
        {
            originalColor = fillImage.color;
            if (colorNormal == Color.red && originalColor != Color.red)
            {
                colorNormal = originalColor;
            }
        }
        
        originalPosition = transform.localPosition;
    }
    
    public void CambiarVidaActual(float cantidadVida)
    {
        if (slider != null)
        {
            slider.value = cantidadVida;
        }
    }
    
    public void InicializarBarraVida(float vidaMaxima)
    {
        cantidadVida = vidaMaxima;
        if (slider != null)
        {
            slider.maxValue = vidaMaxima;
            slider.value = vidaMaxima;
        }
        Debug.Log($"💖 BarraDeVida: Inicializada con vida máxima: {vidaMaxima}");
    }
    
    // Método para animar la barra cuando recibe daño
    public void AnimarDaño()
    {
        if (hitAnimationRoutine != null)
        {
            StopCoroutine(hitAnimationRoutine);
        }
        hitAnimationRoutine = StartCoroutine(HitAnimation());
    }
    
    private IEnumerator HitAnimation()
    {
        // Flash de color más intenso con múltiples pulsos
        if (fillImage != null)
        {
            // Pulsar 2 veces para más notoriedad
            for (int i = 0; i < 2; i++)
            {
                fillImage.color = colorHit;
                yield return new WaitForSeconds(0.08f);
                fillImage.color = colorNormal;
                yield return new WaitForSeconds(0.08f);
            }
            fillImage.color = colorHit;
        }
        
        // Sacudida más pronunciada de la barra
        float elapsedTime = 0f;
        while (elapsedTime < hitShakeDuration)
        {
            // Sacudida más intensa con movimiento más amplio
            float shakeProgress = 1f - (elapsedTime / hitShakeDuration); // Decae con el tiempo
            float currentShakeAmount = hitShakeAmount * shakeProgress;
            
            float x = originalPosition.x + Random.Range(-currentShakeAmount, currentShakeAmount);
            float y = originalPosition.y + Random.Range(-currentShakeAmount, currentShakeAmount);
            transform.localPosition = new Vector3(x, y, originalPosition.z);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Restaurar posición original
        transform.localPosition = originalPosition;
        
        // Transición suave de vuelta al color normal con escala
        if (fillImage != null)
        {
            float fadeTime = 0f;
            Vector3 originalScale = transform.localScale;
            while (fadeTime < hitFlashDuration)
            {
                fadeTime += Time.deltaTime;
                float t = fadeTime / hitFlashDuration;
                
                // Color fade
                fillImage.color = Color.Lerp(colorHit, colorNormal, t);
                
                // Pequeño efecto de escala (pulso)
                float scale = 1f + (0.1f * (1f - t));
                transform.localScale = originalScale * scale;
                
                yield return null;
            }
            fillImage.color = colorNormal;
            transform.localScale = originalScale;
        }
    }
}
