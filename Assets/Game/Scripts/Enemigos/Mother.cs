using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Versión Mother con:
// - Intro de BOSS FINAL (llamado desde un trigger)
// - Panel de "Ganaste nivel 1" al morir
// - Titileo del sprite al recibir daño
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

    [Header("Ataques")]
    public GameObject ataque;
    public GameObject habilidad;
    public GameObject Llave;

    [Header("Projectile speeds (fallback si el proyectil no tiene script)")]
    public float ataqueSpeed = 6f;
    public float habilidadSpeed = 4f;

    [Header("Vida")]
    public float vida = 100f;
    public BarraDeVida barraDeVida;
    public GameObject BarraVida;   // Barra de vida del boss (GameObject de UI)

    [Header("UI Boss")]
    public GameObject panelBossIntro;  // Panel "BOSS FINAL"

    [Header("UI Victoria")]
    public GameObject panelVictoria;   // Panel "Ganaste nivel 1"

    [Header("Feedback de Daño")]
    public SpriteRenderer spriteRenderer;   // Sprite del boss
    public float flashDuration = 0.15f;     // duración total del titileo
    public float flashInterval = 0.05f;     // intervalo entre on/off
    private Coroutine flashRoutine;

    private bool mirandoDerecha = true;

    // Variables para respawn y estado inicial
    private Vector3 originalPosition;
    private float originalVida;
    private bool originalMirandoDerecha;
    private bool isDead = false;

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

        // La barra de vida del boss empieza OCULTA hasta que se active el combate
        if (BarraVida != null)
        {
            BarraVida.SetActive(false);
        }

        // Aseguramos que los paneles de UI estén apagados al inicio
        if (panelVictoria != null)
            panelVictoria.SetActive(false);

        if (panelBossIntro != null)
            panelBossIntro.SetActive(false);

        // Guardar estado inicial para respawn
        originalPosition = transform.position;
        originalVida = vida;
        originalMirandoDerecha = mirandoDerecha;
        isDead = false;

        Debug.Log($"Mother inicializada - Posición: {originalPosition}, Vida: {originalVida}");
    }

    void Update()
    {
        if (jugador == null || animator == null || isDead) return;

        float distanciaJugador = Vector2.Distance(transform.position, jugador.position);
        animator.SetFloat("distanciaJugador", distanciaJugador);

        MirarJugador();
    }

    // Voltea el sprite según la posición del jugador
    public void MirarJugador()
    {
        if (jugador == null || isDead) return;
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
        if (ataque == null || isDead) return;
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
    public void UsarHabilidad()
    {
        if (habilidad == null || isDead) return;
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
        if (isDead) return; // Evitar daño cuando ya está muerto
        
        vida -= daño;
        if (barraDeVida != null) barraDeVida.CambiarVidaActual(vida);

        // Titileo cada vez que recibe daño
        if (flashRoutine != null)
            StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashOnHit());

        if (vida <= 0f)
        {
            if (!isDead) // Solo trigger si no estaba muerto antes
            {
                isDead = true;
                if (animator != null) animator.SetTrigger("Muerte");
                if (BarraVida != null) BarraVida.SetActive(false);
            }
        }
        else
        {
            if (animator != null) animator.SetTrigger("Hit");
        }
    }

    // Llamar desde animación al morir (Animation Event)
    public void Muerte()
    {
        if (Llave != null) Instantiate(Llave, transform.position, Quaternion.identity);

        // Activar mensaje de victoria
        if (panelVictoria != null)
        {
            panelVictoria.SetActive(true);
        }

        Destroy(gameObject);
    }

    // Método llamado por el trigger para iniciar la intro de boss
    public void ShowBossIntro()
    {
        if (isDead) return;

        if (panelBossIntro != null)
        {
            StartCoroutine(MostrarIntroBoss());
        }

        // Activar barra de vida cuando empieza el combate
        if (BarraVida != null)
        {
            BarraVida.SetActive(true);
        }
    }

    // Coroutine para mostrar el cartel de "BOSS FINAL"
    private IEnumerator MostrarIntroBoss()
    {
        panelBossIntro.SetActive(true);
        yield return new WaitForSeconds(3f); // tiempo que se ve el mensaje
        panelBossIntro.SetActive(false);
    }

    // Titileo del sprite al recibir daño
    private IEnumerator FlashOnHit()
    {
        if (spriteRenderer == null)
            yield break;

        float elapsed = 0f;
        bool visible = true;

        while (elapsed < flashDuration)
        {
            visible = !visible;
            spriteRenderer.enabled = visible;

            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }

        // Aseguramos que quede visible al final
        spriteRenderer.enabled = true;
        flashRoutine = null;
    }

    // Método público para respawn/reinicio
    public void Respawn()
    {
        // Restaurar posición y rotación
        transform.position = originalPosition;
        transform.rotation = Quaternion.identity;
        
        // Restaurar vida y estado
        vida = originalVida;
        isDead = false;
        
        // Restaurar dirección
        mirandoDerecha = originalMirandoDerecha;
        
        // Reactivar gameObject si estaba desactivado
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
        
        // Reactivar colliders
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (var col in colliders)
        {
            col.enabled = true;
        }
        
        // Resetear animator
        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
            animator.ResetTrigger("Muerte");
            animator.ResetTrigger("Hit");
        }
        
        // Restaurar barra de vida (apagada hasta que empiece el combate otra vez)
        if (barraDeVida != null)
        {
            barraDeVida.InicializarBarraVida(vida);
        }
        
        if (BarraVida != null)
        {
            BarraVida.SetActive(false);
        }

        // Asegurar sprite visible y sin titileo
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }

        // Apagar paneles de UI
        if (panelBossIntro != null)
            panelBossIntro.SetActive(false);

        if (panelVictoria != null)
            panelVictoria.SetActive(false);
        
        // Limpiar velocidad si tiene Rigidbody2D
        if (rb2D != null)
        {
            rb2D.linearVelocity = Vector2.zero;
            rb2D.angularVelocity = 0f;
        }
        
        Debug.Log($"Mother: Respawn completado - Posición: {transform.position}, Vida: {vida}, isDead: {isDead}");
    }
}
