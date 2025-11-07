using UnityEngine;

[ExecuteInEditMode]
public class ParallaxLayer : MonoBehaviour
{
    [Header("Parallax Settings")]
    [Tooltip("Factor de parallax. Valores más altos = más movimiento")]
    public float parallaxFactor = 0.5f;
    
    [Header("Y Parallax (Optional)")]
    [Tooltip("Habilitar parallax vertical")]
    public bool enableYParallax = false;
    
    [Tooltip("Factor de parallax vertical. Valores más altos = más movimiento")]
    public float yParallaxFactor = 0.3f;

    public void Move(float delta)
    {
        Vector3 newPos = transform.localPosition;
        newPos.x -= delta * parallaxFactor;

        transform.localPosition = newPos;
    }
    
    public void Move(float deltaX, float deltaY)
    {
        Vector3 newPos = transform.localPosition;
        newPos.x -= deltaX * parallaxFactor;
        
        if (enableYParallax)
        {
            newPos.y -= deltaY * yParallaxFactor;
        }

        transform.localPosition = newPos;
    }
}