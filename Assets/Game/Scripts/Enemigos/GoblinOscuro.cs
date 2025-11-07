using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoblinOscuro : Enemigo
{
    protected override float GetDefaultHealth()
    {
        return 2f; // Vida específica del GoblinOscuro
    }

    protected override void AttemptAttack()
    {
        if (!Ataque)
        {
            Ataque = true;
            if (animator != null)
                animator.SetBool("Attack", true);
                
            // Desactivar el rango mientras ataca
            if (Rango != null)
            {
                var rangeCollider = Rango.GetComponent<CapsuleCollider2D>();
                if (rangeCollider != null)
                    rangeCollider.enabled = false;
            }
            
            Debug.Log($"GoblinOscuro {gameObject.name} iniciando ataque");
        }
    }

    // Métodos específicos del GoblinOscuro pueden ir aquí
}
