using UnityEngine;

public class OrbeRespawnManager : MonoBehaviour
{
    private GameObject[] orbes;

    void Start()
    {
        // Guarda todos los orbes con tag "Diamante" al inicio
        orbes = GameObject.FindGameObjectsWithTag("Diamante");
    }

    public void RespawnOrbes()
    {
        foreach (GameObject orbe in orbes)
        {
            if (orbe != null && !orbe.activeSelf)
            {
                orbe.SetActive(true);
            }
        }
        Debug.Log("💎 Todos los orbes reaparecieron.");
    }
}
