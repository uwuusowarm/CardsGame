using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StairsToMenu : MonoBehaviour
{
    private Hex myHex;
    private bool isInitialized = false;
    
    public string scene;

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => HexGrid.Instance != null);

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

    private void OnMouseDown()
    {
        if (!isInitialized)
        {
            return;
        }

        if (IsPlayerAdjacent())
        {
            Debug.Log("Player is adjacent and clicked the exit. Loading Main Menu.");
            
            SceneManager.LoadScene(scene);
        }
        else
        {
            Debug.Log("Player clicked the exit, but is not close enough.");
        }
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

        var neighbors = HexGrid.Instance.GetNeighborsFor(myHex.hexCoords);

        return neighbors.Contains(playerHexCoords);
    }

    private Hex GetHexBelow()
    {
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out RaycastHit hit, 2f))
        {
            return hit.collider.GetComponent<Hex>();
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