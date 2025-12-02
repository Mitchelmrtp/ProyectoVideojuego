using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Versión Mother con:
// - Intro de BOSS FINAL (llamado desde un trigger)
// - Panel de "Ganaste nivel 1" al morir
// - Titileo del sprite al recibir daño
public class Mother : MonoBehaviour
{
    private Animator animator;
    public Rigidbody2D rb2D;
    public Transform jugador;

    [Header("Distancias - CONTROLADAS POR ANIMATOR")]
    [Tooltip("Estas son solo de referencia, las transiciones están en el Animator Controller")]
    public float distanciaDeteccion = 10f;
    public float distanciaPerdida = 15f;
    
    [Header("Arena Combat Limits")]
    [Tooltip("Límites de movimiento desde la posición inicial")]
    public float arenaLimitLeft = 10f;   // Distancia máxima a la izquierda
    public float arenaLimitRight = 10f;  // Distancia máxima a la derecha
    public float arenaLimitBottom = 5f;  // Distancia máxima hacia abajo
    public float arenaLimitTop = 5f;     // Distancia máxima hacia arriba
    private Vector2 arenaLimitsX;        // Límites horizontales calculados
    private Vector2 arenaLimitsY;        // Límites verticales calculados
    
    [Header("Debug")]
    public bool mostrarDebugDistancias = true;

    [Header("Ataques")]
    public GameObject ataque;
    public GameObject habilidad;
    public GameObject Llave;

    [Header("Projectile speeds (fallback si el proyectil no tiene script)")]
    public float ataqueSpeed = 6f;
    public float habilidadSpeed = 4f;

    [Header("Vida")]
    public float vida = 100f;
    public BarraDeVida barraDeVida;
    public GameObject BarraVida;   // Barra de vida del boss (GameObject de UI)

    [Header("UI Boss")]
    public GameObject panelBossIntro;  // Panel "BOSS FINAL"

    [Header("UI Victoria")]
    public GameObject panelVictoria;   // Panel "Ganaste nivel 1"

    [Header("Feedback de Daño")]
    public SpriteRenderer spriteRenderer;   // Sprite del boss
    public float flashDuration = 0.15f;     // duración total del titileo
    public float flashInterval = 0.05f;     // intervalo entre on/off
    private Coroutine flashRoutine;
    
    [Header("Sistema de Daño Mejorado")]
    public float invincibilityDuration = 0.5f;  // Frames de invencibilidad
    public float teleportDistance = 6f;         // Distancia detrás del jugador para teletransportarse
    public float teleportCooldown = 1.2f;       // Tiempo que Mother NO puede atacar después del teletransporte
    public float counterAttackChance = 0.85f;   // 85% de probabilidad de contraatacar
    public float counterAttackDelay = 0.2f;     // Delay antes de contraatacar
    private bool isInvincible = false;
    private bool canAttackAfterTeleport = true; // Flag para controlar ataques post-teletransporte
    private Coroutine teleportRoutine;

    private bool mirandoDerecha = true;

    [Header("🎵 Música del Jefe Derrotado")]
    public AudioClip musicaVictoria;

    // Variables para respawn y estado inicial
    private Vector3 originalPosition;
    private float originalVida;
    private bool originalMirandoDerecha;
    private bool isDead = false;
    
    // Control de ejecución de ataques
    private bool ataqueEjecutado = false;
    private bool habilidadEjecutada = false;
    private string ultimoEstado = "";

    void Start()
    {
        animator = GetComponent<Animator>();
        rb2D = GetComponent<Rigidbody2D>();

        // Buscar jugador por tag 'Player' si no fue asignado en el Inspector
        if (jugador == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) jugador = p.transform;
        }

        // Inicializar barra de vida si está asignada
        if (barraDeVida != null)
        {
            barraDeVida.InicializarBarraVida(vida);
        }

        // La barra de vida del boss empieza OCULTA hasta que se active el combate
        if (BarraVida != null)
        {
            BarraVida.SetActive(false);
        }

