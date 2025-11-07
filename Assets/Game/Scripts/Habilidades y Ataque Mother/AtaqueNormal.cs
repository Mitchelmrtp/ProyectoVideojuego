using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtaqueNormal : MonoBehaviour
{
    [Header("Configuración compatible con Animator existente")]
    [Tooltip("Velocidad y alcance para ataque desde distancia corta")]
    public float speed = 8.0f; // Velocidad adecuada para distancia corta
    [Tooltip("Alcance mejorado para mejor cobertura")]
    public float maxDistance = 35.0f; // Alcance aumentado para mejor rango
    private Vector2 Direction;
    private Rigidbody2D Rigidbody2D;
    private Vector3 startPosition; // Posición inicial para calcular distancia recorrida

    [Tooltip("Daño que causa al jugador")]
    public int dañoGolpeEnemigo = 1;
    
    // ELIMINADO: tiempoDeVida - como en DARK_GAME original

    void OnTriggerEnter2D(Collider2D Collider)
    {
        // SOLO destruir cuando impacte al jugador - ignorar todo lo demás
        if (Collider.CompareTag("Player"))
        {
            float distanciaRecorrida = Vector2.Distance(startPosition, transform.position);
            
            PlayerController playerController = Collider.transform.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.RecibirDaño(dañoGolpeEnemigo);
                Debug.Log($"💥 AtaqueNormal: ¡IMPACTO EXITOSO! Daño aplicado: {dañoGolpeEnemigo}");
                Debug.Log($"📏 Proyectil recorrió {distanciaRecorrida:F1} unidades antes del impacto");
                Debug.Log($"🎯 Impacto en posición: {transform.position} desde origen: {startPosition}");
                Destroy(gameObject);
            }
        }
        // Ignorar COMPLETAMENTE todo lo demás (Ground, plataformas, paredes, etc.)
        else
        {
            // Log opcional para debugging - ver qué está ignorando
            if (Time.frameCount % 60 == 0) // Solo ocasionalmente
            {
                Debug.Log($"🛡️ Proyectil ignora colisión con: {Collider.tag} - continuando viaje");
            }
        }
    }

    void Start()
    {
        Rigidbody2D = GetComponent<Rigidbody2D>();
        startPosition = transform.position; // Guardar posición inicial
        
        // FORZAR VALORES EXTREMOS PARA PROYECTILES DE HECHICERA
        ForceMaxProjectileRange();
        
        Debug.Log($"🚀 AtaqueNormal: Proyectil de ALCANCE EXTREMO creado");
        Debug.Log($"⚡ Velocidad: {speed} - Alcance máximo: {maxDistance} unidades");
        Debug.Log($"🎯 Posición inicial: {startPosition}");
        Debug.Log($"🛡️ Solo se destruye con Player o al alcanzar {maxDistance} unidades");
    }
    
    // Función para forzar valores máximos de alcance
    private void ForceMaxProjectileRange()
    {
        float oldSpeed = speed;
        float oldDistance = maxDistance;
        
        // VALORES COMPATIBLES con el Animator Controller existente
        speed = 8.0f;           // Velocidad apropiada para distancia corta
        maxDistance = 35.0f;    // Alcance mejorado para mejor cobertura
        
        Debug.Log($"🔧 FORCING ANIMATOR-COMPATIBLE PROJECTILE VALUES:");
        Debug.Log($"⚡ Velocidad: {oldSpeed:F1} → {speed:F1} (apropiada para distancia corta)");
        Debug.Log($"📏 Alcance: {oldDistance:F1} → {maxDistance:F1} (mejorado para mejor rango)");
        
        Debug.Log("✅ Proyectil adaptado al Animator Controller existente que funciona");
        Debug.Log($"💡 Mother ataca desde ~3 unidades, proyectil viaja {maxDistance} unidades");
    }

    void FixedUpdate()
    {
        // Movimiento del ataque - simplificado como DARK_GAME
        Rigidbody2D.linearVelocity = Direction * speed;
        
        // VERIFICAR ALCANCE MÁXIMO EXTREMO (200 unidades)
        float distanciaRecorrida = Vector2.Distance(startPosition, transform.position);
        
        // Logging cada 30 frames para monitoring
        if (Time.frameCount % 30 == 0)
        {
            Debug.Log($"🎯 Proyectil: Recorrido {distanciaRecorrida:F1}/{maxDistance} unidades - Vel: {speed}");
        }
        
        if (distanciaRecorrida >= maxDistance)
        {
            Debug.Log($"💥 AtaqueNormal: Proyectil alcanzó DISTANCIA MÁXIMA de {maxDistance} unidades");
            Debug.Log($"📍 Recorrido: {distanciaRecorrida:F2} desde {startPosition} hasta {transform.position}");
            Destroy();
        }
    }

    // Método para establecer la dirección del ataque
    public void SetDirection(Vector2 direction)
    {
        Direction = direction;
    }

    private void Destroy()
    {
        Destroy(gameObject);
    }
}
