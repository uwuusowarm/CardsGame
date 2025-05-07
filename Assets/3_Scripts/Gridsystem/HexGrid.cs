using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

public class HexGrid : MonoBehaviour
{
    Dictionary<Vector3Int, Hex> hexTileDict = new Dictionary<Vector3Int, Hex>();
    Dictionary<Vector3Int, List<Vector3Int>> hexTileNeighboursDict = new Dictionary<Vector3Int, List<Vector3Int>>();
    public static float xOffset = 2f;
    public static float zOffset = 1.73f;

    public static HexGrid Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        InitializeGrid();
    }

    public void AddMovementPoints(int points)
    {
        if (UnitManager.Instance == null)
        {
            Debug.LogError("UnitManager nicht gefunden!");
            return;
        }
        if (UnitManager.Instance.SelectedUnit == null)
        {
            var playerUnits = FindObjectsOfType<Unit>();
            foreach (var unit in playerUnits)
            {
                if (!unit.IsEnemy)
                {
                    UnitManager.Instance.PrepareUnitForMovement(unit);
                    break;
                }
            }
        }
        if (UnitManager.Instance.SelectedUnit != null)
        {
            UnitManager.Instance.SelectedUnit.AddMovementPoints(points);
        }
        else
        {
            Debug.LogWarning("Keine Einheit für Bewegung gefunden!");
        }
    }
    private void InitializeGrid()
    {
    }

    private void Start()
    {
        Debug.Log("Initializing grid...");
        foreach (Hex hex in FindObjectsOfType<Hex>())
        {
            hexTileDict[hex.hexCoords] = hex;
            Debug.Log($"Registered hex at {hex.hexCoords}");
        }
        foreach (Hex hex in FindObjectsOfType<Hex>())
        {
            hexTileDict[hex.hexCoords] = hex;
            hex.HexCoords = hex.hexCoords;
        }
        foreach (var hexPair in hexTileDict)
        {
            hexTileNeighboursDict[hexPair.Key] = new List<Vector3Int>();

            foreach (var direction in Direction.GetDirectionList(hexPair.Key.z))
            {
                Vector3Int neighborCoords = hexPair.Key + direction;
                if (hexTileDict.ContainsKey(neighborCoords))
                {
                    hexTileNeighboursDict[hexPair.Key].Add(neighborCoords);
                }
            }
        }

        Debug.Log($"Grid initialized with {hexTileDict.Count} hexes");
    }

    public Vector3Int GetClosestHex(Vector3 worldPosition)
    {
        return new Vector3Int(
            Mathf.RoundToInt(worldPosition.x / xOffset),
            0,
            Mathf.RoundToInt(worldPosition.z / zOffset)
        );
    }

    public Hex GetTileAt(Vector3Int hexCoordinates)
    {
        Hex result = null;
        hexTileDict.TryGetValue(hexCoordinates, out result);
        return result;
    }

    public List<Vector3Int> GetNeighborsFor(Vector3Int hexCoordinates)
    {
        if (hexTileDict.ContainsKey(hexCoordinates) == false)
            return new List<Vector3Int>();

        if (hexTileNeighboursDict.ContainsKey(hexCoordinates))
            return hexTileNeighboursDict[hexCoordinates];

        hexTileNeighboursDict[hexCoordinates] = new List<Vector3Int>();

        foreach (var direction in Direction.GetDirectionList(hexCoordinates.z))
        {
            Vector3Int neighborCoords = hexCoordinates + direction;
            if (hexTileDict.ContainsKey(neighborCoords))
            {
                hexTileNeighboursDict[hexCoordinates].Add(neighborCoords);
            }
        }

        return hexTileNeighboursDict[hexCoordinates];
    }
    public List<Vector3Int> GetNeighborsFor(Vector3Int hexCoordinates, int range = 1)
    {
        List<Vector3Int> result = new List<Vector3Int>();

        if (range == 1)
        {
            return GetNeighborsFor(hexCoordinates); 
        }
        else
        {

        }

        return result;
    }
}



public static class Direction
{
    public static List<Vector3Int> directionsOffsetOdd = new List<Vector3Int>
    {
        new Vector3Int(-1,0,1),
        new Vector3Int(0,0,1),
        new Vector3Int(1,0,0),
        new Vector3Int(0,0,-1),
        new Vector3Int(-1,0,-1),
        new Vector3Int(-1,0,0),
    };

    public static List<Vector3Int> directionsOffsetEven = new List<Vector3Int>
    {
        new Vector3Int(0,0,1),
        new Vector3Int(1,0,1),
        new Vector3Int(1,0,0),
        new Vector3Int(1,0,-1),
        new Vector3Int(0,0,-1),
        new Vector3Int(-1,0,0),
    };

    public static List<Vector3Int> GetDirectionList(int z)
        => z % 2 == 0 ? directionsOffsetEven : directionsOffsetOdd;
}