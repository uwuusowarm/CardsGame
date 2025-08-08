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
    private HashSet<Vector3Int> highlightedHexes = new HashSet<Vector3Int>();

    public void Initialize(Unit unit, HexGrid grid)
    {
        selectedUnit = unit;
        hexGrid = grid;
        remainingMovementPoints = unit.MovementPoints;

        if (selectedUnit.currentHex != null)
        {
            currentUnitHex = selectedUnit.currentHex.hexCoords;
            Debug.Log($"Using Unit.currentHex: {currentUnitHex}");
        }
        else
        {
            currentUnitHex = hexGrid.GetClosestHex(selectedUnit.transform.position);
            Debug.Log($"Using calculated hex: {currentUnitHex}");

            Hex hex = hexGrid.GetTileAt(currentUnitHex);
            if (hex != null)
            {
                selectedUnit.currentHex = hex;
                hex.SetUnit(selectedUnit);
            }
        }

        ShowAvailableHexes();
    }

    private void ShowAvailableHexes()
    {
        ClearHighlights();
        highlightedHexes.Clear();

        if (remainingMovementPoints <= 0)
        {
            Debug.Log("No movement points remaining");
            return;
        }

        Debug.Log($"ShowAvailableHexes - Current position: {currentUnitHex}");

        List<Vector3Int> neighbors;
        try
        {
            neighbors = hexGrid.GetNeighborsFor(currentUnitHex);
            Debug.Log($"Found {neighbors.Count} neighbors for {currentUnitHex}");
            Debug.Log($"Neighbors list: {string.Join(", ", neighbors)}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error getting neighbors: {e.Message}");
            return;
        }

        int checkedCount = 0;
        int highlightedCount = 0;

        foreach (Vector3Int neighborPos in neighbors)
        {
            checkedCount++;
            Debug.Log($"[{checkedCount}/{neighbors.Count}] Checking neighbor: {neighborPos}");

            try
            {
                Hex neighborHex = hexGrid.GetTileAt(neighborPos);
                if (neighborHex == null)
                {
                    Debug.Log($"  - No hex found at {neighborPos}");
                    continue;
                }

                if (neighborPos == currentUnitHex)
                {
                    Debug.Log($"  - Skipping current hex {neighborPos}");
                    continue;
                }

                bool isOccupied = neighborHex.IsOccupied();
                bool isObstacle = neighborHex.IsObstacle();
                int hexCost = neighborHex.GetCost();

                Debug.Log($"  - Hex {neighborPos}: Cost={hexCost}, Occupied={isOccupied}, Obstacle={isObstacle}");

                if (!isOccupied && !isObstacle)
                {
                    if (hexCost <= remainingMovementPoints)
                    {
                        neighborHex.EnableHighlight();
                        highlightedHexes.Add(neighborPos);
                        highlightedCount++;
                        Debug.Log($"  - HIGHLIGHTING {neighborPos} (#{highlightedCount})");
                    }
                    else
                    {
                        Debug.Log(
                            $"  - Not highlighting {neighborPos} - cost {hexCost} > remaining {remainingMovementPoints}");
                    }
                }
                else
                {
                    Debug.Log($"  - Not highlighting {neighborPos} - not accessible");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error processing neighbor {neighborPos}: {e.Message}\n{e.StackTrace}");
            }
        }

        Debug.Log($"Finished checking {checkedCount}/{neighbors.Count} neighbors");
        Debug.Log($"Total highlighted hexes: {highlightedHexes.Count} ({highlightedCount})");
        Debug.Log($"Final highlighted list: {string.Join(", ", highlightedHexes)}");
    }

    public void AddToPath(Vector3Int selectedHexPosition)
    {
        Debug.Log($"AddToPath called with: {selectedHexPosition}");
        Debug.Log($"Current unit hex: {currentUnitHex}");

        if (selectedHexPosition == currentUnitHex)
        {
            Debug.Log($"Cannot move to current hex {selectedHexPosition}");
            return;
        }

        if (isMoving || !IsHexInRange(selectedHexPosition))
        {
            Debug.Log($"Cannot move to hex: Moving={isMoving}, InRange={IsHexInRange(selectedHexPosition)}");
            return;
        }

        ClearHighlights();

        Hex selectedHex = hexGrid.GetTileAt(selectedHexPosition);
        int moveCost = selectedHex.GetCost();

        if (moveCost > remainingMovementPoints)
        {
            Debug.LogWarning($"Not enough movement points! Need {moveCost}, have {remainingMovementPoints}");
            return;
        }

        isMoving = true;

        Hex currentHex = hexGrid.GetTileAt(currentUnitHex);
        if (currentHex != null)
        {
            currentHex.ClearUnit();
        }

        Vector3 endWorldPos = selectedHex.transform.position;
        selectedUnit.SetIntendedEndPosition(endWorldPos);
        selectedUnit.MovementFinished += OnMovementFinished;
        selectedUnit.MoveTroughPath(new List<Vector3> { endWorldPos });

        if (animator != null)
        {
            animator.SetBool("IsWalking", true);
        }

        selectedHex.SetUnit(selectedUnit);
        selectedUnit.currentHex = selectedHex;

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

        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
        }

        Debug.Log($"Movement finished. New position: {currentUnitHex}");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.CheckForEnemiesInRange();
        }

        if (remainingMovementPoints > 0)
        {
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
            Hex hex = hexGrid.GetTileAt(pos);
            if (hex != null)
            {
                hex.ResetHighlight();
            }
        }

        currentPath.Clear();
        ClearHighlights();
    }

    private void ClearHighlights()
    {
        if (hexGrid == null) return;

        foreach (Vector3Int hexPosition in highlightedHexes)
        {
            Hex hex = hexGrid.GetTileAt(hexPosition);
            if (hex != null)
            {
                hex.DisableHighlight();
            }
        }

        highlightedHexes.Clear();
    }

    public void HideRange()
    {
        ClearHighlights();
    }

    public bool IsHexInRange(Vector3Int hexPosition)
    {
        Debug.Log(
            $"IsHexInRange called for {hexPosition} - Moving: {isMoving}, MovementPoints: {remainingMovementPoints}");

        if (remainingMovementPoints <= 0)
        {
            Debug.Log($"No movement points remaining");
            return false;
        }

        if (isMoving)
        {
            Debug.Log($"Cannot move - already moving");
            return false;
        }

        if (hexPosition == currentUnitHex)
        {
            Debug.Log($"Cannot move to current hex {hexPosition}");
            return false;
        }

        if (!highlightedHexes.Contains(hexPosition))
        {
            Debug.Log(
                $"Hex {hexPosition} is not highlighted/available. Highlighted hexes: {string.Join(", ", highlightedHexes)}");
            return false;
        }

        Hex hex = hexGrid.GetTileAt(hexPosition);
        if (hex == null)
        {
            Debug.Log($"Hex {hexPosition} not found in grid");
            return false;
        }

        if (hex.IsOccupied() || hex.IsObstacle())
        {
            Debug.Log($"Hex {hexPosition} is occupied or obstacle");
            return false;
        }

        bool canAfford = hex.GetCost() <= remainingMovementPoints;
        Debug.Log(
            $"Can afford hex {hexPosition}: {canAfford} (cost: {hex.GetCost()}, remaining: {remainingMovementPoints})");
        return canAfford;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (selectedUnit != null)
            {
                remainingMovementPoints = 4;
                selectedUnit.SetMovementPoints(remainingMovementPoints);
                ShowAvailableHexes();
                Debug.Log("Test: Movement activated with 4 steps!");
            }
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            Debug.Log($"=== MOVEMENT DEBUG INFO ===");
            Debug.Log($"Current unit hex (MovementSystem): {currentUnitHex}");
            Debug.Log($"Highlighted hexes: {string.Join(", ", highlightedHexes)}");
            Debug.Log($"Remaining movement points: {remainingMovementPoints}");
            Debug.Log($"Is moving: {isMoving}");

            if (selectedUnit != null)
            {
                Debug.Log($"Unit world position: {selectedUnit.transform.position}");
                Debug.Log(
                    $"Unit.currentHex: {(selectedUnit.currentHex != null ? selectedUnit.currentHex.hexCoords.ToString() : "null")}");
            }

            try
            {
                List<Vector3Int> neighbors = hexGrid.GetNeighborsFor(currentUnitHex);
                Debug.Log($"All {neighbors.Count} neighbors:");
                for (int i = 0; i < neighbors.Count; i++)
                {
                    Vector3Int neighbor = neighbors[i];
                    Hex hex = hexGrid.GetTileAt(neighbor);
                    if (hex != null)
                    {
                        Debug.Log(
                            $"  {i + 1}. {neighbor}: Cost={hex.GetCost()}, Occupied={hex.IsOccupied()}, Obstacle={hex.IsObstacle()}, Highlighted={highlightedHexes.Contains(neighbor)}");
                    }
                    else
                    {
                        Debug.Log($"  {i + 1}. {neighbor}: NULL HEX");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error in debug info: {e.Message}");
            }

            Debug.Log($"===========================");
        }
    }
}