using UnityEngine;

[System.Serializable]
public class TooltipData
{
    [Header("Content")]
    public string title;
    [TextArea(3, 6)]
    public string description;
    
    [Header("Visual Settings")]
    public Sprite backgroundSprite;
    public Color backgroundColor = Color.white;
    public Vector2 position = Vector2.zero;
    public Vector2 size = new Vector2(300, 150);
    
    [Header("Target Settings")]
    public GameObject targetObject;
    public bool highlightTarget = true;
    public Vector2 targetOffset = new Vector2(100, 50);
    
    [Header("Highlight Settings")]
    [Tooltip("Custom material/shader to use for highlighting this target. If null, will use default highlighting method.")]
    public Material customHighlightMaterial;
    [Tooltip("Custom highlight color to apply (if the material supports it)")]
    public Color highlightColor = Color.yellow;
    
    [Header("Animation")]
    public float fadeInDuration = 0.3f;
    public float fadeOutDuration = 0.2f;
    
    [Header("Interaction")]
    public bool waitForSpecificClick = false;
}