using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ParallaxBackground : MonoBehaviour
{
    [Header("Parallax Settings")]
    [Tooltip("La cámara con el componente CameraScript que controla el parallax")]
    public CameraScript parallaxCamera;
    
    [Header("Auto Setup")]
    [Tooltip("Si true, configurará automáticamente las capas hijas como ParallaxLayer")]
    public bool autoSetupLayers = true;

    List<ParallaxLayer> parallaxLayers = new List<ParallaxLayer>();

    void Start()
    {
        if (parallaxCamera == null)
            parallaxCamera = Camera.main.GetComponent<CameraScript>();

        if (parallaxCamera != null)
            parallaxCamera.onCameraTranslate += Move;

        if (autoSetupLayers)
            SetLayers();
    }

    void SetLayers()
    {
        parallaxLayers.Clear();

        for (int i = 0; i < transform.childCount; i++)
        {
            ParallaxLayer layer = transform.GetChild(i).GetComponent<ParallaxLayer>();

            if (layer != null)
            {
                layer.name = "Layer-" + i;
                parallaxLayers.Add(layer);
            }
        }
    }

    void Move(float delta)
    {
        foreach (ParallaxLayer layer in parallaxLayers)
        {
            layer.Move(delta);
        }
    }

    void OnDestroy()
    {
        if (parallaxCamera != null)
            parallaxCamera.onCameraTranslate -= Move;
    }

    // Método público para añadir capas manualmente
    public void AddLayer(ParallaxLayer layer)
    {
        if (layer != null && !parallaxLayers.Contains(layer))
        {
            parallaxLayers.Add(layer);
        }
    }

    // Método público para remover capas
    public void RemoveLayer(ParallaxLayer layer)
    {
        if (parallaxLayers.Contains(layer))
        {
            parallaxLayers.Remove(layer);
        }
    }

    // Refrescar la configuración de capas
    public void RefreshLayers()
    {
        SetLayers();
    }
}