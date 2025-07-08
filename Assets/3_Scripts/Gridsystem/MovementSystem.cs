using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MovementSystem : MonoBehaviour
{
    private BFSResult movementRange = new BFSResult(new Dictionary<Vector3Int, Vector3Int?>());
    private List<Vector3Int> currentPath = new List<Vector3Int>();
    private int remainingMovementPoints;
    private Unit selectedUnit;
    private HexGrid hexGrid;
    public Animator animator;
    private Vector3Int lastUnitHex;

    public void Initialize(Unit unit, HexGrid grid, int movementPointsFromCache)
    {
        selectedUnit = unit;
        hexGrid = grid;
        remainingMovementPoints = movementPointsFromCache; 
        lastUnitHex = hexGrid.GetClosestHex(selectedUnit.transform.position);
        CalculateRange();
    }

    private void CalculateRange()
    {
        movementRange = GraphSearch.BFSGetRange(hexGrid, lastUnitHex, remainingMovementPoints);
        Debug.Log("Range: " + string.Join(", ", movementRange.GetRangePositions()));
        foreach (Vector3Int hexPosition in movementRange.GetRangePositions())
        {
            if (hexPosition != lastUnitHex)
                hexGrid.GetTileAt(hexPosition).EnableHighlight();
        }
    }

    public void AddToPath(Vector3Int selectedHexPosition)
    {
        Debug.Log("AddToPath: " + selectedHexPosition);

        if (!IsHexInRange(selectedHexPosition))
        {
            Debug.Log("Nicht in Range!");
            return;
        }

        Hex selectedHex = hexGrid.GetTileAt(selectedHexPosition);
        if (selectedHex == null)
        {
            Debug.Log("SelectedHex ist null!");
            return;
        }
        if (selectedHex.IsOccupied())
        {
            Debug.Log("SelectedHex ist besetzt!");
            return;
        }

        int moveCost = selectedHex.GetCost();
        
        int movementUsed = moveCost > 0 ? moveCost : 1;
        
        Debug.Log($"MoveCost: {moveCost}, Remaining: {remainingMovementPoints}");
        
        
        if (moveCost > remainingMovementPoints)
        {
            Debug.Log("Nicht genug Bewegungspunkte!");
            return;
        }

        PlayedCardEffectCache.Instance.UseMovement(movementUsed);
    
        currentPath.Clear();
        currentPath.Add(selectedHexPosition);

        remainingMovementPoints = PlayedCardEffectCache.Instance.PendingMovement; 
        selectedHex.HighLightPath();

        Debug.Log("ConfirmPath wird aufgerufen");
        ConfirmPath();
    }

    private void UpdateMovementRange()
    {
        foreach (Vector3Int pos in movementRange.GetRangePositions())
        {
            hexGrid.GetTileAt(pos).DisableHighlight();
        }

        Vector3Int startPoint = currentPath.Count > 0 ?
            currentPath[currentPath.Count - 1] :
            hexGrid.GetClosestHex(selectedUnit.transform.position);

        movementRange = GraphSearch.BFSGetRange(hexGrid, startPoint, remainingMovementPoints);
        foreach (Vector3Int pos in movementRange.GetRangePositions())
        {
            if (!currentPath.Contains(pos))
                hexGrid.GetTileAt(pos).EnableHighlight();
        }
    }

    public void ConfirmPath()
    {
        if (currentPath.Count == 0) return;

        Vector3Int endHexPos = currentPath[0];
        Hex endHex = hexGrid.GetTileAt(endHexPos);
        if (endHex == null || endHex.IsOccupied())
        {
            Debug.LogWarning("Error MovementSystem: End hex is null or occupied!");
            ClearPath();
            return;
        }

        Vector3 endWorldPos = endHex.transform.position;
        selectedUnit.SetIntendedEndPosition(endWorldPos);
        selectedUnit.MoveTroughPath(new List<Vector3> { endWorldPos });
        lastUnitHex = endHexPos;
    
        selectedUnit.movementPoints = remainingMovementPoints;
        PlayerStatsUI.Instance.UpdateMovementPoints(remainingMovementPoints);
    
        ClearPath();
    }

    public void ClearPath()
    {
        foreach (Vector3Int pos in currentPath)
        {
            hexGrid.GetTileAt(pos).ResetHighlight();
        }
        currentPath.Clear();
        foreach (Vector3Int pos in movementRange.GetRangePositions())
        {
            hexGrid.GetTileAt(pos).DisableHighlight();
        }
    }

    public bool IsHexInRange(Vector3Int hexPosition)
    {
        return movementRange.IsHexPositionInRange(hexPosition);
    }

    public bool IsPositionInPath(Vector3Int position)
    {
        return currentPath.Contains(position);
    }

    public void HideRange()
    {
        if (hexGrid == null) return;

        foreach (Vector3Int hexPosition in movementRange.GetRangePositions())
        {
            Hex hex = hexGrid.GetTileAt(hexPosition);
            if (hex != null)
            {
                hex.DisableHighlight();
            }
        }
    }

    public void ShowPath(Vector3Int selectedHexPosition)
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

    public void MoveUnit()
    {
        if (currentPath.Count == 0) return;
        selectedUnit.MoveTroughPath(currentPath.Select(pos => hexGrid.GetTileAt(pos).transform.position).ToList());
        ClearPath();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (selectedUnit != null)
            {
                remainingMovementPoints = 4;
                CalculateRange();
                Debug.Log("Test: Movement aktiviert mit 4 Schritten!");
            }
        }
    }
}