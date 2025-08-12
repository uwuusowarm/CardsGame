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
    
        if (!isInitialized)
        {
            Debug.Log("StairsToMenu not initialized yet!");
            return;
        }
        
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
        if (HexGrid.Instance == null) return false;

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            return false;
        }

        Vector3Int playerHexCoords = HexGrid.Instance.GetClosestHex(player.transform.position);

        if (myHex.hexCoords == playerHexCoords)
        {
            return true;
        }

        var neighbors = HexGrid.Instance.GetNeighborsFor(myHex.hexCoords);
        return neighbors.Contains(playerHexCoords);
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