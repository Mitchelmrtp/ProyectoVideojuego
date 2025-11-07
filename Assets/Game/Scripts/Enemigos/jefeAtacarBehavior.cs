using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class jefeAtacarBehavior : StateMachineBehaviour
{
    private Mother mother;
    
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        mother = animator.GetComponent<Mother>();
        if (mother != null)
        {
            // LOGGING CRÍTICO PARA DETECTAR PROBLEMA
            float distanciaActual = Vector2.Distance(mother.transform.position, mother.jugador.position);
            
            Debug.Log("⚔️ Mother: Iniciando ataque - llamando MirarJugador()");
            Debug.Log($"🔍 DISTANCIA AL ATACAR: {distanciaActual:F3} unidades");
            
            // DETECTAR SI EL PROBLEMA ES DEL ANIMATOR
            if (distanciaActual > 25.0f)
            {
                Debug.LogError($"🚨 ANIMATOR CONTROLLER PROBLEMA: Ataque iniciado a {distanciaActual:F3} unidades");
                Debug.LogError($"💡 La transición Walk → Attack está mal configurada en Unity");
                Debug.LogError($"🔧 DEBE SER: 'distanciaJugador < 50' NO un valor pequeño como 2");
            }
            else if (distanciaActual < 5.0f)
            {
                Debug.LogError($"🚨 CONFIRMA PROBLEMA: Mother atacando a {distanciaActual:F3} - MUY CERCA para hechicera!");
                Debug.LogError($"⚠️ El Animator está usando valores de 100 PPU sin adaptar a 16 PPU");
            }
            
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
        // Mantener la dirección correcta durante todo el ataque
        if (mother != null)
        {
            mother.MirarJugador();
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
                Debug.LogError($"🔧 Verificar transición Attack → Walk: distanciaJugador > {mother.distanciaPostAtaque}");
            }
            
            // Asegurar que Mother mire al jugador al salir del ataque
            mother.MirarJugador();
        }
        
        Debug.Log("🎯 Mother: Estado Attack finalizado - debería mantener distancia de hechicera");
    }
}