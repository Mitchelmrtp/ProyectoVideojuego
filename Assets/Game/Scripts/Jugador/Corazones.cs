using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Corazones : MonoBehaviour
{
    public List<Image> listaCorazones;
    public GameObject corazonPrefab;
    public PlayerController playerController;
    public int indexActual;
    public Sprite corazonLleno;
    public Sprite CorazonVacio;

    private void Awake(){
        playerController.cambioVida.AddListener(CambiarCorazones);
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
        indexActual = cantidadVidaMaxima; // El índice actual representa la vida actual
    }
    private void cambiarVida(int vidaActual){
        // Validar que vidaActual sea válido
        if (vidaActual < 0) vidaActual = 0;
        if (vidaActual > listaCorazones.Count) vidaActual = listaCorazones.Count;
        
        if (vidaActual < indexActual){
            quitarCorazones(vidaActual);
        }else if (vidaActual > indexActual){
            agregarCorazones(vidaActual);
        }
        // Si vidaActual == indexActual, no hacer nada
    }
    
    private void quitarCorazones(int vidaActual){
        // Quitar corazones desde indexActual hasta vidaActual
        for(int i = indexActual; i > vidaActual; i--){
            int heartIndex = i - 1; // Convertir vida a índice (vida 1 = índice 0)
            if (heartIndex >= 0 && heartIndex < listaCorazones.Count)
            {
                listaCorazones[heartIndex].sprite = CorazonVacio;
            }
        }
        indexActual = vidaActual; // Actualizar índice actual
    }
    
    private void agregarCorazones(int vidaActual){
        // Agregar corazones desde indexActual hasta vidaActual
        for (int i = indexActual; i < vidaActual; i++){
            if (i >= 0 && i < listaCorazones.Count)
            {
                listaCorazones[i].sprite = corazonLleno;
            }
        }
        indexActual = vidaActual; // Actualizar índice actual
    }

}
