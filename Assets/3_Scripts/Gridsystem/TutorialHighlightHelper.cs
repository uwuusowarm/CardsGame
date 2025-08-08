
using UnityEngine;

public class TutorialHighlightHelper : MonoBehaviour
{
    private Color originalColor = Color.white;
    private Material originalMaterial;
    private bool colorStored = false;
    private bool materialStored = false;
    
    public void StoreOriginalColor(Color color)
    {
        if (!colorStored)
        {
            originalColor = color;
            colorStored = true;
        }
    }
    
    public void StoreOriginalMaterial(Material material)
    {
        if (!materialStored)
        {
            originalMaterial = material;
            materialStored = true;
        }
    }
    
    public Color GetOriginalColor()
    {
        return originalColor;
    }
    
    public Material GetOriginalMaterial()
    {
        return originalMaterial;
    }
    
    public void RestoreOriginalMaterial()
    {
        if (materialStored && originalMaterial != null)
        {
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = originalMaterial;
            }
        }
    }
    
    public void RestoreOriginalColor()
    {
        if (colorStored)
        {
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = originalColor;
            }
        }
    }
    
    void OnDestroy()
    {
        colorStored = false;
        materialStored = false;
    }
}