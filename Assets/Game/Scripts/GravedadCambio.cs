using UnityEngine;

public class GravedadCambio : MonoBehaviour
{
    public float gravedadNormal = -9.81f;  // Gravedad normal
    public float gravedadInvertida = 9.81f; // Gravedad invertida
    public GameObject personaje;  // El objeto del personaje

    private bool gravedadInvertidaActiva = false; // Controla si la gravedad ya est� invertida

    // Este m�todo es llamado cuando el personaje entra en la zona de cambio de gravedad
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player") && !gravedadInvertidaActiva)  // Aseg�rate de que el personaje tiene el tag "Player" y la gravedad no est� invertida a�n
        {
            CambiarGravedad();
        }
    }

    void CambiarGravedad()
    {
        // Cambia la gravedad a la invertida
        Physics2D.gravity = new Vector2(0, gravedadInvertida);

        // Cambia la rotaci�n de la imagen del personaje a 180 grados
        personaje.transform.rotation = Quaternion.Euler(0, 0, 180);

        // Marca que la gravedad est� invertida permanentemente
        gravedadInvertidaActiva = true;
    }

    // Si deseas restaurar la gravedad de alguna manera, puedes agregar otro m�todo que se llame cuando lo desees.
    public void RestablecerGravedad()
    {
        // Restaura la gravedad a la normal
        Physics2D.gravity = new Vector2(0, gravedadNormal);

        // Restaura la rotaci�n del personaje a la normal
        personaje.transform.rotation = Quaternion.Euler(0, 0, 0);

        // Marca que la gravedad volvi� a la normal
        gravedadInvertidaActiva = false;
    }
}
