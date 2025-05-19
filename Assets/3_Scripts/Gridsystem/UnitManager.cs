using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HexGrid hexGrid;
    [SerializeField] private MovementSystem movementSystem;
    [SerializeField] private List<EnemyUnit> enemyUnits = new List<EnemyUnit>();

    public static UnitManager Instance { get; private set; }
    public bool PlayersTurn { get; private set; } = true;

    [SerializeField] private Unit selectedUnit;
    private Hex previouslySelectedHex;
    public Unit SelectedUnit => selectedUnit;
    private bool isFirstTurn = true;

    void Awake()
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

    public void HandleUnitSelected(GameObject unit)
    {
        if (!PlayersTurn) return;

        if (!unit.TryGetComponent<Unit>(out var unitReference)) return;

        if (CheckIfTheSameUnitSelected(unitReference)) return;

        PrepareUnitForMovement(unitReference);
    }

    private bool CheckIfTheSameUnitSelected(Unit unitReference)
    {
        if (selectedUnit == unitReference)
        {
            ClearOldSelection();
            return true;
        }
        return false;
    }

    public void HandleTerrainSelected(GameObject hexGO)
    {
        if (selectedUnit == null || !PlayersTurn) return;

        if (!hexGO.TryGetComponent<Hex>(out var selectedHex)) return;

        if (HandleHexOutOfRange(selectedHex.HexCoords) ||
            HandleSelectedHexIsUnitHex(selectedHex.HexCoords)) return;

        HandleTargetHexSelected(selectedHex);
    }

    public void PrepareUnitForMovement(Unit unitReference)
    {
        selectedUnit?.Deselect();

        selectedUnit = unitReference;
        selectedUnit.Select();
        movementSystem.ShowRange(selectedUnit, hexGrid);
    }

    public void ClearOldSelection()
    {
        previouslySelectedHex = null;
        selectedUnit?.Deselect();
        movementSystem.HideRange(hexGrid);
        selectedUnit = null;
        AttackManager.Instance?.ClearHighlights();
    }

    private void HandleTargetHexSelected(Hex selectedHex)
    {
        if (previouslySelectedHex == null || previouslySelectedHex != selectedHex)
        {
            previouslySelectedHex = selectedHex;
            movementSystem.ShowPath(selectedHex.HexCoords, hexGrid);
        }
        else
        {
            movementSystem.MoveUnit(selectedUnit, hexGrid);
            PlayersTurn = false;
            selectedUnit.MovementFinished += OnMovementFinished;
            ClearOldSelection();
        }
    }

    private void OnMovementFinished(Unit unit)
    {
        unit.MovementFinished -= OnMovementFinished;
        EndPlayerTurn();
    }

    private bool HandleSelectedHexIsUnitHex(Vector3Int hexPosition)
    {
        if (hexPosition == hexGrid.GetClosestHex(selectedUnit.transform.position))
        {
            ClearOldSelection();
            return true;
        }
        return false;
    }

    private bool HandleHexOutOfRange(Vector3Int hexPosition)
    {
        if (!movementSystem.IsHexInRange(hexPosition))
        {
            Debug.Log("Hex out of range!");
            return true;
        }
        return false;
    }

    public void EndPlayerTurn()
    {
        PlayersTurn = false;
        ClearOldSelection();
        if (CardManager.Instance != null)
        {
            CardManager.Instance.UpdateDiscardUI();
        }

        StartCoroutine(EnemyTurnRoutine());
    }

    private IEnumerator EnemyTurnRoutine()
    {
        foreach (var enemy in enemyUnits)
        {
            if (enemy != null)
            {
                //enemy.MoveTowardsPlayer();
                yield return new WaitForSeconds(0.5f);
            }
        }
        StartPlayerTurn();
    }

    public void StartPlayerTurn()
    {
        PlayersTurn = true;

        if (!isFirstTurn)
        {
            CardManager.Instance.DrawCard(2);
        }
        isFirstTurn = false;
    }

    public void RegisterEnemy(EnemyUnit enemy)
    {
        if (!enemyUnits.Contains(enemy))
        {
            enemyUnits.Add(enemy);
        }
    }

    public void UnregisterEnemy(EnemyUnit enemy)
    {
        enemyUnits.Remove(enemy);
    }

    public void ActivateMovement()
    {
        if (!PlayersTurn) return;
        Unit[] playerUnits = FindObjectsByType<Unit>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                            .Where(unit => unit.GetComponent<EnemyUnit>() == null) 
                            .ToArray();

        if (playerUnits.Length > 0)
        {
            PrepareUnitForMovement(playerUnits[0]); 
        }
        else
        {
            Debug.LogWarning("Keine Spieler-Unit (ohne EnemyUnit-Skript) gefunden!");
        }
    }
}