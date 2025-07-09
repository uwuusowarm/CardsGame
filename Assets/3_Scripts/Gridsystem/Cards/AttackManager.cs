using System.Collections.Generic;
using UnityEngine;

public class AttackManager : MonoBehaviour
{
    public static AttackManager Instance;
    public LayerMask enemyLayer;

    private int currentAttackDamage;
    private int currentAttackRange;
    private List<EnemyUnit> highlightedEnemies = new List<EnemyUnit>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void TryPrepareAttack()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsAttackAvailable())
        {
            HighlightEnemiesInRange();
        }
    }

    public void PrepareAttack(int damage, int range)
    {
        if (UnitManager.Instance == null)
        {
            Debug.LogError("AttackManager: PrepareAttack - UnitManager.Instance is NULL. Cannot proceed.");
            ReturnCardToHand();
            ClearHighlights(); 
            return;
        }

        currentAttackDamage = damage;
        currentAttackRange = range;
        HighlightEnemiesInRange();
    }

    private void HighlightEnemiesInRange()
    {
        ClearHighlights();
        Vector3Int playerHexCoords = HexGrid.Instance.GetClosestHex(
            UnitManager.Instance.SelectedUnit.transform.position
        );
        Hex playerHex = HexGrid.Instance.GetTileAt(playerHexCoords);

        if (playerHex == null)
        {
            Debug.LogError("Player not on a valid hex!");
            return;
        }
        HashSet<Vector3Int> hexesInRange = GetHexesInRange(playerHexCoords, currentAttackRange);
    
        bool enemiesFound = false;
        foreach (Vector3Int hexCoord in hexesInRange)
        {
            Hex hex = HexGrid.Instance.GetTileAt(hexCoord);
        
            if (hex != null && hex.EnemyUnitOnHex != null)
            {
                EnemyUnit enemy = hex.EnemyUnitOnHex.GetComponent<EnemyUnit>();
                if (enemy != null)
                {
                    enemiesFound = true;
                    enemy.ToggleHighlight(true);
                    highlightedEnemies.Add(enemy);
                    Debug.Log($"Enemy found in range at {hexCoord}");
                }
            }
        }

        if (!enemiesFound)
        {
            Debug.LogError($"No enemies found in range {currentAttackRange}.");
            ReturnCardToHand();
        }
    }


    private int HexDistance(Vector3Int a, Vector3Int b)
    {
        return (Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) + Mathf.Abs(a.z - b.z)) / 2;
    }

    private HashSet<Vector3Int> GetHexesInRange(Vector3Int center, int range)
    {
        HashSet<Vector3Int> result = new HashSet<Vector3Int>();
    
        result.Add(center);
    
        for (int q = -range; q <= range; q++)
        {
            for (int r = Mathf.Max(-range, -q-range); r <= Mathf.Min(range, -q+range); r++)
            {
                int s = -q - r;
                Vector3Int hex = new Vector3Int(
                    center.x + q,
                    center.y + r,
                    center.z + s
                );
            
                if (HexGrid.Instance.GetTileAt(hex) != null)
                {
                    result.Add(hex);
                }
            }
        }
    
        return result;
    }
    
    public void HandleEnemyClick(EnemyUnit enemy)
    {
        if (!GameManager.Instance.IsAttackAvailable())
        {
            return;
        }

        if (highlightedEnemies.Contains(enemy))
        {
            enemy.TakeDamage(currentAttackDamage);
            ClearHighlights();
            GameManager.Instance.PlayerActionResolved(true);
        }
    }

    public void ClearHighlights()
    {
        foreach (EnemyUnit enemy in highlightedEnemies)
        {
            if (enemy != null)
            {
                enemy.ToggleHighlight(false);
            }
        }
        highlightedEnemies.Clear();
    }

    private void ReturnCardToHand()
    {
        Debug.Log("Returning card to hand");
    }
}