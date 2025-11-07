using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitPJ : MonoBehaviour
{
    
    public int dañoGolpeEnemigo;
    void OnTriggerEnter2D(Collider2D Collider){
        if (Collider.CompareTag("Player")){
            print("daño");
            Collider.transform.GetComponent<PlayerController>().RecibirDaño(dañoGolpeEnemigo);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
