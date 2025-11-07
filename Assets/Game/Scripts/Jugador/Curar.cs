using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Curar : MonoBehaviour
{
     public int curación;

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController jugador = other.GetComponent<PlayerController>();
        if (jugador != null && jugador.CompareTag("Player") && jugador.PuedeCurarse())
        {
            jugador.CurarVida(curación);
            Destroy(gameObject); // Destruir el objeto corazón después de curar
        }
    }
}
