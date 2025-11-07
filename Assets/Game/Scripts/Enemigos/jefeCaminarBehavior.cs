using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class jefeCaminarBehavior : StateMachineBehaviour
{
    public Mother Mother;
    public Rigidbody2D rb2D;
    [Header("Configuración para 16 pixels per unit")]
    [Tooltip("Velocidad recomendada: 0.4-1.2 para 16 PPU")]
    public float velocidadMovimiento = 0.8f; // Ajustado para 16 PPU (era ~5 en 100 PPU)
    
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
       Mother = animator.GetComponent<Mother>();
       if (Mother != null)
       {
           rb2D = Mother.rb2D;
           Mother.MirarJugador(); // Esta es la línea clave que falta en nuestro Mother
           Debug.Log("🚶 Mother: Iniciando comportamiento de caminar - llamando MirarJugador()");
       }
       else
       {
           Debug.LogError("❌ jefeCaminarBehavior: No se encontró componente Mother");
       }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (rb2D != null && Mother != null)
        {
            // Primero llamar MirarJugador para asegurar orientación correcta
            Mother.MirarJugador();
            
            // Calcular dirección hacia el jugador directamente
            if (Mother.jugador != null)
            {
                float distanciaActual = Vector2.Distance(Mother.transform.position, Mother.jugador.position);
                Vector2 directionToPlayer = (Mother.jugador.position - Mother.transform.position).normalized;
                
                // Movimiento normal hacia el jugador - dejar que el Animator maneje las transiciones
                rb2D.linearVelocity = new Vector2(directionToPlayer.x * velocidadMovimiento, rb2D.linearVelocity.y);
                
                // Log para debugging cada 60 frames (reducido)
                if (Time.frameCount % 60 == 0)
                {
                    Debug.Log($"🚶 Mother: Caminando - Distancia: {distanciaActual:F2}, Dir: {directionToPlayer.x:F2}, Vel: {rb2D.linearVelocity.x:F2}");
                }
            }
            else
            {
                // Fallback: usar transform.right si no hay referencia al jugador
                rb2D.linearVelocity = new Vector2(velocidadMovimiento, rb2D.linearVelocity.y) * animator.transform.right;
            }
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (rb2D != null)
        {
            rb2D.linearVelocity = new Vector2(0, rb2D.linearVelocity.y);
            Debug.Log("🛑 Mother: Finalizando comportamiento de caminar - deteniendo movimiento");
        }
    }
}