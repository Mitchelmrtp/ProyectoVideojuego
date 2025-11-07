using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AparecerJefe : MonoBehaviour
{
    public GameObject Jefe;
    public GameObject BarraDeVida;

    private void Start()
    {
        // Verificar que las referencias estén asignadas
        if (Jefe == null)
        {
            Debug.LogError("❌ AparecerJefe: Jefe no está asignado en el inspector");
        }
        else
        {
            Debug.Log($"✅ AparecerJefe: Jefe asignado: {Jefe.name}");
            // Asegurar que el jefe esté inicialmente desactivado
            Jefe.SetActive(false);
        }
        
        if (BarraDeVida == null)
        {
            Debug.LogWarning("⚠️ AparecerJefe: BarraDeVida no está asignada en el inspector");
        }
        else
        {
            Debug.Log($"✅ AparecerJefe: BarraDeVida asignada: {BarraDeVida.name}");
            // Asegurar que la barra esté inicialmente desactivada
            BarraDeVida.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"🔍 AparecerJefe: Objeto detectado: {other.name} con tag: {other.tag}");
        
        if (other.CompareTag("Player"))
        {
            Debug.Log("🎯 AparecerJefe: ¡Jugador detectado! Activando jefe...");
            
            if (Jefe != null)
            {
                Jefe.SetActive(true);
                Debug.Log("👹 AparecerJefe: Jefe activado");
            }
            
            if (BarraDeVida != null)
            {
                BarraDeVida.SetActive(true);
                Debug.Log("📊 AparecerJefe: Barra de vida activada");
            }
            
            Debug.Log("🔒 AparecerJefe: Desactivando trigger...");
            gameObject.SetActive(false); // Desactiva el GameObject que contiene este script y el collider
        }
        else
        {
            Debug.Log($"⚠️ AparecerJefe: Objeto ignorado (tag: {other.tag})");
        }
    }
}
