using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

[SelectionBase]
public class Hex : MonoBehaviour
{
    [SerializeField] private GlowHighlight highlight;
    private HexCoordinates hexCoordinates;
    [SerializeField] private HexType hexType;
    public Unit UnitOnHex { get; private set; }
    public EnemyUnit EnemyUnitOnHex { get; private set; }

    public Vector3Int hexCoords => hexCoordinates.GetHexCoords();

    public Vector3Int HexCoords { get; set; }

    public int GetCost()
        => hexType switch
        {
            HexType.Difficult => 20,
            HexType.Default => 10,
            HexType.Road => 5,
            _ => throw new Exception($"Hex of type {hexType} not supported")
        };

    public bool IsObstacle()
    {
        return this.hexType == HexType.Obstacle || UnitOnHex != null || EnemyUnitOnHex != null;
    }

    private void Awake()
    {
        hexCoordinates = GetComponent<HexCoordinates>();
        highlight = GetComponent<GlowHighlight>();
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
        return UnitOnHex != null || this.hexType == HexType.Obstacle || EnemyUnitOnHex != null;
    }

    public void SetUnit(Unit unit)
    {
        if (unit != null)
        {
            Debug.Log($"Setting unit {unit.name} on hex {hexCoords}");
            UnitOnHex = unit;
        }
    }

    public void SetEnemyUnit(EnemyUnit enemy)
    {
        if (enemy != null)
        {
            Debug.Log($"Setting unit {enemy.name} on hex {hexCoords}");
            EnemyUnitOnHex = enemy;
        }
    }
    
    public void ClearUnit()
    {
        UnitOnHex = null;
    }
    
    public void ClearEnemyUnit()
    {
        EnemyUnitOnHex = null;
    }

    public void ClearAll()
    {
        UnitOnHex = null;
        EnemyUnitOnHex = null;
    }

    private Unit unitOnHex;
    
    

    public Unit GetUnit()
    {
        return unitOnHex;
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