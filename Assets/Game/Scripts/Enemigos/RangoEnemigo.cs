using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangoEnemigo : MonoBehaviour
{
    public Animator Animator;
    public Enemigo Enemigo;
    public Mother Mother; // Añadir referencia a Mother para jefes
    
    void OnTriggerEnter2D(Collider2D Collider)
    {
        // Buscar con tag "Player" primero
        if (Collider.CompareTag("Player"))
        {
            Debug.Log("🎯 RangoEnemigo: Jugador detectado, activando combate");
            
            if (Animator != null)
            {
                Animator.SetBool("Running", false);
                Animator.SetBool("Attack", true);
                Debug.Log("🎭 RangoEnemigo: Animaciones de combate activadas");
            }

            // Para enemigos normales
            if (Enemigo != null)
            {
                Enemigo.Ataque = true;
                Debug.Log("⚔️ RangoEnemigo: Modo ataque activado para Enemigo");
            }
            
            // Para Mother (jefe)
            if (Mother != null)
            {
                Debug.Log("👹 RangoEnemigo: Mother detecta al jugador, iniciando combate");
                // Mother ya maneja su propia lógica de combate basada en distancia
            }
            
            // Desactivar el collider para evitar múltiples activaciones
            CapsuleCollider2D capsuleCollider = GetComponent<CapsuleCollider2D>();
            if (capsuleCollider != null)
            {
                capsuleCollider.enabled = false;
                Debug.Log("🔒 RangoEnemigo: Collider desactivado");
            }
        }
        // Mantener compatibilidad con tag "Dark"
        else if (Collider.CompareTag("Dark"))
        {
            Debug.Log("🎯 RangoEnemigo: Jugador detectado (tag Dark - compatibilidad)");
            
            if (Animator != null)
            {
                Animator.SetBool("Running", false);
                Animator.SetBool("Attack", true);
            }

            if (Enemigo != null)
            {
                Enemigo.Ataque = true;
            }
            
            GetComponent<CapsuleCollider2D>().enabled = false;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
