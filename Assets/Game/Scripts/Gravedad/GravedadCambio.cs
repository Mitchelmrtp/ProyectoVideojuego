using UnityEngine;

public class GravedadCambio : MonoBehaviour
{
    public float gravedadNormal = -9.81f;  // Gravedad normal
    public float gravedadInvertida = 9.81f; // Gravedad invertida

    private bool gravedadInvertidaActiva = false; // Controla si la gravedad ya está invertida

    // Este método es llamado cuando el personaje entra en la zona de cambio de gravedad
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player") && !gravedadInvertidaActiva)  // Asegúrate de que el personaje tiene el tag "Player" y la gravedad no está invertida aún
        {
            CambiarGravedad(col.gameObject);
        }
    }

    void CambiarGravedad(GameObject personaje)
    {
        // Obtener el PlayerController del personaje
        PlayerController playerController = personaje.GetComponent<PlayerController>();
        
        if (playerController != null)
        {
            // Llamar al método CambiarGravedad del PlayerController
            playerController.SendMessage("CambiarGravedad");
        }

        // Marca que la gravedad está invertida permanentemente
        gravedadInvertidaActiva = true;
    }

    // Si deseas restaurar la gravedad de alguna manera, puedes agregar otro método que se llame cuando lo desees.
    public void RestablecerGravedad(GameObject personaje)
    {
        // Obtener el PlayerController del personaje
        PlayerController playerController = personaje.GetComponent<PlayerController>();
        
        if (playerController != null)
        {
            // Llamar al método CambiarGravedad del PlayerController para restaurar
            playerController.SendMessage("CambiarGravedad");
        }

        // Marca que la gravedad volvió a la normal
        gravedadInvertidaActiva = false;
    }
}
