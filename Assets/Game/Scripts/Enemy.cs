using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    private string enemyId;

    private void Awake()
    {
        Vector3 pos = transform.localPosition;
        originalPosition = pos;
        limits = new Vector2(pos.x - limitLeft, pos.x + limitRight);

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
        // Reset animator
        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }
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