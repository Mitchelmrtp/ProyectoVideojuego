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
        Debug.Log($"🗡️ SwordAttack: Colisión detectada con {collision.name}, tag: '{collision.tag}'");
        
        // Verificar si es un enemigo normal con tag "Enemy"
        if (collision.CompareTag("Enemy"))
        {
            Debug.Log($"✅ SwordAttack: Confirmado tag 'Enemy', buscando scripts...");
            
            // Intentar con el script Enemigo (enemigos normales)
            Enemigo enemigo = collision.GetComponent<Enemigo>();
            if (enemigo != null)
            {
                Debug.Log($"🎯 SwordAttack: Atacando Enemigo {enemigo.name}, daño: {damage}");
                enemigo.TomarDaño(damage);
                return;
            }

            // Intentar con el script Slime si tiene tag Enemy
            Slime slime = collision.GetComponent<Slime>();
            if (slime != null)
            {
                Debug.Log($"� SwordAttack: Atacando Slime {slime.name}, daño: {damage}");
                slime.TakeDamage(damage);
                return;
            }

            // Intentar con el script Mother (MotherEnemy) si tiene tag Enemy
            Mother motherEnemy = collision.GetComponent<Mother>();
            if (motherEnemy != null)
            {
                Debug.Log($"� SwordAttack: Atacando MotherEnemy {motherEnemy.name}, daño: {damage}");
                motherEnemy.TomarDaño(damage);
                return;
            }

            // Si no encuentra ninguno de los scripts, mostrar advertencia
            Debug.LogWarning($"❌ SwordAttack: Objeto con tag 'Enemy' ({collision.name}) no tiene script Enemigo, Slime ni Mother");
        }
        // Verificar si es un jefe con tag "Jefe"
        else if (collision.CompareTag("Jefe"))
        {
            Debug.Log($"👑 SwordAttack: Confirmado tag 'Jefe', buscando script Mother...");
            
            // Buscar el script Mother en el objeto con tag "Jefe"
            Mother motherEnemy = collision.GetComponent<Mother>();
            if (motherEnemy != null)
            {
                Debug.Log($"✨ SwordAttack: Atacando Jefe (Mother) {motherEnemy.name}, daño: {damage}");
                motherEnemy.TomarDaño(damage);
                return;
            }
            else
            {
                Debug.LogWarning($"❌ SwordAttack: Objeto con tag 'Jefe' ({collision.name}) no tiene script Mother");
            }
        }
        else
        {
            Debug.Log($"ℹ️ SwordAttack: Tag '{collision.tag}' no reconocido para ataque");
        }
    }
}
