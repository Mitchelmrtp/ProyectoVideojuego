using UnityEngine;

public class PortalTrigger : MonoBehaviour
{
    public Animator portalAnimator;  // Referencia al Animator del portal

    // Detectamos la entrada del jugador en la zona del portal
    void OnTriggerEnter2D(Collider2D other)
    {
        // Verificamos si el objeto que entra en el Trigger tiene el Tag "Player"
        if (other.CompareTag("Player"))
        {
            // Activamos el Trigger que hace que el portal se abra
            portalAnimator.SetTrigger("ActivarPortal");
        }
    }

    // Detectamos cuando el jugador sale de la zona del portal
    void OnTriggerExit2D(Collider2D other)
    {
        // Si el jugador sale de la zona del portal, cerramos el portal
        if (other.CompareTag("Player"))
        {
            // Activamos el Trigger para cerrar el portal
            portalAnimator.SetTrigger("CerrarPortal");
        }
    }
}
