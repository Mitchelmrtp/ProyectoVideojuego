using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ParallaxManager: Gestor centralizado para el sistema de parallax.
/// Facilita la configuración y gestión de múltiples capas de parallax.
/// </summary>
public class ParallaxManager : MonoBehaviour
{
    [Header("Camera Reference")]
    [Tooltip("Cámara que controla el parallax (CameraScript o CameraController)")]
    public Camera parallaxCamera;

    [Header("Auto Setup")]
    [Tooltip("Configurar automáticamente las capas al inicio")]
    public bool autoSetup = true;
    
    [Tooltip("Prefijo para identificar GameObjects de parallax")]
    public string parallaxLayerPrefix = "ParallaxLayer";

    [Header("Layer Configuration")]
    [Tooltip("Configuraciones predefinidas para las capas")]
    public List<LayerConfig> layerConfigs = new List<LayerConfig>();

    private List<ParallaxLayer> managedLayers = new List<ParallaxLayer>();
    private CameraScript cameraScript;
    private CameraController cameraController;

    [System.Serializable]
    public class LayerConfig
    {
        public string layerName;
        public float parallaxFactor = 0.5f;
        public bool enableYParallax = false;
        public float yParallaxFactor = 0.5f;
    }

    void Start()
    {
        SetupCameraReference();
        
        if (autoSetup)
        {
            AutoSetupLayers();
        }
    }

    void SetupCameraReference()
    {
        if (parallaxCamera == null)
            parallaxCamera = Camera.main;

        // Try to get camera scripts
        if (parallaxCamera != null)
        {
            cameraScript = parallaxCamera.GetComponent<CameraScript>();
            cameraController = parallaxCamera.GetComponent<CameraController>();

            // Subscribe to camera movement events
            if (cameraScript != null)
                cameraScript.onCameraTranslate += OnCameraMove;
            else if (cameraController != null)
                cameraController.onCameraTranslate += OnCameraMoveXY;
        }
    }

    void AutoSetupLayers()
    {
        // Find all ParallaxLayer components in children
        ParallaxLayer[] layers = GetComponentsInChildren<ParallaxLayer>();
        
        foreach (ParallaxLayer layer in layers)
        {
            if (!managedLayers.Contains(layer))
            {
                managedLayers.Add(layer);
                
                // Apply configuration if available
                LayerConfig config = GetConfigForLayer(layer.name);
                if (config != null)
                {
                    ApplyConfig(layer, config);
                }
            }
        }

        // Create layers from GameObject names matching the prefix
        foreach (Transform child in transform)
        {
            if (child.name.StartsWith(parallaxLayerPrefix))
            {
                ParallaxLayer layer = child.GetComponent<ParallaxLayer>();
                if (layer == null)
                {
                    layer = child.gameObject.AddComponent<ParallaxLayer>();
                }

                if (!managedLayers.Contains(layer))
                {
                    managedLayers.Add(layer);
                }
            }
        }

        Debug.Log($"ParallaxManager: Configuradas {managedLayers.Count} capas de parallax");
    }

    LayerConfig GetConfigForLayer(string layerName)
    {
        foreach (LayerConfig config in layerConfigs)
        {
            if (config.layerName == layerName)
                return config;
        }
        return null;
    }

    void ApplyConfig(ParallaxLayer layer, LayerConfig config)
    {
        layer.parallaxFactor = config.parallaxFactor;
        layer.enableYParallax = config.enableYParallax;
        layer.yParallaxFactor = config.yParallaxFactor;
    }

    void OnCameraMove(float deltaX)
    {
        MoveLayers(deltaX, 0);
    }

    void OnCameraMoveXY(float deltaX, float deltaY)
    {
        MoveLayers(deltaX, deltaY);
    }

    void MoveLayers(float deltaX, float deltaY)
    {
        foreach (ParallaxLayer layer in managedLayers)
        {
            if (layer != null)
            {
                layer.Move(deltaX, deltaY);
            }
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (cameraScript != null)
            cameraScript.onCameraTranslate -= OnCameraMove;
        else if (cameraController != null)
            cameraController.onCameraTranslate -= OnCameraMoveXY;
    }

    // Public methods for runtime management
    public void AddLayer(ParallaxLayer layer)
    {
        if (layer != null && !managedLayers.Contains(layer))
        {
            managedLayers.Add(layer);
        }
    }

    public void RemoveLayer(ParallaxLayer layer)
    {
        if (managedLayers.Contains(layer))
        {
            managedLayers.Remove(layer);
        }
    }

    public void RefreshLayers()
    {
        managedLayers.Clear();
        AutoSetupLayers();
    }

    public void SetAllLayersParallaxFactor(float factor)
    {
        foreach (ParallaxLayer layer in managedLayers)
        {
            if (layer != null)
                layer.parallaxFactor = factor;
        }
    }

    // Create a new layer configuration
    [ContextMenu("Add Layer Config")]
    void AddLayerConfig()
    {
        layerConfigs.Add(new LayerConfig());
    }

    [ContextMenu("Apply All Configurations")]
    void ApplyAllConfigurations()
    {
        foreach (ParallaxLayer layer in managedLayers)
        {
            LayerConfig config = GetConfigForLayer(layer.name);
            if (config != null)
            {
                ApplyConfig(layer, config);
            }
        }
    }
}