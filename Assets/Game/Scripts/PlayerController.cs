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

        // Cuando la gravedad está invertida, el personaje está rotado 180 grados
        // por lo que necesitamos invertir la lógica del flipX
        if (isGravedadInvertida)
        {
            // Con gravedad invertida y rotación 180°, invertimos la lógica de flipX
            sprite.flipX = direction > 0;
        }
        else
        {
            // Comportamiento normal de flipX
            sprite.flipX = direction < 0;
        }

        // Control de movimiento en X
        body.linearVelocity = new Vector2(direction * speed, body.linearVelocity.y);

        // Salto
        if (jumpAction.WasPressedThisFrame() && canJump)
        {
            Debug.Log("El sapo debe saltar...");
            // Si la gravedad está invertida, saltamos en dirección opuesta
            float jumpDirection = isGravedadInvertida ? -jumpImpulse : jumpImpulse;
            body.linearVelocity = new Vector2(body.linearVelocity.x, jumpDirection);
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

    public void CambiarGravedad()
    {
        // Activamos o desactivamos la gravedad invertida
        isGravedadInvertida = !isGravedadInvertida;
        
        // Rotamos el personaje según el estado de la gravedad
        if (isGravedadInvertida)
        {
            // Gravedad invertida: rotar 180 grados
            transform.rotation = Quaternion.Euler(0, 0, 180);
        }
        else
        {
            // Gravedad normal: sin rotación
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }
}
