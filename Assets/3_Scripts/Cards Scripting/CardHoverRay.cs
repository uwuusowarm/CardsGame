using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;





public class GlowHoverMaterial : MonoBehaviour
{
    public Camera mainCamera;

    public Color glowColor = new Color(2f, 2f, 0f, 1f); // leuchtendes Gelb (HDR)
    public float glowPower = 5.0f;

    private GameObject lastHitObject;
    private Material lastHitMaterial;

    private Color originalGlowColor;
    private float originalGlowPower;
    private float originalDynamicGlow;

    void Update()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            GameObject hitObject = hit.collider.gameObject;

            if (hitObject != lastHitObject)
            {
                ClearGlow(); // vorheriges Objekt zurücksetzen

                lastHitObject = hitObject;

                Renderer renderer = hitObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    lastHitMaterial = renderer.material;

                    if (lastHitMaterial.HasProperty("_GlowColor") &&
                        lastHitMaterial.HasProperty("_GlowPower") &&
                        lastHitMaterial.HasProperty("_DynamicGlow"))
                    {
                        // Originalwerte speichern
                        originalGlowColor = lastHitMaterial.GetColor("_GlowColor");
                        originalGlowPower = lastHitMaterial.GetFloat("_GlowPower");
                        originalDynamicGlow = lastHitMaterial.GetFloat("_DynamicGlow");

                        // Glow aktivieren
                        lastHitMaterial.SetColor("_GlowColor", glowColor);
                        lastHitMaterial.SetFloat("_GlowPower", glowPower);
                        lastHitMaterial.SetFloat("_DynamicGlow", 1.0f);
                    }
                }
            }
        }
        else
        {
            ClearGlow(); // nichts getroffen → zurücksetzen
        }
    }

    void ClearGlow()
    {
        if (lastHitMaterial != null)
        {
            if (lastHitMaterial.HasProperty("_GlowColor") &&
                lastHitMaterial.HasProperty("_GlowPower") &&
                lastHitMaterial.HasProperty("_DynamicGlow"))
            {
                // Ursprungswerte wiederherstellen
                lastHitMaterial.SetColor("_GlowColor", originalGlowColor);
                lastHitMaterial.SetFloat("_GlowPower", originalGlowPower);
                lastHitMaterial.SetFloat("_DynamicGlow", originalDynamicGlow);
            }

            lastHitMaterial = null;
            lastHitObject = null;
        }
    }
}

