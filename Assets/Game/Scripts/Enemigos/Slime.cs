using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Minimal SlimeManager used for respawn support. Keeps registration but no logs.
public static class SlimeManager
{
    private static Dictionary<string, Slime> registeredSlimes = new Dictionary<string, Slime>();

    public static void RegisterSlime(string id, Slime slime)
    {
        if (string.IsNullOrEmpty(id) || slime == null) return;
        registeredSlimes[id] = slime;
    }

    public static void UnregisterSlime(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        registeredSlimes.Remove(id);
    }

    public static void RespawnAll()
    {
        foreach (var kv in registeredSlimes)
        {
            kv.Value?.Respawn();
        }
    }

    public static void Clear()
    {
        registeredSlimes.Clear();
    }
}

    // Simplified Slime behavior: patrol, detect & chase player, contact damage, health & animations.
    public class Slime : MonoBehaviour
    {
        [SerializeField] private float speedX = 1.5f;
        [Header("Patrol Limits (distances from start position)")]
        [SerializeField] private float limitLeft = 1f;
        [SerializeField] private float limitRight = 1f;

        [Header("Player Detection")]
        [SerializeField] private float detectionRange = 3f;
        [SerializeField] private float chaseSpeed = 2.5f;
        [SerializeField] private float attackDamage = 1f;
        [SerializeField] private float attackCooldown = 1.5f;

        [Header("Health")]
        [SerializeField] private float maxHealth = 1f;
        // currentHealth is the runtime value; maxHealth is the inspector-configurable maximum.
        [SerializeField, HideInInspector] private float currentHealth;

        private Vector2 limits;
        private int direction = 1;
        private Rigidbody2D body;
        private SpriteRenderer sprite;
        private Vector3 originalPosition;
        private Collider2D enemyCollider;

        private Transform player;
        private bool isChasingPlayer = false;
        private bool playerDetected = false;
        private float lastAttackTime = 0f;

        // Expose a read/write property for current health. Setting to <=0 triggers Defeated().
        public float Health
        {
            get => currentHealth;
            set
            {
                currentHealth = value;
                if (currentHealth <= 0f) Defeated();
            }
        }
        private string enemyId;
        private Animator animator;

        private void Awake()
        {
            originalPosition = transform.localPosition;
            Vector3 pos = transform.position;
            limits = new Vector2(pos.x - limitLeft, pos.x + limitRight);

            body = GetComponent<Rigidbody2D>();
            sprite = GetComponent<SpriteRenderer>();

            // Find a non-trigger collider if available
            Collider2D[] colliders = GetComponents<Collider2D>();
            foreach (var col in colliders)
            {
                if (!col.isTrigger)
                {
                    enemyCollider = col;
                    break;
                }
            }
            if (enemyCollider == null && colliders.Length > 0) enemyCollider = colliders[0];

            // Stable id for respawn manager
            string sceneName = SceneManager.GetActiveScene().name;
            Vector3 worldPos = transform.position;
            string posKey = string.Format("{0}_{1}_{2}", Mathf.Round(worldPos.x * 100f) / 100f, Mathf.Round(worldPos.y * 100f) / 100f, Mathf.Round(worldPos.z * 100f) / 100f);
            string baseName = gameObject.name.Replace("(Clone)", "").Trim();
            enemyId = $"{sceneName}|{baseName}|{posKey}";

            SlimeManager.RegisterSlime(enemyId, this);
        }

        private void Start()
        {
            animator = GetComponent<Animator>();
            // Initialize runtime health from inspector-configured maxHealth
            currentHealth = maxHealth;

            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

    public void Respawn()
        {
            gameObject.SetActive(true);
            transform.localPosition = originalPosition;
            if (body != null)
            {
                body.simulated = true;
                body.linearVelocity = Vector2.zero;
            }
            if (enemyCollider != null) enemyCollider.enabled = true;
            currentHealth = maxHealth;
            direction = 1;
            isChasingPlayer = false;
            playerDetected = false;
            lastAttackTime = 0f;
            if (animator != null)
            {
                animator.Rebind();
                animator.Update(0f);
            }
        }

        private void OnDestroy()
        {
            SlimeManager.UnregisterSlime(enemyId);
        }

        private void Update()
        {
            if (body == null || !body.simulated) return;

            Vector3 pos = transform.position;
            CheckPlayerProximity();

            if (isChasingPlayer && player != null)
            {
                HandleChasingBehavior(pos);
            }
            else
            {
                HandleNormalPatrolBehavior(pos);
            }

            if (direction != 0 && sprite != null) sprite.flipX = direction < 0;

            UpdateAnimatorParameters();
            ApplyMovement();
        }

        private void UpdateAnimatorParameters()
        {
            if (animator == null) return;
            bool isMoving = direction != 0;
            animator.SetBool("Moving", isMoving);
            if (HasAnimatorParameter("isMoving")) animator.SetBool("isMoving", isMoving);
        }

        private bool HasAnimatorParameter(string paramName)
        {
            if (animator == null) return false;
            foreach (var param in animator.parameters) if (param.name == paramName) return true;
            return false;
        }

        private void CheckPlayerProximity()
        {
            if (player == null) return;
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null && (playerController.isDying || playerController.currentHealth <= 0))
            {
                playerDetected = false;
                isChasingPlayer = false;
                return;
            }
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            playerDetected = distanceToPlayer <= detectionRange;
            isChasingPlayer = playerDetected && distanceToPlayer > 0.5f;
        }

        private void HandleChasingBehavior(Vector3 pos)
        {
            float playerDirection = player.position.x - pos.x;
            direction = playerDirection > 0 ? 1 : -1;
            if (pos.x <= limits.x && direction == -1) direction = 0;
            else if (pos.x >= limits.y && direction == 1) direction = 0;
        }

        private void HandleNormalPatrolBehavior(Vector3 pos)
        {
            if (pos.x <= limits.x) direction = 1;
            else if (pos.x >= limits.y) direction = -1;
        }

        private void ApplyMovement()
        {
            if (body != null && body.simulated)
            {
                Vector2 velocity = body.linearVelocity;
                float currentSpeed = isChasingPlayer ? chaseSpeed : speedX;
                velocity.x = direction * currentSpeed;
                body.linearVelocity = velocity;
            }
        }

        public void TakeDamage(float damage)
        {
            currentHealth -= damage;
            if (currentHealth <= 0f) Defeated();
            else if (animator != null) animator.SetTrigger("Damage");
        }

        public void Defeated()
        {
            if (animator != null) animator.SetTrigger("Defeated");
            if (enemyCollider != null) enemyCollider.enabled = false;
            if (body != null) body.simulated = false;
            Invoke(nameof(DeactivateEnemy), 1f);
        }

        private void DeactivateEnemy()
        {
            gameObject.SetActive(false);
        }

        // Called by animation event (optional)
        public void RemoveEnemy() => DeactivateEnemy();

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Detect player attack hitboxes by common names
            string oname = other.name;
            if (oname.Contains("controladorGolpe") || oname.Contains("Golpe") || oname.Contains("AttackArea") || oname.Contains("SwordHitbox") || oname.Contains("Attack"))
            {
                TakeDamage(1f);
                return;
            }

            if (other.CompareTag("Player"))
            {
                PlayerController pc = other.GetComponent<PlayerController>();
                if (pc == null) return;
                if (pc.isDying || pc.currentHealth <= 0) return;

                if (!pc.isAttacking && Time.time >= lastAttackTime + attackCooldown)
                {
                    lastAttackTime = Time.time;
                    pc.TakeDamage((int)attackDamage);
                }
                else if (pc.isAttacking)
                {
                    TakeDamage(1f);
                }
            }
        }

        private void OnDrawGizmos()
        {
            Vector3 pos = transform.position;
            if (originalPosition != Vector3.zero)
            {
                Vector3 worldOriginalPos = transform.parent != null ? transform.parent.TransformPoint(originalPosition) : originalPosition;
                pos = worldOriginalPos;
            }
            Vector3 posLeft = new Vector3(pos.x - limitLeft, pos.y, pos.z);
            Vector3 posRight = new Vector3(pos.x + limitRight, pos.y, pos.z);
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(posLeft, 0.25f);
            Gizmos.DrawSphere(posRight, 0.25f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
    #if UNITY_EDITOR
            UnityEditor.Handles.Label(pos + Vector3.up * (detectionRange + 0.25f), "Detection Range");
    #endif
        }
    }
