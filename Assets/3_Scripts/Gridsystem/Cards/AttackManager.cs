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

        if (UnitManager.Instance.SelectedUnit == null)
        {
            Debug.LogError(
                "AttackManager: PrepareAttack - UnitManager.Instance.SelectedUnit (Player Unit) is NULL. Cannot proceed.");
            Debug.LogWarning(
                "Possible causes: Player unit destroyed? SelectedUnit in UnitManager incorrectly set to null after previous action?");
            Debug.Log(
                $"PrepareAttack called by card for damage: {damage}, range: {range}. StackTrace: {StackTraceUtility.ExtractStackTrace()}");
        }

        currentAttackDamage = damage;
        currentAttackRange = range;
        HighlightEnemiesInRange();
    }

    public List<EnemyUnit> GetEnemiesInRange(Vector3Int centerHex, int range)
    {
        List<EnemyUnit> enemiesInRange = new List<EnemyUnit>();

        Hex centerHexTile = HexGrid.Instance.GetTileAt(centerHex);
        if (centerHexTile == null)
        {
            Debug.LogError($"No hex found at position {centerHex}");
            return enemiesInRange;
        }

        int maxDistance = range + 1;

        HashSet<Vector3Int> hexesInRange = new HashSet<Vector3Int>();
        Queue<Vector3Int> queue = new Queue<Vector3Int>();
        Dictionary<Vector3Int, int> distances = new Dictionary<Vector3Int, int>();

        queue.Enqueue(centerHex);
        distances[centerHex] = 0;

        while (queue.Count > 0)
        {
            Vector3Int current = queue.Dequeue();
            int currentDistance = distances[current];

            if (currentDistance < maxDistance)
            {
                foreach (Vector3Int neighbor in HexGrid.Instance.GetNeighborsFor(current))
                {
                    if (!distances.ContainsKey(neighbor))
                    {
                        distances[neighbor] = currentDistance + 1;
                        queue.Enqueue(neighbor);

                        if (neighbor != centerHex)
                        {
                            hexesInRange.Add(neighbor);
                        }
                    }
                }
            }
        }

        foreach (Vector3Int hexCoord in hexesInRange)
        {
            Hex hex = HexGrid.Instance.GetTileAt(hexCoord);
            if (hex != null && hex.HasEnemyUnit())
            {
                EnemyUnit enemy = hex.EnemyUnitOnHex;
                if (enemy != null && enemy.gameObject.activeInHierarchy)
                {
                    enemiesInRange.Add(enemy);
                }
            }
        }

        return enemiesInRange;
    }

    private void HighlightEnemiesInRange()
    {
        ClearHighlights();

        Vector3Int playerHexCoords;
        if (UnitManager.Instance.SelectedUnit.currentHex != null)
        {
            playerHexCoords = UnitManager.Instance.SelectedUnit.currentHex.hexCoords;
            Debug.Log($"Using Unit.currentHex: {playerHexCoords}");
        }
        else
        {
            Debug.LogError("AttackManager: SelectedUnit.currentHex is null! Cannot determine attack position.");
            return;
        }

        Debug.Log($"=== ATTACK RANGE DEBUG ===");
        Debug.Log($"Player at hex: {playerHexCoords}");
        Debug.Log($"Attack damage: {currentAttackDamage}");
        Debug.Log($"Attack range: {currentAttackRange}");
        Debug.Log($"============================");

        Hex playerHex = HexGrid.Instance.GetTileAt(playerHexCoords);
        if (playerHex == null)
        {
            Debug.LogError($"No hex found at player position {playerHexCoords}");
            return;
        }

        int maxDistance = currentAttackRange + 1;

        HashSet<Vector3Int> hexesInRange = new HashSet<Vector3Int>();
        Queue<Vector3Int> queue = new Queue<Vector3Int>();
        Dictionary<Vector3Int, int> distances = new Dictionary<Vector3Int, int>();

        queue.Enqueue(playerHexCoords);
        distances[playerHexCoords] = 0;

        while (queue.Count > 0)
        {
            Vector3Int current = queue.Dequeue();
            int currentDistance = distances[current];

            if (currentDistance < maxDistance)
            {
                foreach (Vector3Int neighbor in HexGrid.Instance.GetNeighborsFor(current))
                {
                    if (!distances.ContainsKey(neighbor))
                    {
                        distances[neighbor] = currentDistance + 1;
                        queue.Enqueue(neighbor);

                        if (neighbor != playerHexCoords)
                        {
                            hexesInRange.Add(neighbor);
                        }
                    }
                }
            }
        }

        bool enemiesFound = false;
        int enemiesInRange = 0;

        foreach (Vector3Int hexCoord in hexesInRange)
        {
            Hex hex = HexGrid.Instance.GetTileAt(hexCoord);
            if (hex != null && hex.HasEnemyUnit())
            {
                EnemyUnit enemy = hex.EnemyUnitOnHex;
                if (enemy != null && enemy.gameObject.activeInHierarchy)
                {
                    int distance = distances[hexCoord];
                    Debug.Log($"Enemy '{enemy.name}' at {hexCoord} - Distance: {distance} - HIGHLIGHTED");

                    enemy.ToggleHighlight(true);
                    highlightedEnemies.Add(enemy);
                    enemiesFound = true;
                    enemiesInRange++;
                }
            }
        }

        Debug.Log($"Found {enemiesInRange} enemies in range {currentAttackRange}");

        if (!enemiesFound)
        {
            Debug.LogWarning("No enemies found in attack range!");
        }
    }

    private int HexDistance(Vector3Int a, Vector3Int b)
    {
        return (Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) + Mathf.Abs(a.z - b.z)) / 2;
    }

    public HashSet<Vector3Int> GetHexesInRange(Vector3Int center, int range)
    {
        HashSet<Vector3Int> result = new HashSet<Vector3Int>();
        Queue<Vector3Int> queue = new Queue<Vector3Int>();
        Dictionary<Vector3Int, int> distances = new Dictionary<Vector3Int, int>();

        queue.Enqueue(center);
        distances[center] = 0;

        while (queue.Count > 0)
        {
            Vector3Int current = queue.Dequeue();
            int currentDist = distances[current];

            if (currentDist >= range) continue;

            foreach (Vector3Int neighbor in HexGrid.Instance.GetNeighborsFor(current))
            {
                if (!distances.ContainsKey(neighbor))
                {
                    distances[neighbor] = currentDist + 1;
                    queue.Enqueue(neighbor);
                    result.Add(neighbor);
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

            if (GameManager.Instance.IsPoisonAttackActive())
            {
                int poisonDuration = GameManager.Instance.GetPendingPoisonDuration();
                enemy.ApplyPoison(poisonDuration);
                Debug.Log($"Applied poison to {enemy.name} for {poisonDuration} turns from poisoned attack");
                GameManager.Instance.ClearPoisonAttack();
            }

            if (GameManager.Instance.IsStunAttackActive())
            {
                int stunDuration = GameManager.Instance.GetPendingStunDuration();
                enemy.ApplyStun(stunDuration);
                Debug.Log($"Stun effect applied to {enemy.name} for {stunDuration} turns");
                GameManager.Instance.ClearStunAttack();
            }

            ClearHighlights();
            GameManager.Instance.ResetAttackAvailability();
            GameManager.Instance.PlayerActionResolved(true);
        }
    }

    public void ResetAttackValues()
    {
        currentAttackDamage = 0;
        currentAttackRange = 0;
        ClearHighlights();
        Debug.Log("AttackManager: Attack values reset to 0");
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