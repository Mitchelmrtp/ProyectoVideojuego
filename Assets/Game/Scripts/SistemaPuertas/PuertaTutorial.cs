using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuertaTutorial : MonoBehaviour
{
    public GameObject Llave;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Llave.SetActive(true);
        }
    }
}
