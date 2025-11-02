using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float speedX;
    [Header("Patrol Limits (distances from start position)")]
    [SerializeField] private float limitLeft;   // Distancia hacia la izquierda
    [SerializeField] private float limitRight;  // Distancia hacia la derecha
    
    [Header("Player Detection")]
    [SerializeField] private float detectionRange = 3f; // Distancia para detectar al jugador
    [SerializeField] private float fleeRange = 2f; // Distancia para empezar a huir
    [SerializeField] private float fleeSpeed = 7f; // Velocidad cuando huye (más rápida que normal)

    private Vector2 limits;
    private int direction;
    private Rigidbody2D body;
    private SpriteRenderer sprite;
    private Vector3 originalPosition;
    private Collider2D enemyCollider;
    
    // Variables para el comportamiento del enemigo
    private Transform player;
    private bool isFleeingFromPlayer = false;
    private bool playerDetected = false;

    public Animator animator;

    public float health = 1f;
    private string enemyId;

    private void Awake()
    {
        // Usar la posición mundial para consistencia
        Vector3 pos = transform.position;
        originalPosition = transform.localPosition; // Guardar posición local para respawn
        // limits.x = límite izquierdo, limits.y = límite derecho
        limits = new Vector2(pos.x - limitLeft, pos.x + limitRight);

        Debug.Log($"Enemigo {gameObject.name}: Pos Mundial: {pos}, Pos Local: {transform.localPosition}");
        Debug.Log($"Límites calculados - Izquierdo: {limits.x}, Derecho: {limits.y}");

        body = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        enemyCollider = GetComponent<Collider2D>();

        direction = 1; // Hacia la derecha

        // Generate a stable id for this enemy based on scene and position
        string sceneName = SceneManager.GetActiveScene().name;
        Vector3 worldPos = transform.position;
        // Round position to reduce floating point differences
        string posKey = string.Format("{0}_{1}_{2}", Mathf.Round(worldPos.x * 100f)/100f, Mathf.Round(worldPos.y * 100f)/100f, Mathf.Round(worldPos.z * 100f)/100f);
        string baseName = gameObject.name.Replace("(Clone)", "").Trim();
        enemyId = $"{sceneName}|{baseName}|{posKey}";

        // Register this enemy instance so EnemyManager can respawn it later
        EnemyManager.RegisterEnemy(enemyId, this);
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        
        // Buscar al jugador en la escena
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            Debug.Log($"Enemigo {gameObject.name}: Jugador encontrado");
        }
        else
        {
            Debug.LogWarning($"Enemigo {gameObject.name}: No se encontró jugador con tag 'Player'");
        }
    }

    public float Health
    {
        get { return health; }
        set
        {
            health = value;
            if (health <= 0)
            {
                Defeated();
            }
        }
    }

    public void Defeated()
    {
        // Activar animación de derrota
        if (animator != null)
        {
            if (HasAnimatorParameter("Defeated", AnimatorControllerParameterType.Trigger))
            {
                animator.SetTrigger("Defeated");
            }
            else if (HasAnimatorParameter("Death", AnimatorControllerParameterType.Trigger))
            {
                animator.SetTrigger("Death");
            }
        }

        // Desactivar collider para evitar más colisiones
        if (enemyCollider != null)
            enemyCollider.enabled = false;

        // Detener simulación física para evitar empujones o impulsos
        if (body != null)
            body.simulated = false;

        // Desactivar el enemigo después de la animación (fallback 1s)
        Invoke(nameof(DeactivateEnemy), 1f);
    }

    public void RemoveEnemy()
    {
        // This method is no longer needed, as we deactivate instead of destroy
    }

    public void DeactivateEnemy()
    {
        // Desactivar el GameObject en lugar de destruirlo para poder respawnearlo
        gameObject.SetActive(false);
    }

    public void Respawn()
    {
        // Reactivar y resetear el enemigo a su estado inicial
        gameObject.SetActive(true);
        transform.localPosition = originalPosition;
        if (body != null)
        {
            body.simulated = true;
            body.linearVelocity = Vector2.zero;
        }
        if (enemyCollider != null) enemyCollider.enabled = true;
        health = 1f; // or initial health if stored elsewhere
        direction = 1;
        
        // Resetear variables de comportamiento
        isFleeingFromPlayer = false;
        playerDetected = false;
        
        // Reset animator
        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }
    }
    

    private void Update()
    {
        // Solo procesar si el cuerpo está activo
        if (body == null || !body.simulated) return;

        Vector3 pos = transform.position; // Usar posición mundial para consistencia
        
        // Detectar al jugador si existe
        CheckPlayerProximity();
        
        // Determinar comportamiento basado en la proximidad del jugador
        if (isFleeingFromPlayer && player != null)
        {
            HandleFleeingBehavior(pos);
        }
        else
        {
            HandleNormalPatrolBehavior(pos);
        }

        // Actualizar sprite flip basado en dirección
        if (direction != 0)
        {
            sprite.flipX = direction < 0;
        }

        // Actualizar parámetros del animator
        UpdateAnimatorParameters();

        // Aplicar velocidad
        ApplyMovement();
    }

    private void CheckPlayerProximity()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // Detectar si el jugador está cerca
        playerDetected = distanceToPlayer <= detectionRange;
        
        // Determinar si debe huir
        isFleeingFromPlayer = distanceToPlayer <= fleeRange;
        
        // Debug visual (opcional)
        if (playerDetected && !isFleeingFromPlayer)
        {
            Debug.DrawLine(transform.position, player.position, Color.yellow);
        }
        else if (isFleeingFromPlayer)
        {
            Debug.DrawLine(transform.position, player.position, Color.red);
        }
    }

    private void HandleFleeingBehavior(Vector3 pos)
    {
        // Calcular dirección para alejarse del jugador
        float playerDirection = player.position.x - pos.x;
        
        if (playerDirection > 0) // Jugador está a la derecha
        {
            direction = -1; // Huir hacia la izquierda
        }
        else // Jugador está a la izquierda
        {
            direction = 1; // Huir hacia la derecha
        }
        
        // Verificar límites mientras huye
        if (pos.x <= limits.x && direction == -1)
        {
            direction = 0; // Detenerse en el límite izquierdo
        }
        else if (pos.x >= limits.y && direction == 1)
        {
            direction = 0; // Detenerse en el límite derecho
        }
    }

    private void HandleNormalPatrolBehavior(Vector3 pos)
    {
        // Si el jugador está cerca pero no en rango de huida, detenerse
        if (playerDetected)
        {
            direction = 0; // Detenerse cuando el jugador esté cerca
            return;
        }
        
        // Comportamiento de patrullaje normal
        if (pos.x <= limits.x)
        {
            direction = 1;
        }
        else if (pos.x >= limits.y)
        {
            direction = -1;
        }
    }

    private void UpdateAnimatorParameters()
    {
        if (animator != null)
        {
            // Parámetro para detectar si se está moviendo
            bool isMoving = direction != 0;
            
            if (HasAnimatorParameter("isMoving", AnimatorControllerParameterType.Bool))
            {
                animator.SetBool("isMoving", isMoving);
            }
            else if (HasAnimatorParameter("Moving", AnimatorControllerParameterType.Bool))
            {
                animator.SetBool("Moving", isMoving);
            }
        }
    }

    private void ApplyMovement()
    {
        if (body != null && body.simulated)
        {
            Vector2 velocity = body.linearVelocity;
            
            // Usar velocidad de huida si está huyendo, velocidad normal si no
            float currentSpeed = isFleeingFromPlayer ? fleeSpeed : speedX;
            velocity.x = direction * currentSpeed;
            
            body.linearVelocity = velocity;
        }
    }

    // Función helper para verificar si existe un parámetro en el animator
    private bool HasAnimatorParameter(string name, AnimatorControllerParameterType type)
    {
        if (animator == null) return false;
        foreach (var p in animator.parameters)
        {
            if (p.name == name && p.type == type) return true;
        }
        return false;
    }

    // Función para recibir daño
    public void TakeDamage(float damage)
    {
        Health -= damage;
        
        if (Health > 0)
        {
            // Activar animación de daño si existe
            if (animator != null)
            {
                if (HasAnimatorParameter("Damage", AnimatorControllerParameterType.Trigger))
                {
                    animator.SetTrigger("Damage");
                }
                else if (HasAnimatorParameter("Hit", AnimatorControllerParameterType.Trigger))
                {
                    animator.SetTrigger("Hit");
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        // Usar la posición actual del transform en world space
        Vector3 pos = transform.position;
        
        // Si hay una posición original guardada, usarla como centro de patrullaje
        if (originalPosition != Vector3.zero)
        {
            // Convertir originalPosition a world space si es necesario
            Vector3 worldOriginalPos = transform.parent != null ? 
                transform.parent.TransformPoint(originalPosition) : originalPosition;
            pos = worldOriginalPos;
        }
        
        // Dibujar límites de patrullaje
        Vector3 posLeft = new Vector3(pos.x - limitLeft, pos.y, pos.z);    // Límite izquierdo
        Vector3 posRight = new Vector3(pos.x + limitRight, pos.y, pos.z);  // Límite derecho
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(posLeft, 0.5f);
        Gizmos.DrawSphere(posRight, 0.5f);
        
        // Dibujar rango de detección del jugador (centrado en el enemigo)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Dibujar rango de huida (centrado en el enemigo)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, fleeRange);
        
        // Etiquetas para los rangos (solo en Scene view)
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(pos + Vector3.up * (detectionRange + 0.5f), "Detection Range");
        UnityEditor.Handles.Label(pos + Vector3.up * (fleeRange + 0.5f), "Flee Range");
        #endif
    }
}