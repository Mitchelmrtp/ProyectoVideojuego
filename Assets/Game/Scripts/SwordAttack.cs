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
    }

   

    public void AttackRight()
    {
        print("attack right");
        Debug.Log("SwordAttack: Ataque derecha - moviendo a posición derecha");
        swordCollider.enabled = true;
        transform.localPosition = rightAttackOffset;
    }

    public void AttactLeft()
    {
        print("attack left");
        Debug.Log("SwordAttack: Ataque izquierda - moviendo a posición izquierda");
        swordCollider.enabled = true;
        transform.localPosition = leftAttackOffset;
    }

    public void StopAttack()
    {
        Debug.Log("SwordAttack: Desactivando collider");
        swordCollider.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Enemy")
        {
            //deal damage with enemy
            Enemy enemy = collision.GetComponent<Enemy>();

            if (enemy != null)
            {
                enemy.Health -= damage;
            }
        }
    }


}
