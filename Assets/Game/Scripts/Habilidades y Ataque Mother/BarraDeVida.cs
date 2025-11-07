using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BarraDeVida : MonoBehaviour
{
    private Slider slider;
    private float cantidadVida;
    public GameObject BarraVida;
    // Start is called before the first frame update
    void Start()
    {
        slider = GetComponent<Slider>();
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
}
