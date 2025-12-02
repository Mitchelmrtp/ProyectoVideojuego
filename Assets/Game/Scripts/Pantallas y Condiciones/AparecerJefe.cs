using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AparecerJefe : MonoBehaviour
{
    public GameObject Jefe;
    public GameObject BarraDeVida;

    [Header("🎵 Música del Jefe")]
    public AudioClip musicaJefe;   // <-- arrastra tu música aquí

    private void Start()
    {
        if (Jefe == null)
        {
            Debug.LogError("❌ AparecerJefe: Jefe no está asignado en el inspector");
        }
        else
        {
            Jefe.SetActive(false);
        }

        if (BarraDeVida == null)
        {
            Debug.LogWarning("⚠ AparecerJefe: BarraDeVida no está asignada en el inspector");
        }
        else
        {
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
                Jefe.SetActive(true);

            if (BarraDeVida != null)
                BarraDeVida.SetActive(true);

            // ⭐⭐ CAMBIO DE MÚSICA ⭐⭐
            if (musicaJefe != null)
            {
                MusicManager.Instance.PlayMusic(musicaJefe);
                Debug.Log("🎵 Música del Jefe activada");
            }
            else
            {
                Debug.LogWarning("⚠ No se asignó músicaJefe en el inspector.");
            }

            // Desactiva el trigger
            gameObject.SetActive(false);
        }
    }
}