using System.Collections.Generic;
using UnityEngine;

public class GlowHighlight : MonoBehaviour
{
    private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();
    private Dictionary<Color, Material> glowMaterialCache = new Dictionary<Color, Material>();

    [SerializeField] private Material glowMaterial;
    private bool isGlowing = false;
    private Color validSpaceColor = Color.green;
    private Color originalGlowColor;

    private void Awake()
    {
        CacheOriginalMaterials();
        originalGlowColor = glowMaterial.GetColor("_GlowColor");
    }

    private void CacheOriginalMaterials()
    {
        foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
        {
            originalMaterials[renderer] = renderer.materials;
        }
    }

    internal void ResetGlowHighlight()
    {
        if (isGlowing == false) return;
        foreach (Renderer renderer in originalMaterials.Keys)
        {
            Material[] mats = renderer.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i].SetColor("_GlowColor", originalGlowColor);
            }
        }
    }

    internal void HighlightValidPath()
    {
        if (isGlowing == false) return;
        foreach (Renderer renderer in originalMaterials.Keys)
        {
            Material[] mats = renderer.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i].SetColor("_GlowColor", validSpaceColor);
            }
        }
    }
    public void ToggleGlow(bool state)
    {
        if (isGlowing == state) return;
        isGlowing = state;

        foreach (var renderer in originalMaterials.Keys)
        {
            if (isGlowing)
            {
                Material[] glowMats = new Material[renderer.materials.Length];
                for (int i = 0; i < renderer.materials.Length; i++)
                {
                    if (!glowMaterialCache.TryGetValue(originalMaterials[renderer][i].color, out glowMats[i]))
                    {
                        glowMats[i] = new Material(glowMaterial)
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
            }
        }
    }
}