using UnityEngine;

public class EnemyPositionChecker : MonoBehaviour
{
    [SerializeField] private GameObject enemyObject;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CheckEnemyPosition();
        }
    }

    public void CheckEnemyPosition()
    {
        if (enemyObject == null)
        {
            enemyObject = GameObject.FindGameObjectWithTag("Enemy");
            if (enemyObject == null)
            {
                Debug.LogError("No object with tag 'Enemy' found in scene!");
                return;
            }
        }
        Vector3 enemyWorldPos = enemyObject.transform.position;
        Debug.Log($"Enemy world position: {enemyWorldPos}");
        Vector3Int enemyHexCoords = HexGrid.Instance.GetClosestHex(enemyWorldPos);
        Debug.Log($"Enemy hex coordinates: {enemyHexCoords}");
        Hex enemyHex = HexGrid.Instance.GetTileAt(enemyHexCoords);
        if (enemyHex == null)
        {
            Debug.LogError($"No hex found at coordinates {enemyHexCoords}!");
            return;
        }
        Debug.Log($"Hex at {enemyHexCoords} has unit: {enemyHex.UnitOnHex != null}");
        Vector3Int playerHexCoords = HexGrid.Instance.GetClosestHex(
            UnitManager.Instance.SelectedUnit.transform.position
        );
        int distance = HexDistance(playerHexCoords, enemyHexCoords);
        Debug.Log($"Distance to player: {distance} hexes");
        Debug.DrawLine(
            HexGrid.Instance.GetTileAt(playerHexCoords).transform.position,
            enemyHex.transform.position,
            Color.red,
            5f
        );
    }

    private int HexDistance(Vector3Int a, Vector3Int b)
    {
        return (Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) + Mathf.Abs(a.z - b.z)) / 2;
    }
}