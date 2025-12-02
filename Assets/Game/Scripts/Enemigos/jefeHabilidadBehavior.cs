using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class jefeHabilidadBehavior : StateMachineBehaviour
{
    private Mother mother;
    private bool habilidadUsada = false;
    
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        mother = animator.GetComponent<Mother>();
        habilidadUsada = false; // Reset flag
        
        if (mother != null)
        {
            float distanciaActual = Vector2.Distance(mother.transform.position, mother.jugador.position);
            
            Debug.Log("⚡ Mother: Iniciando HABILIDAD - llamando MirarJugador()");
            Debug.Log($"🔍 DISTANCIA AL USAR HABILIDAD: {distanciaActual:F3} unidades");
            
            mother.MirarJugador();
        }
        else
        {
            Debug.LogError("❌ jefeHabilidadBehavior: No se encontró componente Mother");
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (mother == null) return;
        
        mother.MirarJugador();
        
        // Ejecutar habilidad al 70% de la animación (corresponde a ~0.75s del Animation Event original)
        float tiempoNormalizado = stateInfo.normalizedTime % 1;
        if (!habilidadUsada && tiempoNormalizado >= 0.7f)
        {
            Debug.Log($"⚡ EJECUTANDO USARHABILIDAD() en tiempo {tiempoNormalizado:F3}");
            mother.UsarHabilidad();
            habilidadUsada = true;
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (mother != null)
        {
            float distanciaAlSalir = Vector2.Distance(mother.transform.position, mother.jugador.position);
            Debug.Log($"✅ Mother: Finalizando HABILIDAD - distancia actual: {distanciaAlSalir:F2}");
            mother.MirarJugador();
        }
        
        Debug.Log("⚡ Mother: Estado Habilidad finalizado");
    }
}
