using System.Collections.Generic;
using UnityEngine;

public class HexGrid : MonoBehaviour
{
    Dictionary<Vector3Int, Hex> hexTileDict = new Dictionary<Vector3Int, Hex>();
    Dictionary<Vector3Int, List<Vector3Int>> hexTileNeighboursDict = new Dictionary<Vector3Int, List<Vector3Int>>();

    [Header("Grid Dimensions")]
    [Tooltip("The full width of a single hex tile (corner to corner). Can be auto-calculated in the Level Painter.")]
    public float hexWidth = 1.732f;
    [Tooltip("The full height of a single hex tile (flat side to flat side). Can be auto-calculated in the Level Painter.")]
    public float hexHeight = 2f;

    // A getter for the vertical spacing based on height.
    public float ZSpacing => hexHeight * 0.75f;

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
    }

    private void Start()
    {
        Debug.Log("Initializing grid...");
        foreach (Hex hex in FindObjectsOfType<Hex>())
        {
            hexTileDict[hex.hexCoords] = hex;
        }

        foreach (var hexCoords in hexTileDict.Keys)
        {
            GetNeighborsFor(hexCoords);
        }
        Debug.Log($"Grid initialized with {hexTileDict.Count} hexes");
    }

    public Vector3Int GetClosestHex(Vector3 worldPosition)
    {
        // Convert world position to axial coordinates first for accuracy
        float q_axial = (worldPosition.x * Mathf.Sqrt(3) / 3f - worldPosition.z / 3f) / (hexHeight / 2f);
        float r_axial = (worldPosition.z * 2f / 3f) / (hexHeight / 2f);

        // Convert axial to cube coordinates
        Vector3 cube = new Vector3(q_axial, -q_axial - r_axial, r_axial);
        
        // Round cube coordinates to nearest integer
        int rx = Mathf.RoundToInt(cube.x);
        int ry = Mathf.RoundToInt(cube.y);
        int rz = Mathf.RoundToInt(cube.z);

        float dx = Mathf.Abs(rx - cube.x);
        float dy = Mathf.Abs(ry - cube.y);
        float dz = Mathf.Abs(rz - cube.z);

        if (dx > dy && dx > dz)
            rx = -ry - rz;
        else if (dy > dz)
            ry = -rx - rz;
        else
            rz = -rx - ry;

        // Convert final cube coordinates back to offset (odd-q)
        int col = rx;
        int row = rz + (rx - (rx & 1)) / 2;
        
        // The Y coordinate in the Vector3Int is unused for grid positioning, so we return it as the original world Y.
        // Let's stick to the simpler offset math which is what the rest of the system uses.
        // It's less accurate at the seams but easier to reason about.

        int z = Mathf.RoundToInt(worldPosition.z / ZSpacing);
        float xOffset = (z % 2 != 0) ? hexWidth / 2f : 0;
        int x = Mathf.RoundToInt((worldPosition.x - xOffset) / hexWidth);

        return new Vector3Int(x, 0, z);
    }
    
    public Vector3 GetWorldPosition(Vector3Int hexCoordinates)
    {
        float x = hexCoordinates.x * hexWidth;
        float z = hexCoordinates.z * ZSpacing;

        // Stagger odd rows (this is "odd-r" or "odd-row" layout)
        if (hexCoordinates.z % 2 != 0)
        {
            x += hexWidth / 2f;
        }
        return new Vector3(x, 0, z);
    }

    public Hex GetTileAt(Vector3Int hexCoordinates)
    {
        hexTileDict.TryGetValue(hexCoordinates, out Hex result);
        return result;
    }

    public List<Vector3Int> GetNeighborsFor(Vector3Int hexCoordinates)
    {
        if (!hexTileDict.ContainsKey(hexCoordinates))
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
    
    // Unchanged methods...
    public List<Vector3Int> GetNeighborsFor(Vector3Int hexCoordinates, int range = 1) { /* ... */ return new List<Vector3Int>(); }
    public void AddMovementPoints(int points) { /* ... */ }
}


public static class Direction
{
    // --- REVISED DIRECTION VECTORS for pointy-topped "odd-r" layout ---
    // odd rows are shifted to the right
    public static List<Vector3Int> directionsOffsetOdd = new List<Vector3Int>
    {
        new Vector3Int(1, 0, 0),    // E
        new Vector3Int(0, 0, -1),   // SW
        new Vector3Int(-1, 0, -1),  // NW
        new Vector3Int(-1, 0, 0),   // W
        new Vector3Int(-1, 0, 1),   // NE
        new Vector3Int(0, 0, 1)     // SE
    };

    // even rows are not shifted
    public static List<Vector3Int> directionsOffsetEven = new List<Vector3Int>
    {
        new Vector3Int(1, 0, 0),    // E
        new Vector3Int(1, 0, -1),   // SW
        new Vector3Int(0, 0, -1),   // NW
        new Vector3Int(-1, 0, 0),   // W
        new Vector3Int(0, 0, 1),    // NE
        new Vector3Int(1, 0, 1)     // SE
    };

    public static List<Vector3Int> GetDirectionList(int z)
        => z % 2 != 0 ? directionsOffsetOdd : directionsOffsetEven;
}