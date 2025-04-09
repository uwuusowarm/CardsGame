using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class Hex : MonoBehaviour
{
    [SerializeField] private GlowHighlight highlight;
    private HexCoordinates hexCoordinates;
    [SerializeField] private HexType hexType;
    //Feld belegt(test)
    private Unit unitOnHex;

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
        return this.hexType == HexType.Obstacle || unitOnHex != null;
    }

    private void Awake()
    {
        hexCoordinates = GetComponent<HexCoordinates>();
        highlight = GetComponent<GlowHighlight>();
    }

    public void EnableHighlight()
    {
        highlight.ToggleGlow(true);
        Debug.Log("Highlight enabled for " + hexCoords);
    }
    public void DisableHighlight()
    {
        highlight.ToggleGlow(false);
        Debug.Log("Highlight disabled for " + hexCoords);
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
        return unitOnHex != null || this.hexType == HexType.Obstacle;
    }

    public void SetUnit(Unit unit)
    {
        unitOnHex = unit;
    }

    public void ClearUnit()
    {
        unitOnHex = null;
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