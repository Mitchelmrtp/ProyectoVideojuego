using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float jumpImpulse;
    [Header("Parámetros para detector de piso")]
    [SerializeField] private Transform detector;
    [SerializeField] private float sizeDetector;
    [SerializeField] private LayerMask groundLayer;

    private InputAction moveAction;
    private InputAction jumpAction;
    private Rigidbody2D body;
    private SpriteRenderer sprite;

    private bool isGravedadInvertida = false; // Para saber si la gravedad está invertida
    private bool invertirFlip = false; // Controla si la dirección del flipX debe ser invertida

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
    }

    private void Update()
    {
        // Detectamos si estamos en el piso.
        Collider2D colision = Physics2D.OverlapCircle(detector.position, sizeDetector, groundLayer);
        bool canJump = colision != null;  // Si podemos saltar (estamos en el piso)

        Vector2 move = moveAction.ReadValue<Vector2>();
        int direction = move.x == 0 ? 0 : move.x > 0 ? 1 : -1;

        // Si la variable invertirFlip está activada, invertimos la lógica del flipX
        // Esto cambia la forma en que se determina si el sprite se voltea.
        if (invertirFlip)
        {
            sprite.flipX = direction > 0; // Invertimos la lógica de flipX
        }
        else
        {
            sprite.flipX = direction < 0; // Comportamiento normal de flipX
        }

        // Control de movimiento en X
        body.linearVelocity = new Vector2(direction * speed, body.linearVelocity.y);

        // Salto
        if (jumpAction.WasPressedThisFrame() && canJump)
        {
            Debug.Log("El sapo debe saltar...");
            body.linearVelocity = new Vector2(body.linearVelocity.x, jumpImpulse);
        }

        // Si la gravedad está invertida, la velocidad en Y se invierte también.
        if (isGravedadInvertida)
        {
            body.gravityScale = -1;  // Cambiar la gravedad al modo invertido
        }
        else
        {
            body.gravityScale = 1;  // Normalizar la gravedad
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("DeadZone") || collision.gameObject.CompareTag("Enemy"))
        {
            // Mandamos al player a esa posición.
            GameObject spawn = GameObject.FindGameObjectWithTag("SpawnPoint");
            transform.localPosition = spawn.transform.localPosition;
        }
        else if (collision.CompareTag("ZonaGravedad"))
        {
            // Cambiar la gravedad e invertir la lógica de flip
            CambiarGravedad();
        }
    }

    void CambiarGravedad()
    {
        // Activamos o desactivamos la gravedad invertida
        isGravedadInvertida = !isGravedadInvertida;

        // Invertimos la lógica del flipX
        invertirFlip = !invertirFlip;
    }
}
