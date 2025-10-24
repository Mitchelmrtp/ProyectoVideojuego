using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController2 : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 5f;
    public float collisionOffset = 0.05f;
    
    [Header("Parámetros para salto")]
    [SerializeField] private float jumpImpulse = 10f;
    
    [Header("Parámetros para detector de piso")]
    [SerializeField] private Transform detector;
    [SerializeField] private float sizeDetector = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Límite de cambios de gravedad")]
    [SerializeField] private int maxGravityChanges = 2; // 🔹 Número máximo de veces que se puede cambiar la gravedad
    private int currentGravityChanges = 0;              // 🔹 Contador actual de cambios

    public ContactFilter2D movementFilter;
    Vector2 movementInput;
    Rigidbody2D rb;

    List<RaycastHit2D> castCollisions = new List<RaycastHit2D>();

    Animator animator;
    SpriteRenderer spriteRenderer;

    bool canMove = true;
    private bool isAttacking = false; 
    private bool isGravedadInvertida = false; 
    private InputAction jumpAction;
    private InputAction testGravityAction; 

    public SwordAttack swordAttack;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        jumpAction = InputSystem.actions.FindAction("Jump");
        
        // Crear action para test de gravedad (click derecho)
        testGravityAction = new InputAction("TestGravity", InputActionType.Button, "<Mouse>/rightButton");
        testGravityAction.Enable();
        
        Debug.Log($"PlayerController2 iniciado. GravityScale inicial: {rb.gravityScale}");
        
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
        }

        // 🔹 Control de límite para cambio de gravedad manual
        if (testGravityAction.WasPressedThisFrame())
        {
            if (currentGravityChanges < maxGravityChanges)
            {
                Debug.Log($"Click derecho presionado - Cambio de gravedad #{currentGravityChanges + 1}");
                CambiarGravedad();
                currentGravityChanges++;
            }
            else
            {
                Debug.Log($"❌ Límite de cambios de gravedad alcanzado ({maxGravityChanges}). No se puede cambiar más.");
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
        animator.SetTrigger("isAttacking");
        Debug.Log("Attacked");
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
        LockMovement();
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;

        if (spriteRenderer.flipX == true)
        {
            swordAttack.AttactLeft();
        }
        else
        {
            swordAttack.AttackRight();
        }
    }

    public void EndSwordAttack()
    {
        UnlockMovement();
        isAttacking = false;
        swordAttack.StopAttack();
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
                Debug.Log("Tocaste un enemigo - Muriendo");
                PlayerDeath();
            }
        }
        // 🔹 NUEVO: Detectar diamante y aumentar límite de gravedad
        else if (collision.CompareTag("Diamante"))
        {
            Debug.Log("💎 ¡Diamante recogido! Aumentando capacidad de cambio de gravedad +1");
            maxGravityChanges += 1;
            Destroy(collision.gameObject); // Desaparecer el diamante
            Debug.Log($"Nuevo límite de gravedad: {maxGravityChanges}");
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
                Debug.Log("Colisionaste con un enemigo - Muriendo");
                PlayerDeath();
            }
        }
    }

    public void PlayerDeath()
    {
        Debug.Log("¡Jugador ha muerto! Respawneando...");
        
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
        currentGravityChanges = 0;
        Debug.Log("Contador de cambios de gravedad reiniciado tras morir.");

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
