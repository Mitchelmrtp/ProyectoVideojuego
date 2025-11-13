using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerController : MonoBehaviour
{
    
    [Header("Detector de suelo")]
    public float speed = 5f;
    public float jumpForce = 10f;
    public float collisionOffset = 0.05f;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI txtGravedad;

    [Header("Parámetros para detector de piso")]
    [SerializeField] private Transform detector;
    [SerializeField] private float sizeDetector = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Límite de cambios de gravedad")]
    [SerializeField] private int maxGravityChanges = 2;
    private int baseMaxGravityChanges;
    private int gravityChangesAvailable;

    [Header("Combate")]
    public SwordAttack swordAttackComponent;

    [Header("Vida")]
    public int maxHealth = 5;
    public int currentHealth { get; private set; }
    public UnityEngine.Events.UnityEvent<int> cambioVida;
    
    [Header("UI")]
    public MenuPausa menuPausa;
    
    [Header("Configuración Extra")]
    public int ScenaActual;
    public bool TieneLlave = false;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    
    public ContactFilter2D movementFilter;
    Vector2 movementInput;
    List<RaycastHit2D> castCollisions = new List<RaycastHit2D>();
    private InputAction jumpAction;
    private InputAction testGravityAction;
    private InputAction moveAction;
    private InputAction attackAction;
    
    bool canMove = true;
    [HideInInspector] public bool isAttacking = false;
    public bool isDying { get; private set; } = false;
    private bool isGravedadInvertida = false;
    private bool isGrounded = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Reinicializar todas las acciones de input manualmente para evitar problemas de referencia
        jumpAction = new InputAction("Jump", InputActionType.Button, "<Keyboard>/space");
        jumpAction.Enable();
        
        testGravityAction = new InputAction("TestGravity", InputActionType.Button, "<Mouse>/rightButton");
        testGravityAction.Enable();
        
        moveAction = new InputAction("Move", InputActionType.Value, "<Keyboard>/a,<Keyboard>/d,<Keyboard>/leftArrow,<Keyboard>/rightArrow");
        moveAction.Enable();
        
        attackAction = new InputAction("Attack", InputActionType.Button, "<Mouse>/leftButton");
        attackAction.Enable();
        
        // Asegurar que todas las variables de estado se reseteen correctamente
        canMove = true;
        isAttacking = false;
        isDying = false;
        isGravedadInvertida = false;

        baseMaxGravityChanges = maxGravityChanges;
        gravityChangesAvailable = maxGravityChanges;
        ActualizarTextoGravedad();

        currentHealth = maxHealth;
        cambioVida.Invoke(currentHealth);

        if (swordAttackComponent == null)
        {
            swordAttackComponent = GetComponent<SwordAttack>();
            if (swordAttackComponent == null)
                swordAttackComponent = GetComponentInChildren<SwordAttack>();
        }
    }

    void Update()
    {
        if (detector != null)
        {
            Collider2D colision = Physics2D.OverlapCircle(detector.position, sizeDetector, groundLayer);
            isGrounded = colision != null;
            
            bool canJump = isGravedadInvertida ? !isGrounded : isGrounded;

        if (jumpAction != null && jumpAction.WasPressedThisFrame() && canJump && canMove)
        {
            float jumpDirection = isGravedadInvertida ? -jumpForce : jumpForce;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpDirection);
        }
            if (animator != null)
            {
                if (HasAnimatorParameter("isJumping", AnimatorControllerParameterType.Bool))
                    animator.SetBool("isJumping", !isGrounded && rb.linearVelocity.y > 0f);
                if (HasAnimatorParameter("isFalling", AnimatorControllerParameterType.Bool))
                    animator.SetBool("isFalling", !isGrounded && rb.linearVelocity.y < 0f);
                if (HasAnimatorParameter("isGrounded", AnimatorControllerParameterType.Bool))
                    animator.SetBool("isGrounded", isGrounded);
                if (HasAnimatorParameter("verticalVel", AnimatorControllerParameterType.Float))
                    animator.SetFloat("verticalVel", rb.linearVelocity.y);
            }
        }

        if (testGravityAction.WasPressedThisFrame())
        {
            if (gravityChangesAvailable > 0)
            {
                CambiarGravedad();
                gravityChangesAvailable--;
                ActualizarTextoGravedad();
            }
        }
        

    }

    void OnMove(InputValue movementValue)
    {
        Vector2 fullInput = movementValue.Get<Vector2>();
        movementInput = new Vector2(fullInput.x, 0);
    }

    void OnFire()
    {
        if (currentHealth <= 0 || isDying)
        {
            return;
        }
        
        if (isAttacking)
        {
            return;
        }
        
        if (!canMove)
        {
            return;
        }

        if (animator == null) return;

        if (HasAnimatorParameter("isAttacking", AnimatorControllerParameterType.Trigger))
            animator.SetTrigger("isAttacking");
        else if (HasAnimatorParameter("isAttacking", AnimatorControllerParameterType.Bool))
            animator.SetBool("isAttacking", true);

        SwordAttack();
    }

    void FixedUpdate()
    {
        if (canMove)
        {
            float horizontalInput = movementInput.x;
            
            rb.linearVelocity = new Vector2(horizontalInput * speed, rb.linearVelocity.y);
            
            if (animator != null)
            {
                if (HasAnimatorParameter("running", AnimatorControllerParameterType.Bool))
                {
                    animator.SetBool("running", horizontalInput != 0.0f);
                }
                else if (HasAnimatorParameter("isMoving", AnimatorControllerParameterType.Bool))
                {
                    animator.SetBool("isMoving", horizontalInput != 0.0f);
                }
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
            if (animator != null)
            {
                if (HasAnimatorParameter("running", AnimatorControllerParameterType.Bool))
                {
                    animator.SetBool("running", false);
                }
                else if (HasAnimatorParameter("isMoving", AnimatorControllerParameterType.Bool))
                {
                    animator.SetBool("isMoving", false);
                }
            }
        }

        rb.gravityScale = isGravedadInvertida ? -1 : 1;
    }

    private void Jump()
    {
        float jumpDirection = isGravedadInvertida ? -jumpForce : jumpForce;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpDirection);
    }

    private void Golpe()
    {
        if (currentHealth <= 0 || isDying)
        {
            return;
        }
        
        if (isAttacking) return;
        
        isAttacking = true;
        
        if (animator != null)
        {
            if (HasAnimatorParameter("isAttacking", AnimatorControllerParameterType.Trigger))
                animator.SetTrigger("isAttacking");
            else if (HasAnimatorParameter("isAttacking", AnimatorControllerParameterType.Bool))
                animator.SetBool("isAttacking", true);
        }
        
        if (swordAttackComponent != null)
        {
            if (spriteRenderer != null && spriteRenderer.flipX)
            {
                swordAttackComponent.AttackLeft();
            }
            else
            {
                swordAttackComponent.AttackRight();
            }
            
            StartCoroutine(AutoStopSwordAttack());
        }
        
        StartCoroutine(ResetAttackCoroutine());
    }
    
    private IEnumerator AutoStopSwordAttack()
    {
        yield return new WaitForSeconds(0.3f);
        if (swordAttackComponent != null)
        {
            swordAttackComponent.StopAttack();
        }
    }
    
    public void SwordAttack()
    {
        Golpe();
    }
    
    public void StartSwordAttack()
    {
        if (swordAttackComponent != null)
        {
            if (spriteRenderer != null && spriteRenderer.flipX)
            {
                swordAttackComponent.AttackLeft();
            }
            else
            {
                swordAttackComponent.AttackRight();
            }
        }
    }
    
    public void StopSwordAttack()
    {
        if (swordAttackComponent != null)
        {
            swordAttackComponent.StopAttack();
        }
    }

    public void EndSwordAttack()
    {
        isAttacking = false;
        
        if (swordAttackComponent != null)
        {
            swordAttackComponent.StopAttack();
        }
        
        if (animator != null && HasAnimatorParameter("isAttacking", AnimatorControllerParameterType.Bool))
        {
            animator.SetBool("isAttacking", false);
        }
    }

    private IEnumerator ResetAttackCoroutine()
    {
        yield return new WaitForSeconds(0.4f);
        EndSwordAttack();
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

    public void RecibirDaño(int daño)
    {
        if (currentHealth <= 0 || isDying)
        {
            return;
        }
        
        currentHealth -= daño;
        currentHealth = Mathf.Max(0, currentHealth); // Asegurar que no sea negativo
        cambioVida.Invoke(currentHealth);
        
        if (currentHealth <= 0 && !isDying)
        {
            Die();
        }
        else if (currentHealth > 0)
        {
            if (animator != null && HasAnimatorParameter("Hit", AnimatorControllerParameterType.Trigger))
            {
                animator.SetTrigger("Hit");
            }
        }
    }

    #region Sistema de Vida y Daño
    public void TakeDamage(int amount)
    {
        RecibirDaño(amount);
    }
    
    public void CurarVida(int cantidadCuracion)
    {
        currentHealth = Mathf.Min(currentHealth + cantidadCuracion, maxHealth);
        cambioVida.Invoke(currentHealth);
    }

    public bool PuedeCurarse()
    {
        return currentHealth < maxHealth;
    }

    public void Die()
    {
        if (isDying) return;
        
        isDying = true;
        currentHealth = 0;
        LockMovement();
        
        // Reproducir animación de muerte (solo una vez)
        if (animator != null)
        {
            // Desactivar otros parámetros que puedan interferir
            if (HasAnimatorParameter("running", AnimatorControllerParameterType.Bool))
                animator.SetBool("running", false);
            if (HasAnimatorParameter("isMoving", AnimatorControllerParameterType.Bool))
                animator.SetBool("isMoving", false);
            if (HasAnimatorParameter("isJumping", AnimatorControllerParameterType.Bool))
                animator.SetBool("isJumping", false);
            if (HasAnimatorParameter("isFalling", AnimatorControllerParameterType.Bool))
                animator.SetBool("isFalling", false);
            if (HasAnimatorParameter("isAttacking", AnimatorControllerParameterType.Bool))
                animator.SetBool("isAttacking", false);
                
            // Activar trigger de muerte
            if (HasAnimatorParameter("Death", AnimatorControllerParameterType.Trigger))
                animator.SetTrigger("Death");
        }
        
        // Esperar a que termine la animación antes de mostrar el panel
        StartCoroutine(ShowDeathPanelDelayed());
    }

    private IEnumerator ShowDeathPanelDelayed()
    {
        // Esperar 2 segundos para que se vea la animación de muerte
        yield return new WaitForSeconds(2f);
        
        // Detener completamente el animator para evitar loops infinitos
        if (animator != null)
        {
            animator.enabled = false;
        }
        
        // Detener cualquier movimiento restante
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false; // Desactivar completamente la física
        }
        
        // Buscar el MenuPausa si no está asignado
        if (menuPausa == null)
        {
            menuPausa = FindFirstObjectByType<MenuPausa>();
        }
        
        // Mostrar el menú de pausa como "menú de muerte"
        if (menuPausa != null)
        {
            menuPausa.PausarJuego();
        }
    }


    



    #endregion

    #region Sistema de Gravedad
    public void CambiarGravedad()
    {
        isGravedadInvertida = !isGravedadInvertida;
        
        if (isGravedadInvertida)
        {
            transform.rotation = Quaternion.Euler(0, 0, 180);
            rb.gravityScale = -1;
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
            rb.gravityScale = 1;
        }
        
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
    }

    public void RestaurarGravedad()
    {
        isGravedadInvertida = false;
        transform.rotation = Quaternion.Euler(0, 0, 0);
        rb.gravityScale = 1;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
    }
    #endregion

    #region Detección de Colisiones y Triggers
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("DeadZone"))
        {
            Die();
        }
        else if (collision.CompareTag("ZonaGravedad"))
        {
            CambiarGravedad();
        }
        else if (collision.CompareTag("Enemy") && !isAttacking)
        {
            RecibirDaño(1);
        }
        else if (collision.CompareTag("Diamante"))
        {
            maxGravityChanges += 1;
            gravityChangesAvailable += 1;
            collision.gameObject.SetActive(false);
            ActualizarTextoGravedad();
        }
        else if (collision.name.ToLower().Contains("portal"))
        {
            int currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
            int nextScene = currentScene + 1;
            
            if (nextScene < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(nextScene);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDying || currentHealth <= 0)
        {
            return;
        }
        
        if (collision.gameObject.CompareTag("Enemy") && !isAttacking)
        {
            RecibirDaño(1);
        }
    }
    #endregion



    #region Métodos Auxiliares
    private void ActualizarTextoGravedad()
    {
        if (txtGravedad != null)
        {
            txtGravedad.text = $" X {gravityChangesAvailable}";
        }
    }

    private bool HasAnimatorParameter(string name, UnityEngine.AnimatorControllerParameterType type)
    {
        if (animator == null) return false;
        foreach (var p in animator.parameters)
        {
            if (p.name == name && p.type == type) return true;
        }
        return false;
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }
    
    private void OnDestroy()
    {
        if (testGravityAction != null)
        {
            testGravityAction.Disable();
            testGravityAction.Dispose();
        }
        
        if (moveAction != null)
        {
            moveAction.Disable();
            moveAction.Dispose();
        }
        
        if (attackAction != null)
        {
            attackAction.Disable();
            attackAction.Dispose();
        }
        
        if (jumpAction != null)
        {
            jumpAction.Disable();
            jumpAction.Dispose();
        }
    }

    private void OnDrawGizmos()
    {
        if (detector != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(detector.position, sizeDetector);
        }
    }
    #endregion
}