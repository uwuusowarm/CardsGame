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

    public void PrepareAttack(int damage, int range)
    {
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
        HashSet<Vector3Int> hexesInRange = new HashSet<Vector3Int>();
        Queue<Vector3Int> queue = new Queue<Vector3Int>();
        Dictionary<Vector3Int, int> distances = new Dictionary<Vector3Int, int>();

        queue.Enqueue(playerHexCoords);
        distances[playerHexCoords] = 0;

        while (queue.Count > 0)
        {
            Vector3Int current = queue.Dequeue();
            int currentDist = distances[current];

            if (currentDist > currentAttackRange) continue;

            foreach (Vector3Int neighbor in HexGrid.Instance.GetNeighborsFor(current))
            {
                if (!distances.ContainsKey(neighbor))
                {
                    distances[neighbor] = currentDist + 1;
                    queue.Enqueue(neighbor);
                    if (currentDist + 1 <= currentAttackRange)
                    {
                        hexesInRange.Add(neighbor);
                    }
                }
            }
        }
        bool enemiesFound = false;
        foreach (Vector3Int hexCoord in hexesInRange)
        {
            Hex hex = HexGrid.Instance.GetTileAt(hexCoord);
            if (hex != null && hex.UnitOnHex != null)
            {
                EnemyUnit enemy = hex.UnitOnHex.GetComponent<EnemyUnit>();
                if (enemy != null)
                {
                    enemiesFound = true;
                    enemy.ToggleHighlight(true);
                    highlightedEnemies.Add(enemy);
                    Debug.Log($"Enemy found at {hexCoord} (Distance: {distances[hexCoord]})");
                }
            }
        }

        if (!enemiesFound)
        {
            Debug.LogError($"No enemies found in range {currentAttackRange}. Check:");
            Debug.LogError($"- Player position: {playerHexCoords}");
            Debug.LogError($"- Hexes checked: {hexesInRange.Count}");
            Debug.LogError($"- Grid contains player hex: {HexGrid.Instance.GetTileAt(playerHexCoords) != null}");
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
        if (highlightedEnemies.Contains(enemy))
        {
            enemy.TakeDamage(currentAttackDamage);
            ClearHighlights();
            UnitManager.Instance.EndPlayerTurn();
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