using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordAttack : MonoBehaviour
{
    public Collider2D swordCollider;

    public float damage = 3f;
    Vector2 rightAttackOffset;
    Vector2 leftAttackOffset;

    // Start is called before the first frame update
    void Start()
    {
        // Guardamos la posición inicial como posición de ataque derecho
        rightAttackOffset = transform.localPosition;
        // La posición de ataque izquierdo es el espejo horizontal
        leftAttackOffset = new Vector2(-rightAttackOffset.x, rightAttackOffset.y);
        
        Debug.Log($"SwordAttack iniciado. Posición derecha: {rightAttackOffset}, Posición izquierda: {leftAttackOffset}");

        // Asegurarnos de que el collider esté desactivado al iniciar para evitar detecciones fuera del ataque
        if (swordCollider == null)
        {
            Debug.LogWarning("SwordAttack: swordCollider no está asignado en el inspector.");
        }
        else
        {
            swordCollider.enabled = false;
        }
    }

   

    public void AttackRight()
    {
        print("attack right");
        Debug.Log("SwordAttack: Ataque derecha - moviendo a posición derecha");
        if (swordCollider == null)
        {
            Debug.LogError("SwordAttack.AttackRight: swordCollider es null. Asigna el collider en el inspector.");
        }
        else
        {
            swordCollider.enabled = true;
        }
        transform.localPosition = rightAttackOffset;
    }

    // Método con el nombre correcto
    public void AttackLeft()
    {
        print("attack left");
        Debug.Log("SwordAttack: Ataque izquierda - moviendo a posición izquierda");
        if (swordCollider == null)
        {
            Debug.LogError("SwordAttack.AttackLeft: swordCollider es null. Asigna el collider en el inspector.");
        }
        else
        {
            swordCollider.enabled = true;
        }
        transform.localPosition = leftAttackOffset;
    }

    // Mantener la versión con typo por compatibilidad con llamadas existentes
    public void AttactLeft()
    {
        AttackLeft();
    }

    public void StopAttack()
    {
        Debug.Log("SwordAttack: Desactivando collider");
        swordCollider.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();

            if (enemy != null)
            {
                Debug.Log($"SwordAttack: Atacando enemigo {enemy.name}, daño: {damage}");
                enemy.TakeDamage(damage); // Usar la nueva función que activa animaciones
            }
        }
    }


}
