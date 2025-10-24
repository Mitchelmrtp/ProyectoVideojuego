using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float speedX;
    [SerializeField] private float limitRight;
    [SerializeField] private float limitLeft;

    private Vector2 limits;
    private int direction;
    private Rigidbody2D body;
    private SpriteRenderer sprite;
    private Vector3 originalPosition;
    private Collider2D enemyCollider;

    public Animator animator;

    public float health = 1f;

    private void Awake()
    {
        Vector3 pos = transform.localPosition;
        originalPosition = pos;
        limits = new Vector2(pos.x - limitLeft, pos.x + limitRight);

        body = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        enemyCollider = GetComponent<Collider2D>();

        direction = 1; // Hacia la derecha
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
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
        animator.SetTrigger("Defeated");

        // Desactivar collider para evitar más colisiones
        if (enemyCollider != null)
            enemyCollider.enabled = false;

        // Detener simulación física para evitar empujones o impulsos
        if (body != null)
            body.simulated = false;

        // Destruir enemigo después de 1 segundo (ajusta según duración animación)
        Invoke(nameof(RemoveEnemy), 1f);
    }

    public void RemoveEnemy()
    {
        Destroy(gameObject);
    }

    private void Update()
    {
        if (direction != 0)
        {
            sprite.flipX = direction < 0;
        }
        Vector3 pos = transform.localPosition;
        if (pos.x <= limits.x)
        {
            direction = 1;
        }
        if (pos.x >= limits.y)
        {
            direction = -1;
        }

        // Corregir la asignación de velocidad
        if (body != null && body.simulated)
        {
            Vector2 velocity = body.linearVelocity;
            velocity.x = direction * speedX;
            body.linearVelocity = velocity;
        }
    }

    private void OnDrawGizmos()
    {
        Vector3 pos = originalPosition != Vector3.zero ? originalPosition : transform.localPosition;
        Vector3 posLeft = new Vector3(pos.x - limitLeft, pos.y, pos.z);
        Vector3 posRight = new Vector3(pos.x + limitRight, pos.y, pos.z);
        Gizmos.DrawSphere(posLeft, 0.5f);
        Gizmos.DrawSphere(posRight, 0.5f);
    }
}