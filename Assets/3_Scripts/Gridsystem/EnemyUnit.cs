using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class EnemyUnit : Unit  
{
    [Header("Health")]
    public int maxHealth = 3;
    private int currentHealth;

    [Header("Visuals")]
    public Material normalMaterial;
    public Material highlightMaterial;
    private Renderer rend;
    private bool isHighlighted = false;

    private Unit playerUnit;
    private void Start()
    {
        GetComponentInChildren<Renderer>().material.color = Color.red;
        StartCoroutine(InitializeHexPosition());
        playerUnit = FindObjectOfType<Unit>();
        currentHealth = maxHealth;
        rend = GetComponentInChildren<Renderer>();
        rend.material = normalMaterial;
    }
    public void MoveTowardsPlayer()
    {
        if (playerUnit == null)
        {
            return;
        }

        if (currentHex == null)
        {
            return;
        }


        Vector3Int playerHexCoords = HexGrid.Instance.GetClosestHex(playerUnit.transform.position);

        Vector3Int direction = FindBestDirection(playerHexCoords, currentHex.hexCoords);

        Vector3Int targetHexCoords = currentHex.hexCoords + direction;
        Hex targetHex = HexGrid.Instance.GetTileAt(targetHexCoords);

        if (targetHex == null)
        {
            return;
        }

        if (targetHex.IsOccupied())
        {
            return;
        }
        List<Vector3> path = new List<Vector3> { targetHex.transform.position };
        MoveTroughPath(path);
    }

    private Vector3Int FindBestDirection(Vector3Int playerPos, Vector3Int enemyPos)
    {
        List<Vector3Int> possibleDirections = Direction.GetDirectionList(enemyPos.z);
        Vector3Int bestDirection = Vector3Int.zero;
        float shortestDistance = float.MaxValue;

        foreach (Vector3Int dir in possibleDirections)
        {
            Vector3Int neighborPos = enemyPos + dir;
            float distance = Vector3Int.Distance(neighborPos, playerPos);

            if (distance < shortestDistance)
            {
                Hex neighborHex = HexGrid.Instance.GetTileAt(neighborPos);
                if (neighborHex != null && !neighborHex.IsOccupied())
                {
                    shortestDistance = distance;
                    bestDirection = dir;
                }
            }
        }
        return bestDirection;
    }
    public void ToggleHighlight()
    {
        isHighlighted = !isHighlighted;
        rend.material = isHighlighted ? highlightMaterial : normalMaterial;
    }

    private void OnMouseDown()
    {
        if (isHighlighted)
        {
            TakeDamage(1);
            ToggleHighlight();
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log($"Gegner getroffen! Verbleibende HP: {currentHealth}");

        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }
}
