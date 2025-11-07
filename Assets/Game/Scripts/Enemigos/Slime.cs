using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Manager estático para manejar respawn de Slimes
public static class SlimeManager
{
    private static Dictionary<string, Slime> registeredSlimes = new Dictionary<string, Slime>();

    public static void RegisterSlime(string id, Slime slime)
    {
        if (string.IsNullOrEmpty(id) || slime == null) return;
        if (!registeredSlimes.ContainsKey(id))
        {
            registeredSlimes[id] = slime;
            Debug.Log($"SlimeManager: Slime registrado -> {id}");
        }
    }

    public static void UnregisterSlime(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        if (registeredSlimes.ContainsKey(id))
            registeredSlimes.Remove(id);
    }

    public static void RespawnAll()
    {
        Debug.Log("SlimeManager: Respawning all registered slimes...");
        foreach (var kv in registeredSlimes)
        {
            Slime slime = kv.Value;
            if (slime != null)
            {
                slime.Respawn();
            }
        }
    }

    public static void Clear()
    {
        registeredSlimes.Clear();
    }
}

public class Slime : MonoBehaviour
{
    [SerializeField] private float speedX;
    [Header("Patrol Limits (distances from start position)")]
    [SerializeField] private float limitLeft;   // Distancia hacia la izquierda
    [SerializeField] private float limitRight;  // Distancia hacia la derecha
    
    [Header("Player Detection")]
    [SerializeField] private float detectionRange = 3f; // Distancia para detectar al jugador
    [SerializeField] private float chaseSpeed = 4f; // Velocidad cuando persigue al jugador
    [SerializeField] private float attackDamage = 1f; // Daño que hace al jugador
    [SerializeField] private float attackCooldown = 2f; // Tiempo entre ataques
    
    [Header("Health")]
    [SerializeField] private float maxHealth = 1f; // Vida máxima del slime

    private Vector2 limits;
    private int direction;
    private Rigidbody2D body;
    private SpriteRenderer sprite;
    private Vector3 originalPosition;
    private Collider2D enemyCollider;
    
    // Variables para el comportamiento del enemigo
    private Transform player;
    private bool isChasingPlayer = false;
    private bool playerDetected = false;
    // ELIMINADO: isAttacking - variable no utilizada que causaba warning CS0414
    private float lastAttackTime = 0f;

    public float health; // Vida actual del slime (se inicializa con maxHealth)
    private string enemyId;
    
    // Referencia al Animator (opcional)
    private Animator animator;

    private void Awake()
    {
        // Usar la posición mundial para consistencia
        Vector3 pos = transform.position;
        originalPosition = transform.localPosition; // Guardar posición local para respawn
        // limits.x = límite izquierdo, limits.y = límite derecho
        limits = new Vector2(pos.x - limitLeft, pos.x + limitRight);

        Debug.Log($"Slime {gameObject.name}: Pos Mundial: {pos}, Pos Local: {transform.localPosition}");
        Debug.Log($"Límites calculados - Izquierdo: {limits.x}, Derecho: {limits.y}");

        body = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        
        // Obtener todos los colliders
        Collider2D[] colliders = GetComponents<Collider2D>();
        enemyCollider = null;
        
        // Buscar el collider principal (no trigger)
        foreach (var col in colliders)
        {
            if (!col.isTrigger)
            {
                enemyCollider = col;
                break;
            }
        }
        
        // Si no hay collider no-trigger, usar el primero disponible
        if (enemyCollider == null && colliders.Length > 0)
        {
            enemyCollider = colliders[0];
        }
        
        // Verificación de componentes para depuración
        if (enemyCollider == null)
        {
            Debug.LogWarning($"⚠️ {gameObject.name} no tiene Collider2D!");
        }
        else
        {
            Debug.Log($"✅ {gameObject.name} tiene collider principal: {enemyCollider.GetType().Name}, isTrigger: {enemyCollider.isTrigger}");
            
            // Asegurar que el collider principal NO sea trigger para detección de Physics2D.OverlapCircleAll
            if (enemyCollider.isTrigger)
            {
                Debug.LogWarning($"⚠️ {gameObject.name}: El collider principal es trigger. Esto puede impedir la detección de daño.");
            }
        }
        
        Debug.Log($"✅ {gameObject.name} tiene {colliders.Length} colliders en total");
        
        // Verificar que tenga el tag correcto
        if (!gameObject.CompareTag("Enemy"))
        {
            Debug.LogWarning($"⚠️ {gameObject.name} no tiene el tag 'Enemy'. Tag actual: {gameObject.tag}");
        }
        else
        {
            Debug.Log($"✅ {gameObject.name} tiene el tag correcto: Enemy");
        }

        direction = 1; // Hacia la derecha

        // Generate a stable id for this enemy based on scene and position
        string sceneName = SceneManager.GetActiveScene().name;
        Vector3 worldPos = transform.position;
        // Round position to reduce floating point differences
        string posKey = string.Format("{0}_{1}_{2}", Mathf.Round(worldPos.x * 100f)/100f, Mathf.Round(worldPos.y * 100f)/100f, Mathf.Round(worldPos.z * 100f)/100f);
        string baseName = gameObject.name.Replace("(Clone)", "").Trim();
        enemyId = $"{sceneName}|{baseName}|{posKey}";

        // Register this slime in a static dictionary for respawning
        SlimeManager.RegisterSlime(enemyId, this);
    }

