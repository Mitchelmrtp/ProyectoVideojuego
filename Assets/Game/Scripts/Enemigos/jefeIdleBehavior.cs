using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class jefeIdleBehavior : StateMachineBehaviour
{
    private Mother mother;
    
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Generar número aleatorio para decidir el próximo ataque
        animator.SetInteger("NumeroAleatorio", Random.Range(0, 2));
        
        // Obtener referencia a Mother y llamar MirarJugador
        mother = animator.GetComponent<Mother>();
        if (mother != null)
        {
            mother.MirarJugador();
            Debug.Log("🧘 Mother: Estado Idle - llamando MirarJugador()");
        }
        else
        {
            Debug.LogError("❌ jefeIdleBehavior: No se encontró componente Mother");
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Seguir mirando al jugador durante el estado Idle
        if (mother != null)
        {
            mother.MirarJugador();
        }
    }
}