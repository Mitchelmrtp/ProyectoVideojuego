using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerController2 : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 5f;
    public float collisionOffset = 0.05f;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI txtGravedad; // Texto que mostrará los cambios


    [Header("Parámetros para salto")]
    [SerializeField] private float jumpImpulse = 10f;
    
    [Header("Parámetros para detector de piso")]
    [SerializeField] private Transform detector;
    [SerializeField] private float sizeDetector = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Límite de cambios de gravedad")]
    [SerializeField] private int maxGravityChanges = 2; // 🔹 Número máximo de veces que se puede cambiar la gravedad
              
    private int baseMaxGravityChanges;              // 🔹 Contador actual de cambios
    private int gravityChangesAvailable;


    public ContactFilter2D movementFilter;
    Vector2 movementInput;
    Rigidbody2D rb;

    List<RaycastHit2D> castCollisions = new List<RaycastHit2D>();

    Animator animator;
    SpriteRenderer spriteRenderer;
    [Header("Debug")]
    public bool debugDirectAttack = false; // Si se activa, OnFire() llamará directamente a SwordAttack() para probar

    bool canMove = true;
    private bool isAttacking = false; 
    private bool isGravedadInvertida = false; 
    private InputAction jumpAction;
    private InputAction testGravityAction; 

    public SwordAttack swordAttack;
    [Header("Attack Settings")]
    [Tooltip("Duración por defecto del ataque (segundos). Usado como fallback si el Animation Event EndSwordAttack no se ejecuta).")]
    public float attackDuration = 0.4f;
    private Coroutine attackCoroutine = null;
    
    [Header("Health")]
    public int maxHealth = 3;
    private int currentHealth;
    private Coroutine deathCoroutine = null;
    [Header("Timing")]
    [Tooltip("Duración por defecto de la animación de muerte (segundos). Si la animación tiene un Animation Event EndDeath, se cancelará la espera).")]
    public float deathDuration = 1.0f;
    [Tooltip("Duración por defecto del hit (segundos). Usado como fallback si la animación de daño no llama a EndHurt().")]
    public float hitDuration = 0.5f;
    private Coroutine hitCoroutine = null;
    [Header("Respawn")]
    [Tooltip("Si está activado, al morir el jugador se limpiará la lista de enemigos derrotados para que reaparezcan.")]
    public bool respawnEnemiesOnPlayerDeath = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        jumpAction = InputSystem.actions.FindAction("Jump");
        
        // Crear action para test de gravedad (click derecho)
        testGravityAction = new InputAction("TestGravity", InputActionType.Button, "<Mouse>/rightButton");
        testGravityAction.Enable();

        baseMaxGravityChanges = maxGravityChanges;
        gravityChangesAvailable = maxGravityChanges;
        ActualizarTextoGravedad();

        Debug.Log($"PlayerController2 iniciado. Cambios disponibles: {gravityChangesAvailable}/{maxGravityChanges}");

        Collider2D[] colliders = GetComponents<Collider2D>();
        bool hasTrigger = false;
        foreach (var col in colliders)
        {
            if (col.isTrigger)
            {
                hasTrigger = true;
                break;
            }
        }
        
        if (!hasTrigger)
        {
            Debug.LogWarning("¡ADVERTENCIA! El personaje no tiene ningún Collider2D configurado como Trigger. Los portales no funcionarán.");
        }
        else
        {
            Debug.Log("Collider2D Trigger encontrado. Los portales deberían funcionar.");
        }

        // Inicializar vida
        currentHealth = maxHealth;
    }

    private void FixedUpdate()
    {
        if (canMove)
        {
            float horizontalInput = movementInput.x;
            
            if (horizontalInput != 0)
            {
                rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
                animator.SetBool("isMoving", true);
            }
            else
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                animator.SetBool("isMoving", false);
            }

            if (horizontalInput < 0)
            {
                spriteRenderer.flipX = isGravedadInvertida ? false : true;
            }
            else if (horizontalInput > 0)
            {
                spriteRenderer.flipX = isGravedadInvertida ? true : false;
            }
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            animator.SetBool("isMoving", false);
        }

        rb.gravityScale = isGravedadInvertida ? -1 : 1;
    }

    private void Update()
    {
        if (detector != null)
        {
            Collider2D colision = Physics2D.OverlapCircle(detector.position, sizeDetector, groundLayer);
            bool canJump = colision != null;

            if (jumpAction != null && jumpAction.WasPressedThisFrame() && canJump && canMove)
            {
                Debug.Log("El personaje debe saltar...");
                float jumpDirection = isGravedadInvertida ? -jumpImpulse : jumpImpulse;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpDirection);
            }

            // Actualizar parámetros del animator relacionados con salto/grounded
            if (animator != null)
            {
                if (HasAnimatorParameter("isJumping", AnimatorControllerParameterType.Bool))
                    animator.SetBool("isJumping", !canJump && rb.linearVelocity.y > 0f);
                if (HasAnimatorParameter("isFalling", AnimatorControllerParameterType.Bool))
                    animator.SetBool("isFalling", !canJump && rb.linearVelocity.y < 0f);
                if (HasAnimatorParameter("isGrounded", AnimatorControllerParameterType.Bool))
                    animator.SetBool("isGrounded", canJump);
                if (HasAnimatorParameter("verticalVel", AnimatorControllerParameterType.Float))
                    animator.SetFloat("verticalVel", rb.linearVelocity.y);
            }
        }

        // 🔹 Control de límite para cambio de gravedad manual
        if (testGravityAction.WasPressedThisFrame())
        {
            if (gravityChangesAvailable > 0)
            {
                CambiarGravedad();
                gravityChangesAvailable--;
                Debug.Log($"Cambio de gravedad realizado. Restantes: {gravityChangesAvailable}/{maxGravityChanges}");
                ActualizarTextoGravedad();
            }
            else
            {
                Debug.Log("❌ No tienes más cambios de gravedad disponibles.");
            }
        }
    }

    private bool TryMove(Vector2 direction)
    {
        if (direction != Vector2.zero)
        {
            int count = rb.Cast(
               movementInput,
               movementFilter,
               castCollisions,
               moveSpeed * Time.fixedDeltaTime + collisionOffset);

            if (count == 0)
            {
                rb.MovePosition(rb.position + movementInput * moveSpeed * Time.fixedDeltaTime);
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    void OnMove(InputValue movementValue)
    {
        Vector2 fullInput = movementValue.Get<Vector2>();
        movementInput = new Vector2(fullInput.x, 0);
    }

    void OnFire()
    {
        // Evitar iniciar otro ataque mientras ya se está atacando
        if (isAttacking)
        {
            Debug.Log("OnFire: ya se está atacando, ignorando input.");
            return;
        }

        if (animator == null)
        {
            Debug.LogError("OnFire: animator es null");
        }
        else
        {
            // Log corto de animator para diagnóstico
            var pars = animator.parameters;
            string pNames = "";
            foreach (var p in pars)
            {
                pNames += p.name + "(" + p.type + ") ";
            }
            Debug.Log($"Animator: {animator.runtimeAnimatorController?.name ?? "(null controller)"}. Parámetros: {pNames}");
                if (HasAnimatorParameter("isAttacking", AnimatorControllerParameterType.Trigger))
                    animator.SetTrigger("isAttacking");
            Debug.Log("Attacked");
        }

        // Llamamos directamente al método de ataque al recibir el input (click izquierdo)
        SwordAttack();
    }

    
    
    public void LockMovement()
    {
        canMove = false;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    public void UnlockMovement()
    {
        canMove = true;
    }

    public void SwordAttack()
    {
        // Protección: si ya estamos atacando no volvemos a ejecutar
        if (isAttacking)
        {
            Debug.Log("SwordAttack(): ya está atacando, salida temprana.");
            return;
        }

        LockMovement();
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;

        if (swordAttack == null)
        {
            Debug.LogError("SwordAttack(): referencia 'swordAttack' no asignada en el inspector.");
            return;
        }

        if (spriteRenderer.flipX == true)
        {
            swordAttack.AttactLeft();
        }
        else
        {
            swordAttack.AttackRight();
        }

        // Iniciar corrutina de seguridad para asegurar que el ataque termina aunque el Animation Event no se dispare
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
        }
        attackCoroutine = StartCoroutine(AttackTimeoutCoroutine(attackDuration));
    }

    public void EndSwordAttack()
    {
        UnlockMovement();
        isAttacking = false;
        swordAttack.StopAttack();
        // Detener corrutina si sigue activa
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }
    }

    private void ActualizarTextoGravedad()
    {
        if (txtGravedad != null)
        {
            txtGravedad.text = $"Total de saltos de gravedad: {gravityChangesAvailable}";
        }
    }


    private IEnumerator AttackTimeoutCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        // Si al terminar el timeout todavía estamos atacando, forzamos el fin del ataque
        if (isAttacking)
        {
            Debug.Log("AttackTimeoutCoroutine: tiempo de ataque expirado; llamando a EndSwordAttack() como fallback.");
            EndSwordAttack();
        }
        attackCoroutine = null;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"Trigger detectado con: {collision.gameObject.name}, Tag: {collision.gameObject.tag}");
        
        if (collision.gameObject.CompareTag("DeadZone"))
        {
            Debug.Log("Entrando en DeadZone - Respawning");
            PlayerDeath();
        }
        else if (collision.CompareTag("ZonaGravedad"))
        {
            Debug.Log("Entrando en ZonaGravedad - Cambiando gravedad");
            CambiarGravedad();
        }
        else if (collision.CompareTag("Enemy"))
        {
            if (isAttacking)
            {
                Debug.Log("Tocaste un enemigo mientras atacas - El enemigo debería morir, no tú");
            }
            else
            {
                Debug.Log("Tocaste un enemigo - Recibiendo daño");
                TakeDamage(1);
            }
        }
        // 🔹 NUEVO: Detectar diamante y aumentar límite de gravedad
        else if (collision.CompareTag("Diamante"))
        {
            Debug.Log("💎 ¡Diamante recogido! Aumentando capacidad de cambio de gravedad +1");
            maxGravityChanges += 1;
            gravityChangesAvailable += 1;
            Destroy(collision.gameObject);
            Debug.Log($"Nuevo límite total: {maxGravityChanges}, disponibles: {gravityChangesAvailable}");
            ActualizarTextoGravedad();
        }
        else
        {
            Debug.Log($"Tag no reconocido: {collision.gameObject.tag}");
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"Colisión detectada con: {collision.gameObject.name}, Tag: {collision.gameObject.tag}");
        
        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (isAttacking)
            {
                Debug.Log("Colisionaste con un enemigo mientras atacas - El enemigo debería morir, no tú");
            }
            else
            {
                Debug.Log("Colisionaste con un enemigo - Recibiendo daño");
                TakeDamage(1);
            }
        }
    }

    public void TakeDamage(int amount)
    {
        if (currentHealth <= 0)
            return;

        currentHealth -= amount;
        Debug.Log($"Player: TakeDamage({amount}). Vida restante: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Reproducir animación de daño/hit si existe el parámetro
            if (animator != null)
            {
                if (HasAnimatorParameter("Hit", AnimatorControllerParameterType.Trigger))
                    animator.SetTrigger("Hit");
                if (HasAnimatorParameter("isHurt", AnimatorControllerParameterType.Trigger))
                    animator.SetTrigger("isHurt");
            }
            LockMovement();

            // Iniciar fallback: si la animación de hit no llama a EndHurt(), desbloqueamos después de hitDuration
            if (hitCoroutine != null)
                StopCoroutine(hitCoroutine);
            hitCoroutine = StartCoroutine(HitTimeoutCoroutine(hitDuration));
        }
    }

    public void EndHurt()
    {
        // Llamado desde animación al terminar el hit
        // Detener corrutina de fallback si existe
        if (hitCoroutine != null)
        {
            StopCoroutine(hitCoroutine);
            hitCoroutine = null;
        }
        UnlockMovement();
    }

    private IEnumerator HitTimeoutCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        hitCoroutine = null;
        Debug.Log("HitTimeoutCoroutine: tiempo de hit expirado; llamando a EndHurt() como fallback.");
        EndHurt();
    }

    public void Die()
    {
        Debug.Log("Player: Die() invocado");
        if (animator != null)
        {
            if (HasAnimatorParameter("Death", AnimatorControllerParameterType.Trigger))
                animator.SetTrigger("Death");
            if (HasAnimatorParameter("isDead", AnimatorControllerParameterType.Trigger))
                animator.SetTrigger("isDead");
        }
        LockMovement();
        // Iniciar fallback: si la animación no tiene un Animation Event EndDeath, respawnearemos tras deathDuration
        if (deathCoroutine != null)
        {
            StopCoroutine(deathCoroutine);
        }
        deathCoroutine = StartCoroutine(DeathCoroutine(deathDuration));
    }

    public void EndDeath()
    {
        // Llamado desde animación de muerte cuando quieras respawnear
        if (deathCoroutine != null)
        {
            StopCoroutine(deathCoroutine);
            deathCoroutine = null;
        }
        PlayerDeath();
    }

    // Helper: comprobar si el Animator tiene un parámetro con nombre y tipo dados
    private bool HasAnimatorParameter(string name, UnityEngine.AnimatorControllerParameterType type)
    {
        if (animator == null) return false;
        foreach (var p in animator.parameters)
        {
            if (p.name == name && p.type == type) return true;
        }
        return false;
    }

    private IEnumerator DeathCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        deathCoroutine = null;
        Debug.Log("DeathCoroutine: tiempo expirado; respawneando al jugador.");
        PlayerDeath();
    }

    public void PlayerDeath()
    {
        Debug.Log("¡Jugador ha muerto! Respawneando...");
        // Opcional: permitir que los enemigos reaparezcan al respawnear el jugador
        if (respawnEnemiesOnPlayerDeath)
        {
            // Llamar a RespawnAll para reactivar y reiniciar los enemigos registrados
            EnemyManager.RespawnAll();
            Debug.Log("EnemyManager: RespawnAll() called because player respawned.");
        }
        else
        {
            // Si no queremos que reaparezcan, limpiamos el registro
            EnemyManager.Clear();
            Debug.Log("EnemyManager: registry cleared (no respawn on player death).");
        }
        
        rb.linearVelocity = Vector2.zero;
        
        GameObject spawn = GameObject.FindGameObjectWithTag("SpawnPoint");
        if (spawn != null)
        {
            transform.localPosition = spawn.transform.localPosition;
            transform.rotation = Quaternion.Euler(0, 0, 0);
            isGravedadInvertida = false;
            rb.gravityScale = 1;
            spriteRenderer.flipX = false;

            Debug.Log($"Jugador respawneado en: {spawn.transform.localPosition}");
        }
        else
        {
            Debug.LogError("No se encontró ningún SpawnPoint con tag 'SpawnPoint'");
            transform.localPosition = Vector3.zero;
        }

        // 🔹 Reiniciar contador de cambios de gravedad
        maxGravityChanges = baseMaxGravityChanges;
        gravityChangesAvailable = maxGravityChanges;

        Debug.Log($"Contadores de gravedad reiniciados: {gravityChangesAvailable}/{maxGravityChanges}");

        // Restaurar estado del Animator y variables para que el jugador reaparezca correctamente
        if (animator != null)
        {
            // Cancelar triggers y volver al estado por defecto
            animator.ResetTrigger("Death");
            animator.ResetTrigger("isDead");
            animator.ResetTrigger("Hit");
            animator.ResetTrigger("isHurt");
            // Forzar rebind para limpiar estados y variables animadas
            animator.Rebind();
            // Intentar reproducir el estado idle si existe
            try
            {
                animator.Play("Player_idle", 0, 0f);
                animator.Update(0f);
            }
            catch {
                // Si no existe el estado, Rebind ya debería dejarlo en un estado consistente
            }
        }

        // Detener corrutinas relacionadas si siguen activas
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }
        if (deathCoroutine != null)
        {
            StopCoroutine(deathCoroutine);
            deathCoroutine = null;
        }
        if (hitCoroutine != null)
        {
            StopCoroutine(hitCoroutine);
            hitCoroutine = null;
        }

        // Reiniciar vida
        currentHealth = maxHealth;

        // Asegurarse de que el ataque esté desactivado
        if (swordAttack != null)
            swordAttack.StopAttack();

        // Reset flags
        isAttacking = false;

        UnlockMovement();
    }

    public void CambiarGravedad()
    {
        isGravedadInvertida = !isGravedadInvertida;
        
        Debug.Log($"=== CAMBIO DE GRAVEDAD ===");
        Debug.Log($"Gravedad invertida: {isGravedadInvertida}");
        Debug.Log($"Rotation antes: {transform.rotation.eulerAngles}");
        Debug.Log($"GravityScale antes: {rb.gravityScale}");
        
        if (isGravedadInvertida)
        {
            transform.rotation = Quaternion.Euler(0, 0, 180);
            rb.gravityScale = -1;
            Debug.Log("Aplicando gravedad invertida");
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
            rb.gravityScale = 1;
            Debug.Log("Aplicando gravedad normal");
        }
        
        Debug.Log($"Rotation después: {transform.rotation.eulerAngles}");
        Debug.Log($"GravityScale después: {rb.gravityScale}");
        Debug.Log("========================");
        
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
    }
    
    private void OnDestroy()
    {
        if (testGravityAction != null)
        {
            testGravityAction.Disable();
            testGravityAction.Dispose();
        }
    }

    [System.Obsolete("Solo para testing")]
    public void TestCambiarGravedad()
    {
        Debug.Log("TEST: Forzando cambio de gravedad");
        CambiarGravedad();
    }

    private void OnDrawGizmosSelected()
    {
        if (detector != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(detector.position, sizeDetector);
        }
    }
}
