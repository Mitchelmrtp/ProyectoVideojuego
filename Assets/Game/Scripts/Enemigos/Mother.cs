using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mother : MonoBehaviour
{
    private Animator animator;
    public GameObject ataque;
    public GameObject habilidad;
    public Rigidbody2D rb2D;
    public Transform jugador;
    public GameObject Llave;
    private bool mirandoDerecha = true;
    public BarraDeVida barraDeVida;
    public GameObject BarraVida;

    public float vida;

    [Header("⚠️ Configuración ADAPTADA al Animator Controller existente")]
    [Tooltip("Distancias que coinciden con las transiciones del Animator actual")]
    public float distanciaDeteccion = 10.0f;     // Detecta desde distancia media
    public float distanciaPerdida = 15.0f;       // Persigue a distancia media
    public float distanciaAtaque = 3.0f;         // Ataca desde distancia corta (coincide con Animator)
    public float distanciaPostAtaque = 4.0f;     // Se aleja ligeramente después del ataque

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        rb2D = GetComponent<Rigidbody2D>();
        
        // FORZAR VALORES CORRECTOS (en caso de override del Inspector)
        ForceCorrectDistances();
        
        // Log inmediato de valores después de forzar
        Debug.Log($"🔍 VALORES FINALES DESPUÉS DE ForceCorrectDistances():");
        Debug.Log($"📊 distanciaDeteccion = {distanciaDeteccion}");
        Debug.Log($"⚔️ distanciaAtaque = {distanciaAtaque}");
        Debug.Log($"🏃 distanciaPerdida = {distanciaPerdida}");
        Debug.Log($"🛡️ distanciaPostAtaque = {distanciaPostAtaque}");
        
        // Buscar al jugador por tag "Player" en lugar de "Dark"
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            jugador = playerObject.GetComponent<Transform>();
            Debug.Log("✅ Mother: Jugador encontrado por tag 'Player'");
        }
        else
        {
            Debug.LogError("❌ Mother: No se encontró jugador con tag 'Player'");
        }
        
        if (barraDeVida != null)
        {
            barraDeVida.InicializarBarraVida(vida);
            Debug.Log($"💖 Mother: Barra de vida inicializada con {vida} HP");
        }
        else
        {
            Debug.LogWarning("⚠️ Mother: BarraDeVida no está asignada en el inspector");
        }
        
        if (BarraVida != null)
        {
            BarraVida.SetActive(true);
            Debug.Log("📊 Mother: Barra de vida UI activada");
        }
        else
        {
            Debug.LogWarning("⚠️ Mother: BarraVida GameObject no está asignado en el inspector");
        }
        
        Debug.Log($"🎯 Mother inicializada con {vida} HP en posición {transform.position}");
        
        // Mostrar configuraciones recomendadas para 16 PPU
        MostrarConfiguracionesRecomendadas();
        
        // Verificar parámetros del Animator
        VerificarParametrosAnimator();
    }
    
    /*
    // Sistema para forzar a Mother a mantener distancia de hechicera - DESHABILITADO
    // Se confía en el Animator Controller existente para manejar las distancias
    private void ForzarAlejamientoSiEsMuyCorto(float distanciaJugador)
    {
        // Esta función está deshabilitada para usar el Animator Controller existente
    }
    */

    // Verificación para mantener compatibilidad con Animator Controller existente
    private void VerificarValoresCorrectos()
    {
        // Verificar cada 2 segundos
        if (Time.frameCount % 120 == 0)
        {
            bool valoresIncorrectos = false;
            
            if (distanciaAtaque > 5.0f)
            {
                Debug.LogWarning($"⚠️ AJUSTANDO: distanciaAtaque = {distanciaAtaque} (adaptando a Animator existente)");
                distanciaAtaque = 3.0f;
                valoresIncorrectos = true;
            }
            
            if (distanciaDeteccion > 15.0f)
            {
                Debug.LogWarning($"⚠️ AJUSTANDO: distanciaDeteccion = {distanciaDeteccion} (adaptando a Animator existente)");
                distanciaDeteccion = 10.0f;
                valoresIncorrectos = true;
            }
            
            if (valoresIncorrectos)
            {
                distanciaPerdida = 15.0f;
                distanciaPostAtaque = 4.0f;
                Debug.Log($"🔧 VALORES AJUSTADOS para compatibilidad con Animator Controller existente");
            }
        }
    }

    // Función para forzar los valores correctos (compatibles con Animator existente)
    private void ForceCorrectDistances()
    {
        float oldDeteccion = distanciaDeteccion;
        float oldAtaque = distanciaAtaque;
        float oldPerdida = distanciaPerdida;
        float oldPostAtaque = distanciaPostAtaque;
        
        // VALORES COMPATIBLES con el Animator Controller existente
        distanciaDeteccion = 10.0f;   // Detecta desde distancia razonable
        distanciaPerdida = 15.0f;     // Persigue desde distancia media  
        distanciaAtaque = 3.0f;       // Ataca desde distancia que coincide con Animator
        distanciaPostAtaque = 4.0f;   // Se aleja ligeramente después
        
        Debug.Log("🔧 FORCING ANIMATOR-COMPATIBLE DISTANCES:");
        Debug.Log($"📊 Detección: {oldDeteccion:F1} → {distanciaDeteccion:F1}");
        Debug.Log($"⚔️ Ataque: {oldAtaque:F1} → {distanciaAtaque:F1} (compatible con Animator)");
        Debug.Log($"🏃 Pérdida: {oldPerdida:F1} → {distanciaPerdida:F1}");
        Debug.Log($"🛡️ Post-Ataque: {oldPostAtaque:F1} → {distanciaPostAtaque:F1}");
        
        Debug.Log("✅ Valores adaptados al Animator Controller existente que funciona en el otro juego");
    }

    private void MostrarConfiguracionesRecomendadas()
    {
        Debug.Log("� CONFIGURACIÓN CRÍTICA - Mother Hechicera de EXTREMO LARGO ALCANCE:");
        Debug.Log($"📐 TRANSICIONES REQUERIDAS EN ANIMATOR CONTROLLER:");
        Debug.Log($"🎯 Idle → Walk: CUANDO distanciaJugador < {distanciaDeteccion}f");
        Debug.Log($"🚶 Walk → Idle: CUANDO distanciaJugador > {distanciaPerdida}f");  
        Debug.Log($"⚔️ Walk → Attack: CUANDO distanciaJugador < {distanciaAtaque}f");
        Debug.Log($"🔄 Attack → Walk: CUANDO distanciaJugador > {distanciaPostAtaque}f");
        Debug.Log("� PROBLEMA DETECTADO: Si Mother ataca a ~2 unidades, el Animator usa valores antiguos!");
        Debug.Log("💡 SOLUCIÓN: Actualizar TODAS las transiciones del Animator con estos valores exactos");
    }
    
    private void VerificarParametrosAnimator()
    {
        if (animator != null)
        {
            Debug.Log("🎭 Mother: Verificando parámetros del Animator...");
            bool tieneDistanciaJugador = false;
            
            foreach (var parameter in animator.parameters)
            {
                Debug.Log($"📋 Mother: Parámetro encontrado: {parameter.name} ({parameter.type})");
                if (parameter.name == "distanciaJugador")
                {
                    tieneDistanciaJugador = true;
                }
            }
            
            if (!tieneDistanciaJugador)
            {
                Debug.LogWarning("⚠️ Mother: El Animator NO tiene el parámetro 'distanciaJugador' (Float)");
                Debug.LogWarning("💡 Mother: Añade un parámetro Float llamado 'distanciaJugador' en el Animator Controller");
            }
            else
            {
                Debug.Log("✅ Mother: Parámetro 'distanciaJugador' encontrado correctamente");
            }
        }
    }

    void Update()
    {
        // VERIFICACIÓN CONTINUA DE VALORES CORRECTOS
        VerificarValoresCorrectos();
        
        if (jugador != null)
        {
            float distanciaJugador = Vector2.Distance(transform.position, jugador.position);
            
            // ELIMINAR sistema de alejamiento forzado - dejar que Animator maneje el comportamiento
            // El Animator Controller existente ya sabe cómo manejar las distancias
            
            if (animator != null)
            {
                animator.SetFloat("distanciaJugador", distanciaJugador);
                
                // Log cada 60 frames (aproximadamente cada segundo) para debugging
                if (Time.frameCount % 60 == 0)
                {
                    bool jugadorALaDerecha = jugador.position.x > transform.position.x;
                    string estadoEsperado = GetEstadoEsperadoPorDistancia(distanciaJugador);
                    string estadoActual = GetCurrentAnimatorState();
                    
                    Debug.Log($"🔍 Mother: Distancia al jugador: {distanciaJugador:F2}");
                    Debug.Log($"🎭 Mother: Estado actual del animator: {estadoActual}");
                    Debug.Log($"🎯 Estado esperado por distancia: {estadoEsperado}");
                    
                    // DETECTAR PROBLEMA DE ANIMATOR
                    if (estadoActual == "Attack" && distanciaJugador > distanciaAtaque)
                    {
                        Debug.LogError($"🚨 PROBLEMA DETECTADO: Animator en ATTACK pero distancia {distanciaJugador:F2} > {distanciaAtaque}");
                        Debug.LogError($"💡 El Animator Controller tiene transiciones con valores INCORRECTOS!");
                        Debug.LogError($"🔧 SOLUCIÓN: Walk → Attack debe ser 'distanciaJugador < {distanciaAtaque}'");
                        
                        // VERIFICAR SI LOS VALORES SE CAMBIARON
                        if (distanciaAtaque < 30.0f)
                        {
                            Debug.LogError($"🚨 CRÍTICO: distanciaAtaque = {distanciaAtaque} - ¡VALUES FUERON SOBRESCRITOS!");
                            Debug.LogError($"⚠️ Unity Inspector o Animator está sobrescribiendo los valores del código");
                            // Re-forzar valores
                            distanciaAtaque = 50.0f;
                            distanciaDeteccion = 60.0f;
                            distanciaPerdida = 70.0f;
                            distanciaPostAtaque = 52.0f;
                            Debug.LogError($"🔧 VALORES RE-FORZADOS: distanciaAtaque ahora = {distanciaAtaque}");
                        }
                    }
                    
                    Debug.Log($"👁️ Mother: Mirando {(mirandoDerecha ? "derecha" : "izquierda")}, jugador está a la {(jugadorALaDerecha ? "derecha" : "izquierda")}");
                    Debug.Log($"📍 Mother pos: {transform.position.x:F2}, Jugador pos: {jugador.position.x:F2}");
                    
                    // Información adicional sobre movimiento
                    if (rb2D != null)
                    {
                        Debug.Log($"🏃 Mother velocidad actual: {rb2D.linearVelocity.x:F2} (Y: {rb2D.linearVelocity.y:F2})");
                        Debug.Log($"🎯 Mother transform.right: {transform.right.x:F2}");
                    }
                }
            }
            else
            {
                Debug.LogWarning("⚠️ Mother: Animator es null");
            }
        }
        else
        {
            if (Time.frameCount % 120 == 0) // Log cada 2 segundos
            {
                Debug.LogWarning("⚠️ Mother: Jugador es null");
            }
        }
    }
    
    // Método auxiliar para obtener el estado actual del animator
    private string GetCurrentAnimatorState()
    {
        if (animator != null)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            return stateInfo.IsName("Idle") ? "Idle" : 
                   stateInfo.IsName("Attack") ? "Attack" : 
                   stateInfo.IsName("Hit") ? "Hit" : 
                   stateInfo.IsName("Death") ? "Death" : 
                   "Unknown";
        }
        return "Animator Null";
    }
    
    // Función para determinar qué estado debería estar basándose en la distancia
    private string GetEstadoEsperadoPorDistancia(float distancia)
    {
        if (distancia <= distanciaAtaque)
            return "Attack";
        else if (distancia <= distanciaDeteccion)
            return "Walk";
        else if (distancia >= distanciaPerdida)
            return "Idle";
        else
            return "Transición";
    }

    public void TomarDaño(float daño)
    {
        Debug.Log($"💥 Mother recibe daño: {daño}. Vida actual: {vida} -> {vida - daño}");
        
        vida -= daño;
        
        if (barraDeVida != null)
        {
            barraDeVida.CambiarVidaActual(vida);
        }
        
        if (vida <= 0)
        {
            Debug.Log("💀 Mother: Vida agotada, ejecutando animación de muerte");
            if (animator != null)
            {
                animator.SetTrigger("Muerte");
            }
            if (BarraVida != null)
            {
                BarraVida.SetActive(false);
            }
        }
        else
        {
            Debug.Log("💥 Mother: Recibiendo hit, ejecutando animación de daño");
            if (animator != null)
            {
                animator.SetTrigger("Hit");
            }
        }
    }

    private void Muerte()
    {
        Debug.Log("🗝️ Mother: Generando llave y destruyendo GameObject");
        if (Llave != null)
        {
            Instantiate(Llave, transform.position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("⚠️ Mother: Prefab de llave no está asignado");
        }
        Destroy(gameObject);
    }

    public void MirarJugador()
    {
        if (jugador == null) 
        {
            Debug.LogWarning("⚠️ Mother.MirarJugador(): jugador es null");
            return;
        }
        
        // Determinar si necesita voltear
        bool jugadorALaDerecha = jugador.position.x > transform.position.x;
        bool necesitaVoltear = (jugadorALaDerecha && !mirandoDerecha) || (!jugadorALaDerecha && mirandoDerecha);
        
        if (necesitaVoltear)
        {
            mirandoDerecha = !mirandoDerecha;
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y + 180, 0);
            
            // Log para debugging (solo cada 30 frames para no saturar)
            if (Time.frameCount % 30 == 0)
            {
                Debug.Log($"🔄 Mother volteó: ahora mirando {(mirandoDerecha ? "derecha" : "izquierda")}. Jugador en X:{jugador.position.x:F2}, Mother en X:{transform.position.x:F2}");
            }
        }
    }

    // Método para instanciar el ataque
    public void Atacar()
    {
        if (ataque == null)
        {
            Debug.LogWarning("⚠️ Mother: Prefab de ataque no está asignado");
            return;
        }
        
        Debug.Log("⚔️ Mother: Ejecutando ataque");
        GameObject nuevoAtaque = Instantiate(ataque, transform.position, Quaternion.identity);
        AtaqueNormal ataqueScript = nuevoAtaque.GetComponent<AtaqueNormal>();
        
        if (ataqueScript != null)
        {
            if (mirandoDerecha)
            {
                ataqueScript.SetDirection(Vector2.right);
                nuevoAtaque.transform.localScale = new Vector3(-1, 1, 1);
                Debug.Log("➡️ Mother: Ataque dirigido hacia la derecha");
            }
            else
            {
                ataqueScript.SetDirection(Vector2.left);
                nuevoAtaque.transform.localScale = new Vector3(1, 1, 1);
                Debug.Log("⬅️ Mother: Ataque dirigido hacia la izquierda");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ Mother: El prefab de ataque no tiene componente AtaqueNormal");
        }
    }

    private void UsarHabilidad()
    {
        if (habilidad == null)
        {
            Debug.LogWarning("⚠️ Mother: Prefab de habilidad no está asignado");
            return;
        }
        
        Debug.Log("✨ Mother: Usando habilidad especial");
        GameObject nuevaHabilidad = Instantiate(habilidad, transform.position, Quaternion.identity);
        AtaqueNormal habilidadScript = nuevaHabilidad.GetComponent<AtaqueNormal>();
        
        if (habilidadScript != null)
        {
            if (mirandoDerecha)
            {
                habilidadScript.SetDirection(Vector2.right);
                nuevaHabilidad.transform.localScale = new Vector3(-1, 1, 1);
                Debug.Log("➡️ Mother: Habilidad dirigida hacia la derecha");
            }
            else
            {
                habilidadScript.SetDirection(Vector2.left);
                nuevaHabilidad.transform.localScale = new Vector3(1, 1, 1);
                Debug.Log("⬅️ Mother: Habilidad dirigida hacia la izquierda");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ Mother: El prefab de habilidad no tiene componente AtaqueNormal");
        }
    }
}
