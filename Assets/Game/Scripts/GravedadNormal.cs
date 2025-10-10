using UnityEngine;

public class GravedadNormal : MonoBehaviour
{
    public GameObject personaje;  // El objeto del personaje

    // Este método es llamado cuando el personaje entra en la zona de cambio de gravedad
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            // Llamamos al método CambiarGravedad en el PlayerController para restaurar la gravedad normal
            PlayerController playerController = personaje.GetComponent<PlayerController>();

            if (playerController != null)
            {
                // Enviar el comando para cambiar la gravedad a normal
                playerController.RestaurarGravedad();
            }
        }
    }
}
