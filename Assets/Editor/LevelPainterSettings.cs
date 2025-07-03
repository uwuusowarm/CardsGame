using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LevelPainterSettings", menuName = "Level Design/Level Painter Settings")]
public class LevelPainterSettings : ScriptableObject
{
    [Header("Hex Prefabs")]
    public List<HexPrefabMapping> hexPrefabs = new List<HexPrefabMapping>();

    [Header("Placeable Objects")]
    [Tooltip("Objects that can be placed on a hex using the 'Select' tool.")]
    public List<PlaceableObject> placeableObjects = new List<PlaceableObject>();
}