    private void Start()
    {
        // Obtener el Animator si existe
        animator = GetComponent<Animator>();
        
        // Inicializar la salud con el valor máximo del inspector
        health = maxHealth;
        Debug.Log($"Slime {gameObject.name}: Salud inicializada a {health}");
        
        // Buscar al jugador en la escena
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            Debug.Log($"Slime {gameObject.name}: Jugador encontrado");
        }
        else
        {
            Debug.LogWarning($"Slime {gameObject.name}: No se encontró jugador con tag 'Player'");
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
        Debug.Log($"Slime {gameObject.name}: Defeated!");
        
        // Activar animación de derrota
        if (animator != null)
        {
            animator.SetTrigger("Defeated");
        }

        // Desactivar collider para evitar más colisiones
        if (enemyCollider != null)
            enemyCollider.enabled = false;

        // Detener simulación física para evitar empujones o impulsos
        if (body != null)
            body.simulated = false;

        // Desactivar el enemigo después de un breve delay
        Invoke(nameof(DeactivateEnemy), 1f);
    }

    public void DeactivateEnemy()
    {
        // Desactivar el GameObject en lugar de destruirlo para poder respawnearlo
        gameObject.SetActive(false);
    }
    
    // Método para ser llamado por Animation Events
    public void RemoveEnemy()
    {
        Debug.Log($"Slime {gameObject.name}: RemoveEnemy llamado por Animation Event");
        DeactivateEnemy();
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
        health = maxHealth; // Restaurar la salud máxima del inspector
        direction = 1;
        
        // Resetear variables de comportamiento
        isChasingPlayer = false;
        playerDetected = false;
        // ELIMINADO: isAttacking - variable no utilizada
        lastAttackTime = 0f;
        
        // Reset animator
        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }
    }
    
    private void OnDestroy()
    {
        // Unregister when the object is destroyed
        SlimeManager.UnregisterSlime(enemyId);
    }
    
    private void Update()
    {
        // Solo procesar si el cuerpo está activo
        if (body == null || !body.simulated) return;

        Vector3 pos = transform.position; // Usar posición mundial para consistencia
        
        // Detectar al jugador si existe
        CheckPlayerProximity();
        
        // Determinar comportamiento basado en la proximidad del jugador
        if (isChasingPlayer && player != null)
        {
            HandleChasingBehavior(pos);
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

    private void UpdateAnimatorParameters()
    {
        if (animator != null)
        {
            // Parámetro para detectar si se está moviendo (basado en el Animator Controller)
            bool isMoving = direction != 0;
            animator.SetBool("Moving", isMoving);
            
            // Otros parámetros que podrían estar en el Animator
            if (HasAnimatorParameter("isMoving"))
            {
                animator.SetBool("isMoving", isMoving);
            }
        }
    }

    // Función helper para verificar si existe un parámetro en el animator
    private bool HasAnimatorParameter(string paramName)
    {
        if (animator == null) return false;
        
        foreach (var param in animator.parameters)
        {
            if (param.name == paramName)
                return true;
        }
        return false;
    }

    private void CheckPlayerProximity()
    {
        if (player == null) return;
        
        // Verificar si el jugador está muriendo/muerto - detener toda persecución
        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null && (playerController.isDying || playerController.currentHealth <= 0))
        {
            // El jugador está muriendo/muerto, detener persecución
            playerDetected = false;
            isChasingPlayer = false;
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // Detectar si el jugador está cerca para perseguir
        playerDetected = distanceToPlayer <= detectionRange;
        
        // Determinar si debe perseguir (pero no atacar a distancia)
        isChasingPlayer = playerDetected && distanceToPlayer > 0.5f; // Usar distancia muy pequeña para contacto
        
        // DEBUG: No atacar automáticamente a distancia - solo por contacto físico
        // El ataque se maneja en OnTriggerEnter2D
        
        // Debug visual
        if (playerDetected)
        {
            Debug.DrawLine(transform.position, player.position, Color.orange); // Persiguiendo
        }
    }

    private void HandleChasingBehavior(Vector3 pos)
    {
        // Calcular dirección para perseguir al jugador
        float playerDirection = player.position.x - pos.x;
        
        if (playerDirection > 0) // Jugador está a la derecha
        {
            direction = 1; // Ir hacia la derecha
        }
        else // Jugador está a la izquierda
        {
            direction = -1; // Ir hacia la izquierda
        }
        
        // Verificar límites mientras persigue
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
        // Comportamiento de patrullaje normal - moverse entre los límites
        if (pos.x <= limits.x)
        {
            direction = 1; // Ir hacia la derecha
        }
        else if (pos.x >= limits.y)
        {
            direction = -1; // Ir hacia la izquierda
        }
    }

    private void ApplyMovement()
    {
        if (body != null && body.simulated)
        {
            Vector2 velocity = body.linearVelocity;
            
            // Usar velocidad de persecución si está persiguiendo, velocidad normal si no
            float currentSpeed = isChasingPlayer ? chaseSpeed : speedX;
            velocity.x = direction * currentSpeed;
            
            body.linearVelocity = velocity;
        }
    }

    // Función para recibir daño
    public void TakeDamage(float damage)
    {
        Debug.Log($"🩸 Slime {gameObject.name} MÉTODO TakeDamage LLAMADO con {damage} de daño. Vida actual: {health}");
        
        health -= damage;
        Debug.Log($"🩸 Slime {gameObject.name} recibió {damage} de daño. Vida restante: {health}");
        
        if (health <= 0)
        {
            Debug.Log($"💀 Slime {gameObject.name} eliminado por daño");
            Defeated();
        }
        else
        {
            // Activar animación de daño
            if (animator != null)
            {
                animator.SetTrigger("Damage");
                Debug.Log("🎭 Animación de daño activada");
            }
        }
    }

    // Método para ser llamado cuando el jugador ataca al slime
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Slime: Trigger detectado con {other.name}, tag: {other.tag}");
        
        // Verificar si es el área de ataque del jugador por nombre del collider
        if (other.name.Contains("controladorGolpe") || 
            other.name.Contains("Golpe") || 
            other.name.Contains("AttackArea") ||
            other.name.Contains("SwordHitbox") ||
            other.name.Contains("Attack"))
        {
            TakeDamage(1f);
            Debug.Log("Slime recibió daño del área de ataque del jugador");
            return;
        }
        
        // Verificar si el jugador nos toca directamente 
        if (other.CompareTag("Player"))
        {
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                // NUEVA VERIFICACIÓN: No atacar si el jugador está muerto o muriendo
                if (playerController.isDying || playerController.currentHealth <= 0)
                {
                    Debug.Log("Slime: Jugador está muriendo/muerto, no atacar");
                    return;
                }
                
                Debug.Log($"Slime en contacto con jugador - isAttacking: {playerController.isAttacking}");
                // Solo hacer daño al jugador si no está atacando y respetando cooldown
                if (!playerController.isAttacking && Time.time >= lastAttackTime + attackCooldown)
                {
                    lastAttackTime = Time.time;
                    playerController.TakeDamage((int)attackDamage);
                    Debug.Log($"Slime causó {attackDamage} de daño al jugador por contacto físico");
                }
                else if (playerController.isAttacking)
                {
                    Debug.Log("Jugador está atacando - Slime debería recibir daño");
                    TakeDamage(1f);
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
        
        // Etiquetas para los rangos (solo en Scene view)
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(pos + Vector3.up * (detectionRange + 0.5f), "Detection Range");
        #endif
    }
}
