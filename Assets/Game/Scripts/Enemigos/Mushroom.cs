using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mushroom : Enemigo
{
    protected override float GetDefaultHealth()
    {
        return 1f; // Vida específica del Mushroom (más débil)
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
            
            Debug.Log($"Mushroom {gameObject.name} iniciando ataque");
        }
    }

    // Los mushrooms pueden tener comportamiento específico más lento
    protected override void PatrullaBehavior()
    {
        // Patrulla más lenta para los mushrooms
        if (animator != null)
            animator.SetBool("Running", false);
            
        Cronometro += 0.5f * Time.deltaTime; // Más lento que otros enemigos
        if (Cronometro >= 2) // Esperar más tiempo entre movimientos
        {
            Rutina = Random.Range(0, 2);
            Cronometro = 0;
        }
        
        switch (Rutina)
        {
            case 0:
                if (animator != null)
                    animator.SetBool("Running", false);
                break;
            case 1:
                direccion = Random.Range(0, 2);
                Rutina++;
                break;
            case 2:
                switch (direccion)
                {
                    case 0:
                        transform.rotation = Quaternion.Euler(0, 0, 0);
                        transform.Translate(Vector3.right * (speed_run * 0.7f) * Time.deltaTime); // Más lento
                        break;
                    case 1:
                        transform.rotation = Quaternion.Euler(0, 180, 0);
                        transform.Translate(Vector3.right * (speed_run * 0.7f) * Time.deltaTime); // Más lento
                        break;
                }
                if (animator != null)
                    animator.SetBool("Running", true);
                break;
        }
    }
}
