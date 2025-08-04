
using System.Collections.Generic;
using UnityEngine;

public class MovementSystem : MonoBehaviour
{
    private List<Vector3Int> currentPath = new List<Vector3Int>();
    private int remainingMovementPoints;
    private Unit selectedUnit;
    private HexGrid hexGrid;
    public Animator animator;
    private Vector3Int currentUnitHex;
    private bool isMoving = false;

    public void Initialize(Unit unit, HexGrid grid)
    {
        selectedUnit = unit;
        hexGrid = grid;
        remainingMovementPoints = unit.MovementPoints;
        currentUnitHex = hexGrid.GetClosestHex(selectedUnit.transform.position);
        ShowAvailableHexes();
    }

    private void ShowAvailableHexes()
    {
        ClearHighlights();

        if (remainingMovementPoints <= 0) return;

        List<Vector3Int> neighbors = hexGrid.GetNeighborsFor(currentUnitHex);
        foreach (Vector3Int hexPosition in neighbors)
        {
            Hex hex = hexGrid.GetTileAt(hexPosition);
            if (hex != null && !hex.IsOccupied() && hex.GetCost() <= remainingMovementPoints)
            {
                hex.EnableHighlight();
            }
        }
    }

    public void AddToPath(Vector3Int selectedHexPosition)
    {
        if (isMoving || !IsHexInRange(selectedHexPosition)) 
        {
            Debug.Log($"Cannot move to hex: Moving={isMoving}, InRange={IsHexInRange(selectedHexPosition)}");
            return;
        }
        
        ClearHighlights();
        
        Hex selectedHex = hexGrid.GetTileAt(selectedHexPosition);
        int moveCost = selectedHex.GetCost();

        isMoving = true;

        Vector3 endWorldPos = selectedHex.transform.position;
        selectedUnit.SetIntendedEndPosition(endWorldPos);
        selectedUnit.MovementFinished += OnMovementFinished;
        selectedUnit.MoveTroughPath(new List<Vector3> { endWorldPos });

        Hex currentHex = hexGrid.GetTileAt(currentUnitHex);
        if (currentHex != null)
        {
            currentHex.ClearUnit();
        }
        selectedHex.SetUnit(selectedUnit);

        if (selectedUnit.TryGetComponent<HexCoordinates>(out var hexCoordinates))
        {
            hexCoordinates.UpdateHexCoords(selectedHexPosition);
        }

        remainingMovementPoints -= moveCost;
        currentUnitHex = selectedHexPosition;

        if (PlayerStatusUI.Instance != null)
        {
            PlayerStatusUI.Instance.UpdateMovementPoints(remainingMovementPoints);
        }
        
        selectedUnit.SetMovementPoints(remainingMovementPoints);
        
        Debug.Log($"Moving to {selectedHexPosition}, Cost: {moveCost}, Remaining Points: {remainingMovementPoints}");
    }


    private void OnMovementFinished(Unit unit)
    {
        unit.MovementFinished -= OnMovementFinished;
        isMoving = false;

        if (remainingMovementPoints > 0)
        {
            ClearHighlights();
            ShowAvailableHexes();
        }
        else
        {
            ClearHighlights();
        }
    }

    public void ClearPath()
    {
        foreach (Vector3Int pos in currentPath)
        {
            hexGrid.GetTileAt(pos).ResetHighlight();
        }
        currentPath.Clear();
        ClearHighlights();
    }

    private void ClearHighlights()
    {
        if (hexGrid == null) return;

        List<Vector3Int> neighbors = hexGrid.GetNeighborsFor(currentUnitHex);
        foreach (Vector3Int hexPosition in neighbors)
        {
            Hex hex = hexGrid.GetTileAt(hexPosition);
            if (hex != null)
            {
                hex.DisableHighlight();
            }
        }
    }

    public void HideRange()
    {
        ClearHighlights();
    }

    public bool IsHexInRange(Vector3Int hexPosition)
    {
        if (remainingMovementPoints <= 0) return false;

        List<Vector3Int> neighbors = hexGrid.GetNeighborsFor(currentUnitHex);
        if (!neighbors.Contains(hexPosition))
            return false;

        Hex hex = hexGrid.GetTileAt(hexPosition);
        return hex != null && !hex.IsOccupied() && hex.GetCost() <= remainingMovementPoints;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (selectedUnit != null)
            {
                remainingMovementPoints = 4;
                ShowAvailableHexes();
                Debug.Log("Test: Movement activated with 4 steps!");
            }
        }
    }
}