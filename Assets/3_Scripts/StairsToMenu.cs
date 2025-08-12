using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StairsToMenu : MonoBehaviour
{
    private Hex myHex;
    private bool isInitialized = false;
    
    public static StairsToMenu Instance { get; private set; }

    public int scene;

    private IEnumerator Start()
    {
        Instance = this;

        yield return new WaitUntil(() => HexGrid.Instance != null && HexGrid.Instance.GetAllHexes().Count > 0);

        myHex = GetHexBelow();

        if (myHex != null)
        {
            isInitialized = true;
        }
        else
        {
            enabled = false;
        }
    }

    public void OnStairsClicked()
    {
        Debug.Log("StairsToMenu.OnStairsClicked() called!");

            Debug.Log("Player completed the level. Loading scene: " + scene);
            PlayerDataManager.Instance.SavePlayerState();

            NextLevel();
    }

    public void NextLevel()
    {
        SceneManager.LoadScene(scene);
    }

    private bool IsPlayerAdjacent()
    {
        Debug.Log("=== STAIRS ADJACENCY DEBUG ===");
        
        if (myHex == null)
        {
            Debug.LogError("Stairs hex not initialized!");
            return false;
        }
        Debug.Log($"Stairs is at hex: {myHex.hexCoords}");

        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance is null in IsPlayerAdjacent!");
            return false;
        }

        Unit playerUnit = GameManager.Instance.PlayerUnit;
        if (playerUnit == null)
        {
            Debug.LogError("Player unit not found!");
            return false;
        }

        if (HexGrid.Instance == null)
        {
            Debug.LogError("HexGrid.Instance is null!");
            return false;
        }

        Debug.Log($"Player world position: {playerUnit.transform.position}");
        Debug.Log($"Stairs world position: {transform.position}");

        float worldDistance = Vector3.Distance(playerUnit.transform.position, transform.position);
        Debug.Log($"World distance between player and stairs: {worldDistance:F2}");

        if (worldDistance <= 3.0f) 
        {
            Debug.Log("WORLD DISTANCE CHECK: Player is close enough - ADJACENT!");
            return true;
        }

        Vector3Int playerHexCoords = CalculateHexFromWorldPos(playerUnit.transform.position);
        Vector3Int stairsHexCoords = CalculateHexFromWorldPos(transform.position);
        
        Debug.Log($"Manual calculation - Player hex: {playerHexCoords}, Stairs hex: {stairsHexCoords}");

        int hexDistance = Mathf.Abs(playerHexCoords.x - stairsHexCoords.x) + 
                         Mathf.Abs(playerHexCoords.z - stairsHexCoords.z);
        Debug.Log($"Manual hex distance: {hexDistance}");

        if (hexDistance <= 1)
        {
            Debug.Log("MANUAL HEX DISTANCE CHECK: Player is adjacent - ADJACENT!");
            return true;
        }

        Debug.Log("Player is NOT adjacent to stairs.");
        Debug.Log("===============================");
        return false;
    }

    private Vector3Int CalculateHexFromWorldPos(Vector3 worldPos)
    {
        float hexWidth = HexGrid.Instance.hexWidth;
        float hexHeight = HexGrid.Instance.hexHeight;
        float zSpacing = hexHeight * 0.75f;

        int z = Mathf.RoundToInt(worldPos.z / zSpacing);
        float xOffset = (z % 2 != 0) ? hexWidth / 2f : 0;
        int x = Mathf.RoundToInt((worldPos.x - xOffset) / hexWidth);

        return new Vector3Int(x, 0, z);
    }


    private Hex GetHexBelow()
    {
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out RaycastHit hit, 2f))
        {
            Hex hex = hit.collider.GetComponent<Hex>();
            if (hex != null) return hex;
        }
        
        Vector3Int coords = HexGrid.Instance.GetClosestHex(transform.position);
        return HexGrid.Instance.GetTileAt(coords);
    }

    private void OnDrawGizmosSelected()
    {
        if (myHex != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(myHex.transform.position + Vector3.up * 0.5f, Vector3.one * 0.9f);
        }
    }
}