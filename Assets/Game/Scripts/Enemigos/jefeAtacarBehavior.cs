using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class jefeAtacarBehavior : StateMachineBehaviour
{
    private Mother mother;
    private bool atacado = false;
    
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        mother = animator.GetComponent<Mother>();
        atacado = false; // Reset flag
        
        if (mother != null)
        {
            float distanciaActual = Vector2.Distance(mother.transform.position, mother.jugador.position);
            
            Debug.Log("⚔️ Mother: Iniciando ataque - llamando MirarJugador()");
            Debug.Log($"🔍 DISTANCIA AL ATACAR: {distanciaActual:F3} unidades");
            
            // Crucial: mirar al jugador antes de atacar
            mother.MirarJugador();
        }
        else
        {
            Debug.LogError("❌ jefeAtacarBehavior: No se encontró componente Mother");
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (mother == null) return;
        
        // Mantener la dirección correcta durante todo el ataque
        mother.MirarJugador();
        
        // Ejecutar el ataque cuando la animación llegue al 50% (frame perfecto para disparar)
        float tiempoNormalizado = stateInfo.normalizedTime % 1;
        if (!atacado && tiempoNormalizado >= 0.5f)
        {
            Debug.Log($"🎯 EJECUTANDO ATACAR() en tiempo {tiempoNormalizado:F3} ({(tiempoNormalizado * 1.0833334f):F3}s)");
            mother.Atacar();
            atacado = true; // Evitar múltiples llamadas
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (mother != null)
        {
            float distanciaAlSalir = Vector2.Distance(mother.transform.position, mother.jugador.position);
            Debug.Log($"✅ Mother: Finalizando ataque - distancia actual: {distanciaAlSalir:F2}");
            
            // VERIFICAR SI MOTHER ESTÁ MUY CERCA AL SALIR DEL ATAQUE
            if (distanciaAlSalir < 20.0f)
            {
                Debug.LogError($"🚨 PROBLEMA POST-ATAQUE: Mother muy cerca ({distanciaAlSalir:F2}) al salir de Attack");
                Debug.LogError($"💡 Debería alejarse para mantener distancia de hechicera (>50 unidades)");
                Debug.LogError($"🔧 Verificar transición Attack → Walk en el Animator Controller");
            }
            
            // Asegurar que Mother mire al jugador al salir del ataque
            mother.MirarJugador();
        }
        
        Debug.Log("🎯 Mother: Estado Attack finalizado");
    }
}