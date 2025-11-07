using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wings : Enemigo
{
    [Header("Vuelo")]
    public float alturaVuelo = 2f;
    public float velocidadVertical = 1f;
    
    protected override float GetDefaultHealth()
    {
        return 2f; // Vida específica del Wings
    }

    protected override void HandleGravityEffects()
    {
        // Los enemigos voladores no son afectados por la gravedad
        if (rb != null)
        {
            rb.gravityScale = 0f; // Sin gravedad para enemigos voladores
        }
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
            
            Debug.Log($"Wings {gameObject.name} iniciando ataque");
        }
    }

    protected override void PatrullaBehavior()
    {
        // Comportamiento de vuelo para patrulla
        if (animator != null)
            animator.SetBool("Running", false);
            
        Cronometro += 1 * Time.deltaTime;
        if (Cronometro >= 1)
        {
            Rutina = Random.Range(0, 3); // Añadir movimiento vertical
            Cronometro = 0;
        }
        
        switch (Rutina)
        {
            case 0:
                if (animator != null)
                    animator.SetBool("Running", false);
                break;
            case 1:
                direccion = Random.Range(0, 4); // 0-1 horizontal, 2-3 vertical
                Rutina++;
                break;
            case 2:
                switch (direccion)
                {
                    case 0:
                        transform.rotation = Quaternion.Euler(0, 0, 0);
                        transform.Translate(Vector3.right * speed_run * Time.deltaTime);
                        break;
                    case 1:
                        transform.rotation = Quaternion.Euler(0, 180, 0);
                        transform.Translate(Vector3.right * speed_run * Time.deltaTime);
                        break;
                    case 2:
                        transform.Translate(Vector3.up * velocidadVertical * Time.deltaTime);
                        break;
                    case 3:
                        transform.Translate(Vector3.down * velocidadVertical * Time.deltaTime);
                        break;
                }
                if (animator != null)
                    animator.SetBool("Running", true);
                break;
        }
    }

    protected override void MoveTowardsPlayer()
    {
        if (animator != null)
        {
            animator.SetBool("Running", true);
            animator.SetBool("Attack", false);
        }
        
        Vector3 direction = (Target.transform.position - transform.position).normalized;
        
        // Movimiento tanto horizontal como vertical hacia el jugador
        transform.Translate(direction * speed_run * Time.deltaTime);
        
        // Orientación basada en la dirección horizontal
        if (direction.x < 0)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        else if (direction.x > 0)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }
}
