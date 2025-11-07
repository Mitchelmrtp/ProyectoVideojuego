using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemigo : MonoBehaviour
{
    [Header("Vida del Enemigo")]
    public float vida = 3f;
    
    [Header("Comportamiento")]
    public int Rutina;
    public float Cronometro;
    public int direccion;
    public float speed_run = 2f;
    public bool Ataque;
    
    [Header("Detección del Jugador")]
    public GameObject Target;
    public float rango_vision = 5f;
    public float rango_ataque = 1.5f;
    
    [Header("Combate")]
    public GameObject Rango;
    public GameObject HitPj;

    // Componentes
    protected Animator animator;
    protected Rigidbody2D rb;
    protected SpriteRenderer spriteRenderer;
    
    // Variables para respawn y gestión
    private string enemyId;
    private Vector3 originalPosition;
    private bool isDead = false;

    protected virtual void Start()
    {
        // Buscar al jugador por tag en lugar de por nombre
        if (Target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Target = player;
                Debug.Log($"Enemigo {gameObject.name}: Jugador encontrado por tag 'Player'");
            }
            else
            {
                Debug.LogError($"Enemigo {gameObject.name}: No se encontró jugador con tag 'Player'");
            }
        }
        
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Guardar posición inicial para respawn
        originalPosition = transform.position;
        
        // Generar ID único para el sistema de enemigos
        enemyId = GenerateEnemyId();
        
        // Registrar en el EnemyManager
        EnemyManager.RegisterEnemy(enemyId, this);
        
        Debug.Log($"Enemigo {gameObject.name} iniciado. ID: {enemyId}, Vida: {vida}");
    }

    protected virtual void Update()
    {
        if (!isDead && Target != null)
        {
            Comportamientos();
            
            // Aplicar efectos de gravedad si es necesario
            HandleGravityEffects();
        }
    }

    protected virtual void HandleGravityEffects()
    {
        // Los enemigos pueden ser afectados por la gravedad del ambiente
        // Este método puede ser sobrescrito por enemigos específicos
        if (rb != null)
        {
            // Mantener gravedad normal por defecto, a menos que se especifique lo contrario
            rb.gravityScale = 1f;
        }
    }

    public void final_Animation(){
        if (animator != null)
        {
            animator.SetBool("Attack", false);
        }
        Ataque = false;
        
        if (Rango != null)
        {
            var rangeCollider = Rango.GetComponent<CapsuleCollider2D>();
            if (rangeCollider != null)
                rangeCollider.enabled = true;
        }
    }
    
    public void ColliderWeaponTrue(){
        if (HitPj != null)
        {
            var hitCollider = HitPj.GetComponent<CapsuleCollider2D>();
            if (hitCollider != null)
                hitCollider.enabled = true;
        }
    }
    
    public void ColliderWeaponFalse(){
        if (HitPj != null)
        {
            var hitCollider = HitPj.GetComponent<CapsuleCollider2D>();
            if (hitCollider != null)
                hitCollider.enabled = false;
        }
    }

    //Comportamiento de ataque

    public virtual void TomarDaño(float daño)
    {
        if (isDead) return;
        
        vida -= daño;
        Debug.Log($"Enemigo {gameObject.name} recibió {daño} de daño. Vida restante: {vida}");
        
        if (vida <= 0)
        {
            Muerte();
        }
        else
        {
            Hit();
        }
    }

    protected virtual void Muerte()
    {
        if (isDead) return;
        
        isDead = true;
        Debug.Log($"Enemigo {gameObject.name} murió");
        
        if (animator != null)
            animator.SetTrigger("Muerte");
            
        // Desregistrar del EnemyManager
        EnemyManager.UnregisterEnemy(enemyId);
        
        // Desactivar colliders para evitar más interacciones
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }
        
        // Desactivar componentes de ataque si existen
        if (Rango != null)
        {
            var rangeCollider = Rango.GetComponent<Collider2D>();
            if (rangeCollider != null)
                rangeCollider.enabled = false;
        }
        
        if (HitPj != null)
        {
            var hitCollider = HitPj.GetComponent<Collider2D>();
            if (hitCollider != null)
                hitCollider.enabled = false;
        }
    }

    protected virtual void Hit()
    {
        Debug.Log($"Enemigo {gameObject.name} recibió un golpe");
        if (animator != null)
            animator.SetTrigger("Hit");
    }

    public virtual void Destroy()
    {
        EnemyManager.UnregisterEnemy(enemyId);
        Destroy(gameObject);
    }

    public virtual void Comportamientos()
    {
        if (Target == null) return;
        
        // Verificar si el jugador está muriendo/muerto - detener ataques
        PlayerController playerController = Target.GetComponent<PlayerController>();
        if (playerController != null && (playerController.isDying || playerController.currentHealth <= 0))
        {
            // El jugador está muriendo/muerto, solo patrullar
            PatrullaBehavior();
            return;
        }
        
        float distanciaAlJugador = Mathf.Abs(transform.position.x - Target.transform.position.x);
        
        if (distanciaAlJugador > rango_vision && !Ataque)
        {
            // Comportamiento de patrulla cuando el jugador está lejos
            PatrullaBehavior();
        }
        else
        {
            // Comportamiento de persecución/ataque cuando el jugador está cerca
            if (distanciaAlJugador < rango_ataque && !Ataque)
            {
                // Acercarse al jugador para atacar
                MoveTowardsPlayer();
            }
            else if (distanciaAlJugador <= rango_ataque)
            {
                // Ejecutar ataque si está en rango
                AttemptAttack();
            }
            else
            {
                // Perseguir al jugador
                ChasePlayer();
            }
        }
    }

    protected virtual void PatrullaBehavior()
    {
        if (animator != null)
            animator.SetBool("Running", false);
            
        Cronometro += 1 * Time.deltaTime;
        if (Cronometro >= 1)
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
                        transform.Translate(Vector3.right * speed_run * Time.deltaTime);
                        break;
                    case 1:
                        transform.rotation = Quaternion.Euler(0, 180, 0);
                        transform.Translate(Vector3.right * speed_run * Time.deltaTime);
                        break;
                }
                if (animator != null)
                    animator.SetBool("Running", true);
                break;
        }
    }

    protected virtual void MoveTowardsPlayer()
    {
        if (animator != null)
        {
            animator.SetBool("Running", true);
            animator.SetBool("Attack", false);
        }
        
        if (transform.position.x < Target.transform.position.x)
        {
            transform.Translate(Vector3.right * speed_run * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else
        {
            transform.Translate(Vector3.right * speed_run * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
    }

    protected virtual void ChasePlayer()
    {
        if (!Ataque)
        {
            if (transform.position.x < Target.transform.position.x)
            {
                transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            else
            {
                transform.rotation = Quaternion.Euler(0, 180, 0);
            }
            
            if (animator != null)
                animator.SetBool("Running", false);
        }
    }

    protected virtual void AttemptAttack()
    {
        // Este método debe ser implementado por cada enemigo específico
        Debug.Log($"Enemigo {gameObject.name} intenta atacar");
    }

    // Sistema de respawn
    public virtual void Respawn()
    {
        if (gameObject == null) return;
        
        Debug.Log($"Respawning enemigo {gameObject.name}");
        
        // Restaurar posición y estado
        transform.position = originalPosition;
        transform.rotation = Quaternion.identity;
        vida = GetDefaultHealth(); // Cada enemigo puede definir su vida por defecto
        isDead = false;
        Ataque = false;
        
        // Reactivar colliders
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (var col in colliders)
        {
            col.enabled = true;
        }
        
        // Reactivar gameObject si estaba desactivado
        gameObject.SetActive(true);
        
        // Resetear animator
        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }
        
        Debug.Log($"Enemigo {gameObject.name} respawneado. Vida: {vida}");
    }

    // Método para obtener la vida por defecto (debe ser sobrescrito)
    protected virtual float GetDefaultHealth()
    {
        return 3f; // Valor por defecto
    }

    private string GenerateEnemyId()
    {
        return $"{gameObject.name}_{GetInstanceID()}_{Time.time}";
    }

    protected virtual void OnDrawGizmosSelected()
    {
        // Dibujar rango de visión
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rango_vision);
        
        // Dibujar rango de ataque
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rango_ataque);
    }
}