using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

[SelectionBase]
public class Hex : MonoBehaviour
{
    [SerializeField] private GlowHighlight highlight;
    private HexCoordinates hexCoordinates;
    [SerializeField] private HexType hexType;
    public Unit UnitOnHex { get; private set; }

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
        return this.hexType == HexType.Obstacle || UnitOnHex != null;
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
        return UnitOnHex != null || this.hexType == HexType.Obstacle;
    }

    public void SetUnit(Unit unit)
    {
        if (unit != null)
        {
            //Debug.Log($"Setting unit {unit.name} on hex {hexCoords}");
            UnitOnHex = unit;
        }
    }

    public void ClearUnit()
    {
        UnitOnHex = null;
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