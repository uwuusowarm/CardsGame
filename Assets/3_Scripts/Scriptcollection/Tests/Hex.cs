using System;
using UnityEngine;

[SelectionBase]
public class Hex : MonoBehaviour
{
    [SerializeField] private GlowHighlight highlight;
    private HexCoordinates hexCoordinates;
    public Vector3Int HexCoords { get; set; }
    public Vector3Int hexCoords => hexCoordinates.GetHexCoords();

    [SerializeField] private HexType hexType;
    
    public Unit UnitOnHex { get; private set; }
    public EnemyUnit EnemyUnitOnHex { get; private set; }

    [Tooltip("ID of the room this hex belongs to. Set by the Level Painter tool.")]
    public int RoomID = 1;

    [Tooltip("A non-unit object placed on this hex (e.g., door, chest, obstacle).")]
    public GameObject PlacedObject { get; private set; }
    
    private ChestController chestOnTile;

    public int GetCost()
        => hexType switch
        {
            HexType.Difficult => 2,
            HexType.Default => 1,
            HexType.Road => 0,
            _ => throw new Exception($"Hex of type {hexType} not supported")
        };

    public bool IsObstacle()
    {
        return this.hexType == HexType.Obstacle || UnitOnHex != null || EnemyUnitOnHex != null || PlacedObject != null;
    }
    private void Awake()
    
    {
        hexCoordinates = GetComponent<HexCoordinates>();
        highlight = GetComponent<GlowHighlight>();

        Transform propsTransform = transform.Find("Props");
        if (propsTransform != null && propsTransform.childCount > 0)
        {
            PlacedObject = propsTransform.GetChild(0).gameObject;
        }
    }


    public void EnableHighlight()
    {
        highlight.ToggleGlow(true);
    }
    public void DisableHighlight()
    {
        highlight.ToggleGlow(false);
    }
    internal void ResetHighlight()
    {
        highlight.ResetGlowHighlight();
    }
    internal void HighLightPath()
    {
        highlight.HighlightValidPath();
    }
    public bool IsOccupied()
    {
        return IsObstacle(); 
    }

    public void SetUnit(Unit unit)
    {
        if (unit != null) UnitOnHex = unit;
    }

    public void SetEnemyUnit(EnemyUnit enemy)
    {
        if (enemy != null) EnemyUnitOnHex = enemy;
    }
    
    public void ClearUnit()
    {
        UnitOnHex = null;
    }
    
    public void ClearEnemyUnit()
    {
        EnemyUnitOnHex = null;
    }

    public void SetPlacedObject(GameObject obj)
    {
        if (PlacedObject != null)
        {
            DestroyImmediate(PlacedObject);
        }
        PlacedObject = obj;
    }


    public void ClearPlacedObject()
    {
        if (PlacedObject != null)
        {
            DestroyImmediate(PlacedObject);
            PlacedObject = null;
        }
    }


    public void ClearAll()
    {
        UnitOnHex = null;
        EnemyUnitOnHex = null;
    }

    public bool HasEnemyUnit()
    {
        return EnemyUnitOnHex != null;
    }

    public void SetChest(ChestController chest)
    {
        chestOnTile = chest;
    }

    public void ClearChest()
    {
        chestOnTile = null;
    }
    
    public ChestController GetChest()
    {
        return chestOnTile;
    }
    
    public HexType HexType
    {
        get => hexType;
        set => hexType = value;
    }
}

public enum HexType
{
    None,
    Default,
    Difficult,
    Road,
    Water,
    Obstacle
}
