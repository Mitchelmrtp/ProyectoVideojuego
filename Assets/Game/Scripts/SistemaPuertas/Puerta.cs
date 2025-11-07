using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puerta : MonoBehaviour
{
    private Animator animator;
    private BoxCollider2D boxCollider;

    private void Start()
    {
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    public void AbrirPuerta()
    {
        if (animator != null)
        {
            animator.SetTrigger("AbrirPuerta"); // Suponiendo que tienes un trigger "AbrirPuerta" configurado en el Animator
        }

        // Desactivar el BoxCollider2D de la puerta para permitir el paso
        if (boxCollider != null)
        {
            boxCollider.enabled = false;
        }
    }
}