        // Aseguramos que los paneles de UI estén apagados al inicio
        if (panelVictoria != null)
            panelVictoria.SetActive(false);

        if (panelBossIntro != null)
            panelBossIntro.SetActive(false);

        // Guardar estado inicial para respawn
        originalPosition = transform.position;
        originalVida = vida;
        originalMirandoDerecha = mirandoDerecha;
        isDead = false;
        
        // Calcular límites de la arena basados en la posición inicial
        arenaLimitsX = new Vector2(originalPosition.x - arenaLimitLeft, originalPosition.x + arenaLimitRight);
        arenaLimitsY = new Vector2(originalPosition.y - arenaLimitBottom, originalPosition.y + arenaLimitTop);

        Debug.Log($"Mother inicializada - Posición: {originalPosition}, Vida: {originalVida}");
        Debug.Log($"Arena límites X: [{arenaLimitsX.x:F2}, {arenaLimitsX.y:F2}] Y: [{arenaLimitsY.x:F2}, {arenaLimitsY.y:F2}]");
    }

    void Update()
    {
        if (jugador == null || animator == null || isDead) return;

        // Obtener estado actual del Animator
        var currentState = animator.GetCurrentAnimatorStateInfo(0);
        string estadoNombre = "Desconocido";
        
        if (currentState.IsName("idle")) estadoNombre = "Idle";
        else if (currentState.IsName("Run")) estadoNombre = "Run";
        else if (currentState.IsName("Attack")) estadoNombre = "Attack";
        else if (currentState.IsName("Habilidad")) estadoNombre = "Habilidad";
        else if (currentState.IsName("Muerte")) estadoNombre = "Muerte";
        else if (currentState.IsName("Hit")) estadoNombre = "Hit";

        // SISTEMA DE DETECCIÓN Y EJECUCIÓN AUTOMÁTICA DE ATAQUES
        // Resetear flags cuando cambiamos de estado
        if (estadoNombre != ultimoEstado)
        {
            Debug.Log($"🔄 CAMBIO DE ESTADO: {ultimoEstado} → {estadoNombre}");
            ultimoEstado = estadoNombre;
            ataqueEjecutado = false;
            habilidadEjecutada = false;
        }
        
        // EJECUTAR ATAQUE NORMAL cuando estamos en estado Attack al 50% de la animación
        // SOLO si puede atacar (no está en cooldown post-teletransporte)
        if (estadoNombre == "Attack" && !ataqueEjecutado && canAttackAfterTeleport)
        {
            float tiempoNormalizado = currentState.normalizedTime % 1;
            if (tiempoNormalizado >= 0.5f)
            {
                Debug.Log($"🎯 DISPARANDO ATAQUE NORMAL en tiempo {tiempoNormalizado:F3}");
                Atacar();
                ataqueEjecutado = true;
            }
        }
        
        // EJECUTAR HABILIDAD cuando estamos en estado Habilidad al 70% de la animación
        // SOLO si puede atacar (no está en cooldown post-teletransporte)
        if (estadoNombre == "Habilidad" && !habilidadEjecutada && canAttackAfterTeleport)
        {
            float tiempoNormalizado = currentState.normalizedTime % 1;
            if (tiempoNormalizado >= 0.7f)
            {
                Debug.Log($"⚡ DISPARANDO HABILIDAD en tiempo {tiempoNormalizado:F3}");
                UsarHabilidad();
                habilidadEjecutada = true;
            }
        }

        // Actualizar distancia al jugador normalmente
        float distanciaJugador = Vector2.Distance(transform.position, jugador.position);
        animator.SetFloat("distanciaJugador", distanciaJugador);
        
        // Debug detallado
        if (mostrarDebugDistancias && Time.frameCount % 30 == 0)
        {
            Debug.Log($"📊 Distancia: {distanciaJugador:F2} | Estado: {estadoNombre} | Tiempo: {currentState.normalizedTime:F2}");
        }

        MirarJugador();
        
        // Aplicar límites de arena para evitar que Mother salga del área de combate
        EnforceArenaLimits();
    }
    
    // Mantener a Mother dentro de los límites de la arena
    private void EnforceArenaLimits()
    {
        Vector3 pos = transform.position;
        bool positionClamped = false;
        
        // Limitar posición horizontal
        if (pos.x < arenaLimitsX.x)
        {
            pos.x = arenaLimitsX.x;
            positionClamped = true;
        }
        else if (pos.x > arenaLimitsX.y)
        {
            pos.x = arenaLimitsX.y;
            positionClamped = true;
        }
        
        // Limitar posición vertical
        if (pos.y < arenaLimitsY.x)
        {
            pos.y = arenaLimitsY.x;
            positionClamped = true;
        }
        else if (pos.y > arenaLimitsY.y)
        {
            pos.y = arenaLimitsY.y;
            positionClamped = true;
        }
        
        if (positionClamped)
        {
            transform.position = pos;
            // Detener velocidad si salió de los límites
            if (rb2D != null)
            {
                rb2D.linearVelocity = new Vector2(0, rb2D.linearVelocity.y);
            }
            Debug.LogWarning($"⚠️ Mother salió de los límites de la arena. Posición corregida a {pos}");
        }
    }

    // Voltea el sprite según la posición del jugador
    public void MirarJugador()
    {
        if (jugador == null || isDead) return;
        bool jugadorALaDerecha = jugador.position.x > transform.position.x;
        if (jugadorALaDerecha != mirandoDerecha)
        {
            mirandoDerecha = jugadorALaDerecha;
            Vector3 s = transform.localScale;
            s.x = Mathf.Abs(s.x) * (mirandoDerecha ? 1f : -1f);
            transform.localScale = s;
        }
    }

    // Instancia el prefab de ataque - LLAMADO POR ANIMATION EVENT
    public void Atacar()
    {
        Debug.Log("🎯 ===== ATACAR() LLAMADO =====");
        
        if (ataque == null)
        {
            Debug.LogError("❌ CRÍTICO: El prefab 'ataque' está NULL. Ve al Inspector de Mother y asigna el prefab en el campo 'Ataque'");
            return;
        }
        
        if (isDead)
        {
            Debug.Log("⚰️ Mother está muerta, cancelando ataque");
            return;
        }
        
        Debug.Log($"✅ Creando proyectil de ataque en {transform.position}, mirando {(mirandoDerecha ? "DERECHA" : "IZQUIERDA")}");
        
        GameObject nuevo = Instantiate(ataque, transform.position, Quaternion.identity);
        
        if (nuevo == null)
        {
            Debug.LogError("❌ ERROR: Instantiate falló, no se creó el GameObject");
            return;
        }
        
        Debug.Log($"✅ GameObject creado: {nuevo.name}");

        // Configurar dirección con AtaqueNormal
        var ataqueScript = nuevo.GetComponent<AtaqueNormal>();
        if (ataqueScript != null)
        {
            Debug.Log("✅ Componente AtaqueNormal encontrado");
            if (mirandoDerecha)
            {
                ataqueScript.SetDirection(Vector2.right);
                nuevo.transform.localScale = new Vector3(-1, 1, 1);
                Debug.Log("→ Configurado para ir a la DERECHA");
            }
            else
            {
                ataqueScript.SetDirection(Vector2.left);
                nuevo.transform.localScale = new Vector3(1, 1, 1);
                Debug.Log("← Configurado para ir a la IZQUIERDA");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ No se encontró AtaqueNormal, usando Rigidbody2D");
            // Fallback con Rigidbody2D
            var rb = nuevo.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                float dir = mirandoDerecha ? 1f : -1f;
                rb.linearVelocity = new Vector2(dir * ataqueSpeed, rb.linearVelocity.y);
                Vector3 s = nuevo.transform.localScale;
                s.x = Mathf.Abs(s.x) * (mirandoDerecha ? -1f : 1f);
                nuevo.transform.localScale = s;
                Debug.Log($"✅ Rigidbody2D configurado con velocidad: {rb.linearVelocity}");
            }
            else
            {
                Debug.LogError("❌ El prefab no tiene ni AtaqueNormal ni Rigidbody2D!");
            }
        }
        
        Debug.Log("🎯 ===== FIN ATACAR() =====");
    }

    // Método llamado por AnimationEvent en la animación 'MotherHabilty'
    public void UsarHabilidad()
    {
        Debug.Log("⚡ ===== USAR HABILIDAD() LLAMADO =====");
        
        if (habilidad == null)
        {
            Debug.LogError("❌ CRÍTICO: El prefab 'habilidad' está NULL");
            return;
        }
        
        if (isDead)
        {
            Debug.Log("⚰️ Mother está muerta, cancelando habilidad");
            return;
        }
        
        Debug.Log($"✅ Creando proyectil de habilidad en {transform.position}");
        
        GameObject nueva = Instantiate(habilidad, transform.position, Quaternion.identity);
        
        if (nueva == null)
        {
            Debug.LogError("❌ ERROR: Instantiate falló para habilidad");
            return;
        }
        
        Debug.Log($"✅ Habilidad creada: {nueva.name}");

        var habilidadScript = nueva.GetComponent<AtaqueNormal>();
        if (habilidadScript != null)
        {
            if (mirandoDerecha)
            {
                habilidadScript.SetDirection(Vector2.right);
                nueva.transform.localScale = new Vector3(-1, 1, 1);
            }
            else
            {
                habilidadScript.SetDirection(Vector2.left);
                nueva.transform.localScale = new Vector3(1, 1, 1);
            }
            Debug.Log("✅ Habilidad configurada con AtaqueNormal");
        }
        else
        {
            var rb = nueva.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                float dir = mirandoDerecha ? 1f : -1f;
                rb.linearVelocity = new Vector2(dir * habilidadSpeed, rb.linearVelocity.y);
                Vector3 s = nueva.transform.localScale;
                s.x = Mathf.Abs(s.x) * (mirandoDerecha ? -1f : 1f);
                nueva.transform.localScale = s;
                Debug.Log($"✅ Habilidad configurada con Rigidbody2D: {rb.linearVelocity}");
            }
        }
        
        Debug.Log("⚡ ===== FIN USAR HABILIDAD() =====");
    }

    public void TomarDaño(float daño)
    {
        if (isDead || isInvincible) return; // Invencibilidad temporal para evitar stunlock
        
        vida -= daño;
        if (barraDeVida != null)
        {
            barraDeVida.CambiarVidaActual(vida);
            barraDeVida.AnimarDaño();
        }

        // Titileo cada vez que recibe daño
        if (flashRoutine != null)
            StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashOnHit());
        
        // TELETRANSPORTE TÁCTICO en lugar de knockback
        if (teleportRoutine != null)
            StopCoroutine(teleportRoutine);
        teleportRoutine = StartCoroutine(TacticalTeleport());
        
        // Activar invencibilidad temporal
        StartCoroutine(InvincibilityFrames());

        if (vida <= 0f)
        {
            if (!isDead)
            {
                isDead = true;
                if (animator != null) animator.SetTrigger("Muerte");
                if (BarraVida != null) BarraVida.SetActive(false);
            }
        }
        else
        {
            if (animator != null) animator.SetTrigger("Hit");
            
            // Posibilidad de contraatacar después del teletransporte
            if (Random.value < counterAttackChance)
            {
                StartCoroutine(CounterAttack());
            }
        }
    }

    // Llamar desde animación al morir (Animation Event)
    public void Muerte()
    {
        if (Llave != null) Instantiate(Llave, transform.position, Quaternion.identity);

        // Activar mensaje de victoria
        if (panelVictoria != null)
        {
            panelVictoria.SetActive(true);
        }

        // ⭐ CAMBIO DE MÚSICA CUANDO EL JEFE MUERE ⭐
        if (musicaVictoria != null)
        {
            MusicManager.Instance.PlayMusic(musicaVictoria);
            Debug.Log("🎵 Música de victoria reproducida");
        }
        else
        {
            Debug.LogWarning("⚠ No se asignó musicaVictoria en el inspector.");
        }

        Destroy(gameObject);
    }

    // Método llamado por el trigger para iniciar la intro de boss
    public void ShowBossIntro()
    {
        if (isDead) return;

        if (panelBossIntro != null)
        {
            StartCoroutine(MostrarIntroBoss());
        }

        // Activar barra de vida cuando empieza el combate
        if (BarraVida != null)
        {
            BarraVida.SetActive(true);
        }
    }

    // Coroutine para mostrar el cartel de "BOSS FINAL"
    private IEnumerator MostrarIntroBoss()
    {
        panelBossIntro.SetActive(true);
        yield return new WaitForSeconds(3f); // tiempo que se ve el mensaje
        panelBossIntro.SetActive(false);
    }

    // Titileo del sprite al recibir daño
    private IEnumerator FlashOnHit()
    {
        if (spriteRenderer == null)
            yield break;

        float elapsed = 0f;
        bool visible = true;

        while (elapsed < flashDuration)
        {
            visible = !visible;
            spriteRenderer.enabled = visible;

            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }

        // Aseguramos que quede visible al final
        spriteRenderer.enabled = true;
        flashRoutine = null;
    }
    
    // TELETRANSPORTE TÁCTICO - Aparece detrás del jugador a distancia de ataque
    private IEnumerator TacticalTeleport()
    {
        if (jugador == null || animator == null)
        {
            teleportRoutine = null;
            yield break;
        }
        
        Debug.Log("✨ TELETRANSPORTE TÁCTICO ACTIVADO");
        
        // BLOQUEAR ATAQUES durante el teletransporte y cooldown
        canAttackAfterTeleport = false;
        
        // Efecto visual breve (el flash ya está activo)
        yield return new WaitForSeconds(0.1f);
        
        // Calcular posición a distancia táctica del jugador (aleatoriamente a izquierda o derecha)
        float offsetX = Random.Range(-1f, 1f) > 0 ? teleportDistance : -teleportDistance;
        Vector3 nuevaPosicion = jugador.position + new Vector3(offsetX, 0, 0);
        
        // VERIFICAR QUE LA NUEVA POSICIÓN ESTÉ DENTRO DE LOS LÍMITES DE LA ARENA
        nuevaPosicion.x = Mathf.Clamp(nuevaPosicion.x, arenaLimitsX.x, arenaLimitsX.y);
        nuevaPosicion.y = Mathf.Clamp(nuevaPosicion.y, arenaLimitsY.x, arenaLimitsY.y);
        
        Debug.Log($"🎯 Posición de teletransporte calculada: {nuevaPosicion} (dentro de límites)");
        
        // TELETRANSPORTE INSTANTÁNEO
        transform.position = nuevaPosicion;
        
        // Asegurar que Mother mire hacia el jugador inmediatamente
        MirarJugador();
        
        // Actualizar distancia en el animator para que entre en rango de ataque
        float distanciaReal = Vector2.Distance(transform.position, jugador.position);
        animator.SetFloat("distanciaJugador", distanciaReal);
        
        Debug.Log($"✅ Teletransporte completado a {nuevaPosicion} - Distancia al jugador: {distanciaReal:F2}");
        
        // Detener cualquier velocidad residual
        if (rb2D != null)
        {
            rb2D.linearVelocity = new Vector2(0, rb2D.linearVelocity.y);
        }
        
        // ESPERAR COOLDOWN antes de permitir ataques nuevamente
        Debug.Log($"⏳ Esperando {teleportCooldown}s antes de permitir ataques...");
        yield return new WaitForSeconds(teleportCooldown);
        
        // DESBLOQUEAR ATAQUES
        canAttackAfterTeleport = true;
        Debug.Log("✅ Mother puede atacar nuevamente");
        
        teleportRoutine = null;
    }
    
    // Frames de invencibilidad para evitar stunlock
    private IEnumerator InvincibilityFrames()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }
    
    // Contraataque después de recibir daño - SIEMPRE A DISTANCIA
    private IEnumerator CounterAttack()
    {
        // Esperar el cooldown completo del teletransporte + delay adicional
        yield return new WaitForSeconds(teleportCooldown + counterAttackDelay);
        
        if (isDead || jugador == null || !canAttackAfterTeleport) yield break;
        
        // FORZAR transición a estado Habilidad en el Animator
        // Esto asegura que Mother haga la animación antes de disparar
        Debug.Log("💥 Contraataque activado - Forzando estado Habilidad");
        
        if (animator != null)
        {
            // Forzar distancia que active Habilidad (NumeroAleatorio = 0)
            animator.SetInteger("NumeroAleatorio", 0);
            
            // Dar tiempo para que entre en el estado y la animación dispare normalmente
            yield return new WaitForSeconds(1.5f);
        }
    }

    // Método público para respawn/reinicio
    public void Respawn()
    {
        // Restaurar posición y rotación
        transform.position = originalPosition;
        transform.rotation = Quaternion.identity;
        
        // Restaurar vida y estado
        vida = originalVida;
        isDead = false;
        
        // Restaurar dirección
        mirandoDerecha = originalMirandoDerecha;
        
        // Reactivar gameObject si estaba desactivado
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
        
        // Reactivar colliders
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (var col in colliders)
        {
            col.enabled = true;
        }
        
        // Resetear animator
        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
            animator.ResetTrigger("Muerte");
            animator.ResetTrigger("Hit");
        }
        
        // Restaurar barra de vida (apagada hasta que empiece el combate otra vez)
        if (barraDeVida != null)
        {
            barraDeVida.InicializarBarraVida(vida);
        }
        
        if (BarraVida != null)
        {
            BarraVida.SetActive(false);
        }

        // Asegurar sprite visible y sin titileo
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }

        // Apagar paneles de UI
        if (panelBossIntro != null)
            panelBossIntro.SetActive(false);

        if (panelVictoria != null)
            panelVictoria.SetActive(false);
        
        
        Debug.Log($"Mother: Respawn completado - Posición: {transform.position}, Vida: {vida}, isDead: {isDead}");
    }
    
    // Dibujar límites de la arena en el editor
    private void OnDrawGizmosSelected()
    {
        Vector3 centerPos = Application.isPlaying ? originalPosition : transform.position;
        
        // Calcular límites
        float leftX = centerPos.x - arenaLimitLeft;
        float rightX = centerPos.x + arenaLimitRight;
        float bottomY = centerPos.y - arenaLimitBottom;
        float topY = centerPos.y + arenaLimitTop;
        
        // Dibujar rectángulo de la arena
        Gizmos.color = Color.cyan;
        
        // Esquinas
        Vector3 bottomLeft = new Vector3(leftX, bottomY, centerPos.z);
        Vector3 bottomRight = new Vector3(rightX, bottomY, centerPos.z);
        Vector3 topLeft = new Vector3(leftX, topY, centerPos.z);
        Vector3 topRight = new Vector3(rightX, topY, centerPos.z);
        
        // Líneas del rectángulo
        Gizmos.DrawLine(bottomLeft, bottomRight);
        Gizmos.DrawLine(bottomRight, topRight);
        Gizmos.DrawLine(topRight, topLeft);
        Gizmos.DrawLine(topLeft, bottomLeft);
        
        // Esferas en las esquinas
        Gizmos.DrawSphere(bottomLeft, 0.3f);
        Gizmos.DrawSphere(bottomRight, 0.3f);
        Gizmos.DrawSphere(topLeft, 0.3f);
        Gizmos.DrawSphere(topRight, 0.3f);
        
        // Etiqueta central
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(centerPos + Vector3.up * 2f, "Arena de Combate");
        UnityEditor.Handles.Label(bottomLeft + Vector3.down * 0.5f, $"L:{arenaLimitLeft}");
        UnityEditor.Handles.Label(bottomRight + Vector3.down * 0.5f, $"R:{arenaLimitRight}");
        #endif
    }
}
