using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class jefeHitBehavior : StateMachineBehaviour
{
    private Mother mother;
    
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        mother = animator.GetComponent<Mother>();
        if (mother != null)
        {
            // Crucial: mirar al jugador al recibir daño
            mother.MirarJugador();
            Debug.Log("💥 Mother: Recibiendo daño - llamando MirarJugador()");
        }
        else
        {
            Debug.LogError("❌ jefeHitBehavior: No se encontró componente Mother");
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Mantener la dirección correcta durante la animación de daño
        if (mother != null)
        {
            mother.MirarJugador();
        }
    }
}