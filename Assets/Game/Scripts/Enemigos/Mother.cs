using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Versión simplificada de Mother: mantiene solo lo esencial.
public class Mother : MonoBehaviour
{
    private Animator animator;
    public Rigidbody2D rb2D;
    public Transform jugador;

    [Header("Distancias (usadas sólo para referencia)")]
    public float distanciaDeteccion = 10f;
    public float distanciaAtaque = 3f;
    public float distanciaPerdida = 15f;
    public float distanciaPostAtaque = 4f;

    public GameObject ataque;
    public GameObject habilidad;
    public GameObject Llave;
    [Header("Projectile speeds (fallback if projectile has no controller)")]
    public float ataqueSpeed = 6f;
    public float habilidadSpeed = 4f;

    public float vida = 100f;
    public BarraDeVida barraDeVida;
    public GameObject BarraVida;

    private bool mirandoDerecha = true;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb2D = GetComponent<Rigidbody2D>();

        // Buscar jugador por tag 'Player' si no fue asignado en el Inspector
        if (jugador == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) jugador = p.transform;
        }

        // Inicializar barra de vida si está asignada
        if (barraDeVida != null)
        {
            barraDeVida.InicializarBarraVida(vida);
        }
        if (BarraVida != null)
        {
            BarraVida.SetActive(true);
        }
    }

    void Update()
    {
        if (jugador == null || animator == null) return;

        float distanciaJugador = Vector2.Distance(transform.position, jugador.position);
        animator.SetFloat("distanciaJugador", distanciaJugador);

        MirarJugador();
    }

    // Voltea el sprite según la posición del jugador
    public void MirarJugador()
    {
        if (jugador == null) return;
        bool jugadorALaDerecha = jugador.position.x > transform.position.x;
        if (jugadorALaDerecha != mirandoDerecha)
        {
            mirandoDerecha = jugadorALaDerecha;
            Vector3 s = transform.localScale;
            s.x = Mathf.Abs(s.x) * (mirandoDerecha ? 1f : -1f);
            transform.localScale = s;
        }
    }

    // Instancia el prefab de ataque (si existe)
    public void Atacar()
    {
        if (ataque == null) return;
        GameObject nuevo = Instantiate(ataque, transform.position, Quaternion.identity);

        // Preferir componente de control de ataque si existe
        var ataqueScript = nuevo.GetComponent<AtaqueNormal>();
        if (ataqueScript != null)
        {
            if (mirandoDerecha)
            {
                ataqueScript.SetDirection(Vector2.right);
                nuevo.transform.localScale = new Vector3(-1, 1, 1);
            }
            else
            {
                ataqueScript.SetDirection(Vector2.left);
                nuevo.transform.localScale = new Vector3(1, 1, 1);
            }
            return;
        }

        // Fallback: si tiene Rigidbody2D, darle una velocidad inicial
        var rb = nuevo.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            float dir = mirandoDerecha ? 1f : -1f;
            rb.linearVelocity = new Vector2(dir * ataqueSpeed, rb.linearVelocity.y);
            Vector3 s = nuevo.transform.localScale;
            s.x = Mathf.Abs(s.x) * (mirandoDerecha ? -1f : 1f);
            nuevo.transform.localScale = s;
        }
    }

    // Método llamado por AnimationEvent en la animación 'MotherHabilty'
    // Mantenerlo público y sin parámetros para que Unity pueda encontrarlo.
    public void UsarHabilidad()
    {
        if (habilidad == null) return;
        GameObject nueva = Instantiate(habilidad, transform.position, Quaternion.identity);

        var habilidadScript = nueva.GetComponent<AtaqueNormal>();
        if (habilidadScript != null)
        {
            if (mirandoDerecha)
            {
                habilidadScript.SetDirection(Vector2.right);
                nueva.transform.localScale = new Vector3(-1, 1, 1);
            }
            else
            {
                habilidadScript.SetDirection(Vector2.left);
                nueva.transform.localScale = new Vector3(1, 1, 1);
            }
            return;
        }

        var rb = nueva.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            float dir = mirandoDerecha ? 1f : -1f;
            rb.linearVelocity = new Vector2(dir * habilidadSpeed, rb.linearVelocity.y);
            Vector3 s = nueva.transform.localScale;
            s.x = Mathf.Abs(s.x) * (mirandoDerecha ? -1f : 1f);
            nueva.transform.localScale = s;
        }
    }

    public void TomarDaño(float daño)
    {
        vida -= daño;
        if (barraDeVida != null) barraDeVida.CambiarVidaActual(vida);

        if (vida <= 0f)
        {
            if (animator != null) animator.SetTrigger("Muerte");
            if (BarraVida != null) BarraVida.SetActive(false);
        }
        else
        {
            if (animator != null) animator.SetTrigger("Hit");
        }
    }

    // Llamar desde animación al morir
    public void Muerte()
    {
        if (Llave != null) Instantiate(Llave, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
 
