using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController3 : MonoBehaviour
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

    public ContactFilter2D movementFilter;
    Vector2 movementInput;
    Rigidbody2D rb;

    List<RaycastHit2D> castCollisions = new List<RaycastHit2D>();

    Animator animator;
    SpriteRenderer spriteRenderer;

    bool canMove = true;
    private bool isAttacking = false; // Para saber si el jugador está atacando
    private bool isGravedadInvertida = false; // Para saber si la gravedad está invertida
    private InputAction jumpAction;
    private InputAction testGravityAction; // Para testing con tecla G
    private Rigidbody2D body;

    public SwordAttack swordAttack;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        jumpAction = InputSystem.actions.FindAction("Jump");
        
        // Crear action para test de gravedad (tecla G)
        testGravityAction = new InputAction("TestGravity", InputActionType.Button, "<Mouse>/rightButton");
        testGravityAction.Enable();
        
        // Verificar configuración inicial
        Debug.Log($"PlayerController2 iniciado. GravityScale inicial: {rb.gravityScale}");
        
        // Verificar que el personaje tenga un Collider2D configurado como Trigger
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
            // Solo movimiento horizontal para juego de plataformas
            float horizontalInput = movementInput.x;
            
            if (horizontalInput != 0)
            {
                // Movimiento horizontal usando velocity
                rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
                animator.SetBool("isMoving", true);
            }
            else
            {
                // Detener movimiento horizontal
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                animator.SetBool("isMoving", false);
            }

            // Corregir la dirección del sprite según el estado de la gravedad
            if (horizontalInput < 0)
            {
                // Cuando la gravedad está invertida, invertimos la lógica del flipX
                spriteRenderer.flipX = isGravedadInvertida ? false : true;
            }
            else if (horizontalInput > 0)
            {
                // Cuando la gravedad está invertida, invertimos la lógica del flipX
                spriteRenderer.flipX = isGravedadInvertida ? true : false;
            }
        }
        else
        {
            // Si no puede moverse (durante ataque), asegurar que esté detenido horizontalmente
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            animator.SetBool("isMoving", false);
        }

        // Manejo de la gravedad
        if (isGravedadInvertida)
        {
            rb.gravityScale = -1;  // Cambiar la gravedad al modo invertido
        }
        else
        {
            rb.gravityScale = 1;  // Normalizar la gravedad
        }
    }

    private void Update()
    {
        // Detectamos si estamos en el piso para poder saltar
        if (detector != null)
        {
            Collider2D colision = Physics2D.OverlapCircle(detector.position, sizeDetector, groundLayer);
            bool canJump = colision != null;  // Si podemos saltar (estamos en el piso o techo)

            // Salto
            if (jumpAction != null && jumpAction.WasPressedThisFrame() && canJump && canMove)
            {
                Debug.Log("El personaje debe saltar...");
                // Si la gravedad está invertida, saltamos en dirección opuesta
                float jumpDirection = isGravedadInvertida ? -jumpImpulse : jumpImpulse;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpDirection);
            }
        }
        
        // TEST: Presiona G para cambiar gravedad manualmente (solo para debug)
        if (testGravityAction.WasPressedThisFrame())
        {
            Debug.Log("Tecla G presionada - Cambiando gravedad manualmente");
            CambiarGravedad();
        }
    }

    private bool TryMove(Vector2 direction)
    {
        if (direction != Vector2.zero)
        {
            int count = rb.Cast(
               movementInput, // X and Y values between -1 and 1 that represent the direction from the body to look for collisions
               movementFilter, // The settings that determine where a collision can occur on such as layers to collide with
               castCollisions, // List of collisions to store the found collisions into after the Cast is finished
               moveSpeed * Time.fixedDeltaTime + collisionOffset); // The amount to cast equal to the movement plus an offset

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
            //can't move if there's no direction to move in
            return false;
        }
    }

    void OnMove(InputValue movementValue)
    {
        // Solo tomamos el input horizontal para movimiento de plataformas
        Vector2 fullInput = movementValue.Get<Vector2>();
        movementInput = new Vector2(fullInput.x, 0); // Solo componente X
    }

    void OnFire()
    {
        animator.SetTrigger("isAttacking");
        Debug.Log("Attacked");
    }

    public void LockMovement()
    {
        canMove = false;
        // Detener el movimiento horizontal durante el ataque
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    public void UnlockMovement()
    {
        canMove = true;
    }

    public void SwordAttack()
    {
        LockMovement();
        isAttacking = true; // Marcar que está atacando
        
        // Asegurar que el personaje esté completamente detenido durante el ataque
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
        isAttacking = false; // Desmarcar que está atacando
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
            // Cambiar la gravedad e invertir la lógica de flip
            CambiarGravedad();
        }
        else if (collision.CompareTag("Enemy"))
        {
            if (isAttacking)
            {
                Debug.Log("Tocaste un enemigo mientras atacas - El enemigo debería morir, no tú");
                // Aquí podrías agregar lógica para matar al enemigo
                // Por ejemplo: collision.GetComponent<Enemy>().Die();
            }
            else
            {
                Debug.Log("Tocaste un enemigo - Muriendo");
                PlayerDeath();
            }
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
                // Aquí podrías agregar lógica para matar al enemigo
                // Por ejemplo: collision.GetComponent<Enemy>().Die();
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
        
        // Detener el movimiento
        rb.linearVelocity = Vector2.zero;
        
        // Buscar el punto de spawn
        GameObject spawn = GameObject.FindGameObjectWithTag("SpawnPoint");
        if (spawn != null)
        {
            // Resetear posición
            transform.localPosition = spawn.transform.localPosition;
            
            // Resetear rotación (en caso de que estuviera con gravedad invertida)
            transform.rotation = Quaternion.Euler(0, 0, 0);
            
            // Resetear gravedad a normal
            isGravedadInvertida = false;
            rb.gravityScale = 1;
            
            // Resetear sprite flip
            spriteRenderer.flipX = false;
            
            Debug.Log($"Jugador respawneado en: {spawn.transform.localPosition}");
        }
        else
        {
            Debug.LogError("No se encontró ningún SpawnPoint con tag 'SpawnPoint'");
            // Como fallback, resetear posición a origen
            transform.localPosition = Vector3.zero;
        }
        
        // Asegurar que el jugador pueda moverse después del respawn
        UnlockMovement();
    }

    public void CambiarGravedad()
    {
        // Activamos o desactivamos la gravedad invertida
        isGravedadInvertida = !isGravedadInvertida;
        
        Debug.Log($"=== CAMBIO DE GRAVEDAD ===");
        Debug.Log($"Gravedad invertida: {isGravedadInvertida}");
        Debug.Log($"Rotation antes: {transform.rotation.eulerAngles}");
        Debug.Log($"GravityScale antes: {rb.gravityScale}");
        
        // Rotamos el personaje según el estado de la gravedad
        if (isGravedadInvertida)
        {
            // Gravedad invertida: rotar 180 grados
            transform.rotation = Quaternion.Euler(0, 0, 180);
            rb.gravityScale = -1;
            Debug.Log("Aplicando gravedad invertida");
        }
        else
        {
            // Gravedad normal: sin rotación
            transform.rotation = Quaternion.Euler(0, 0, 0);
            rb.gravityScale = 1;
            Debug.Log("Aplicando gravedad normal");
        }
        
        Debug.Log($"Rotation después: {transform.rotation.eulerAngles}");
        Debug.Log($"GravityScale después: {rb.gravityScale}");
        Debug.Log("========================");
        
        // Resetear la velocidad Y para evitar comportamientos extraños en la transición
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
    }
    
    private void OnDestroy()
    {
        // Limpiar la InputAction cuando se destruya el objeto
        if (testGravityAction != null)
        {
            testGravityAction.Disable();
            testGravityAction.Dispose();
        }
    }
    
    // Método para testear la gravedad manualmente (puedes llamarlo desde el inspector o con una tecla)
    [System.Obsolete("Solo para testing")]
    public void TestCambiarGravedad()
    {
        Debug.Log("TEST: Forzando cambio de gravedad");
        CambiarGravedad();
    }

    // Método para visualizar el detector en el editor
    private void OnDrawGizmosSelected()
    {
        if (detector != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(detector.position, sizeDetector);
        }
    }

    public void RestaurarGravedad()
    {
        // Restaurar la gravedad normal
        isGravedadInvertida = false;

        // Restaurar la rotación del personaje a su estado normal
        transform.rotation = Quaternion.Euler(0, 0, 0);

        // Ajustamos la gravedad global a su valor normal
        body.gravityScale = 1;  // Vuelve la gravedad a la normal
    }
}
