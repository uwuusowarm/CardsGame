using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHighlighter : MonoBehaviour
{
    [SerializeField] private Material highlightMaterial;
    private Material originalMaterial;
    private Renderer enemyRenderer;

    void Start()
    {
        enemyRenderer = GetComponentInChildren<Renderer>();
        if (enemyRenderer != null)
        {
            originalMaterial = enemyRenderer.material;
        }
    }

    public void ToggleHighlight(bool highlight)
    {
        if (enemyRenderer == null) return;

        enemyRenderer.material = highlight ? highlightMaterial : originalMaterial;
    }
}