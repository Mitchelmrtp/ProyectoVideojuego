using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skeleton : Enemigo
{
    protected override float GetDefaultHealth()
    {
        return 3f; // Vida específica del Skeleton
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
            
            Debug.Log($"Skeleton {gameObject.name} iniciando ataque");
        }
    }

    // Métodos específicos del Skeleton pueden ir aquí
}
