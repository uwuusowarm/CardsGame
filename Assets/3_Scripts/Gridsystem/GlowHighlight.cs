using System.Collections.Generic;
using UnityEngine;

public class GlowHighlight : MonoBehaviour
{
    private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();
    private Dictionary<Color, Material> glowMaterialCache = new Dictionary<Color, Material>();
    [SerializeField] private Material glowMaterial;
    private Material customMaterial; 
    private bool isGlowing = false;
    private Color validSpaceColor = Color.green;
    private Color originalGlowColor;
    
    private void Awake()
    {
        CacheOriginalMaterials();
        if (glowMaterial != null)
        {
            originalGlowColor = glowMaterial.GetColor("_GlowColor");
        }
    }
    
    private void CacheOriginalMaterials()
    {
        foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
        {
            originalMaterials[renderer] = renderer.materials;
        }
    }
    
    public void ResetGlowHighlight()
    {
        if (isGlowing == false) return;
        foreach (Renderer renderer in originalMaterials.Keys)
        {
            Material[] mats = renderer.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i].HasProperty("_GlowColor"))
                {
                    mats[i].SetColor("_GlowColor", originalGlowColor);
                }
            }
        }
    }
    
    public void HighlightValidPath()
    {
        if (isGlowing == false) return;
        foreach (Renderer renderer in originalMaterials.Keys)
        {
            Material[] mats = renderer.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i].HasProperty("_GlowColor"))
                {
                    mats[i].SetColor("_GlowColor", validSpaceColor);
                }
            }
        }
    }
    
    public void SetCustomHighlightMaterial(Material customHighlightMaterial)
    {
        customMaterial = customHighlightMaterial;
    }
    
    public void ToggleGlow(bool state)
    {
        if (isGlowing == state) return;
        isGlowing = state;

        Material materialToUse = customMaterial != null ? customMaterial : glowMaterial;
        
        if (materialToUse == null)
        {
            Debug.LogWarning($"No glow material assigned on {gameObject.name}");
            return;
        }

        foreach (var renderer in originalMaterials.Keys)
        {
            if (isGlowing)
            {
                Material[] glowMats = new Material[renderer.materials.Length];
                for (int i = 0; i < renderer.materials.Length; i++)
                {
                    string cacheKey = $"{materialToUse.name}_{originalMaterials[renderer][i].color}";
                    
                    if (!glowMaterialCache.TryGetValue(originalMaterials[renderer][i].color, out glowMats[i]))
                    {
                        glowMats[i] = new Material(materialToUse)
                        {
                            color = originalMaterials[renderer][i].color
                        };
                        glowMaterialCache[originalMaterials[renderer][i].color] = glowMats[i];
                    }
                }
                renderer.materials = glowMats;
            }
            else
            {
                renderer.materials = originalMaterials[renderer];
                customMaterial = null;
            }
        }
    }
    
    public void SetHighlightColor(Color color)
    {
        if (!isGlowing) return;
        
        foreach (Renderer renderer in originalMaterials.Keys)
        {
            Material[] mats = renderer.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i].HasProperty("_GlowColor"))
                {
                    mats[i].SetColor("_GlowColor", color);
                }
                else if (mats[i].HasProperty("_Color"))
                {
                    mats[i].SetColor("_Color", color);
                }
            }
        }
    }
}