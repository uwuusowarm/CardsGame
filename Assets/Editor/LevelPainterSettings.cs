// --- START OF FILE LevelPainterSettings.cs ---

using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PlaceableObject
{
    public string name;
    public GameObject prefab;
    public bool isDoor; 
    
    [Tooltip("The default local Y-position offset when this object is placed on a hex.")]
    public float yOffset = 1.421f;
}

[CreateAssetMenu(fileName = "LevelPainterSettings", menuName = "Level Design/Level Painter Settings")]
public class LevelPainterSettings : ScriptableObject
{
    [Header("Hex Prefabs")]
    public List<HexPrefabMapping> hexPrefabs = new List<HexPrefabMapping>();

    [Header("Placeable Objects")]
    [Tooltip("Objects that can be placed on a hex using the 'Select' tool or the object brush.")]
    public List<PlaceableObject> placeableObjects = new List<PlaceableObject>();
}