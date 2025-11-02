using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Corazones : MonoBehaviour
{
    public List<Image> listaCorazones;
    public GameObject corazonPrefab;
    public PlayerController2 PlayerController2;
    public int indexActual;
    public Sprite corazonLleno;
    public Sprite CorazonVacio;

    private void Start(){
        // Si no se asignó manualmente, buscar automáticamente
        if (PlayerController2 == null)
        {
            // Primero intentar por componente
            PlayerController2 = FindFirstObjectByType<PlayerController2>();
            
            // Si no se encuentra, intentar por tag "Player"
            if (PlayerController2 == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    PlayerController2 = playerObj.GetComponent<PlayerController2>();
                }
            }
            
            Debug.Log("Buscando PlayerController2 automáticamente...");
        }
        
        if (PlayerController2 != null)
        {
            PlayerController2.cambioVida.AddListener(CambiarCorazones);
            Debug.Log("✅ Conectado exitosamente al PlayerController2");
        }
        else
        {
            Debug.LogError("❌ No se encontró PlayerController2 en la escena. Verifica que:");
            Debug.LogError("  1. El Player esté instanciado en la escena (no solo como prefab)");
            Debug.LogError("  2. El Player tenga el componente PlayerController2");
            Debug.LogError("  3. El Player tenga el tag 'Player' (opcional)");
        }
    }

    private void CambiarCorazones(int vidaActual){
        if(!listaCorazones.Any()){
            crearCorazones(vidaActual);
        }else{
            cambiarVida(vidaActual);
        }
    }
    private void crearCorazones(int cantidadVidaMaxima){
        for (int i = 0; i < cantidadVidaMaxima; i++){
            GameObject corazon = Instantiate(corazonPrefab, transform);
            listaCorazones.Add(corazon.GetComponent<Image>());
        }
        indexActual = cantidadVidaMaxima - 1;
    }
    private void cambiarVida(int vidaActual){
        if (vidaActual <= indexActual){
            quitarCorazones(vidaActual);
        }else{
            agregarCorazones(vidaActual);
        }
    }
    private void quitarCorazones(int vidaActual){
        for(int i = indexActual; i >= vidaActual; i--){
            indexActual = i;
            listaCorazones[indexActual].sprite = CorazonVacio;
        }
    }
    private void agregarCorazones(int vidaActual){
        for (int i = indexActual; i < vidaActual; i++){
            indexActual = i;
            listaCorazones[indexActual].sprite = corazonLleno;
        }
    }

}
