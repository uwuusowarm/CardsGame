using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MovementSystem : MonoBehaviour
{
    private BFSResult movementRange = new BFSResult(new Dictionary<Vector3Int, Vector3Int?>());
    private List<Vector3Int> currentPath = new List<Vector3Int>();

    public void HideRange(HexGrid hexGrid)
    {
        if (movementRange.visitedNodesDict == null) return;

        foreach (Vector3Int hexPosition in movementRange.GetRangePositions())
        {
            Hex hex = hexGrid.GetTileAt(hexPosition);
            if (hex != null)
            {
                hex.DisableHighlight();
            }
        }
    }

    public void ShowRange(Unit selectedUnit, HexGrid hexGrid)
    {
        CalcualteRange(selectedUnit, hexGrid);

        foreach (Vector3Int hexPosition in movementRange.GetRangePositions())
        {
            hexGrid.GetTileAt(hexPosition).EnableHighlight();
        }
    }
    public void AutoShowRange(Unit unit, HexGrid grid)
    {
        CalcualteRange(unit, grid);
        foreach (Vector3Int hexPosition in movementRange.GetRangePositions())
        {
            grid.GetTileAt(hexPosition).EnableHighlight();
        }
    }

    public void CalcualteRange(Unit selectedUnit, HexGrid hexGrid)
    {
        movementRange = GraphSearch.BFSGetRange(hexGrid, hexGrid.GetClosestHex(selectedUnit.transform.position), selectedUnit.MovementPoints);
    }

    public void ShowPath(Vector3Int selectedHexPosition, HexGrid hexGrid)
    {
        if (movementRange.GetRangePositions().Contains(selectedHexPosition))
        {
            foreach (Vector3Int hexPosition in currentPath)
            {
                hexGrid.GetTileAt(hexPosition).ResetHighlight();
            }
            currentPath = movementRange.GetPathTo(selectedHexPosition);
            foreach (Vector3Int hexPosition in currentPath)
            {
                hexGrid.GetTileAt(hexPosition).HighLightPath();
            }
        }
    }

    public void MoveUnit(Unit selectedUnit, HexGrid hexGrid)
    {
        Debug.Log("Moving unit" + selectedUnit.name);
        selectedUnit.MoveTroughPath(currentPath.Select(pos => hexGrid.GetTileAt(pos).transform.position).ToList());
    }

    public bool IsHexInRange(Vector3Int hexPosition)
    {
        return movementRange.IsHexPositionInRange(hexPosition);
    }
}
