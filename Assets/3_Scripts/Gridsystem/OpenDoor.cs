using System.Collections;
using UnityEngine;

public class OpenDoor : MonoBehaviour
{
    public int roomID;
    public bool isOpen = false;

    private Hex myHex;
    private Renderer rend;
    private bool isInitialized = false;

    [Header("Tür Visuals")]
    [SerializeField] private GameObject türGeschlossenObjekt;
    [SerializeField] private GameObject türOffenObjekt;

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => HexGrid.Instance != null);
        yield return new WaitForSeconds(0.1f); 

        myHex = GetHexBelow();

        if (myHex != null)
        {
            roomID = myHex.RoomID;
            rend = GetComponentInChildren<Renderer>();

            if (!isOpen)
                SetClosed();
            else
                SetOpen();

            isInitialized = true;
        }
        else
        {
            Debug.LogError("Door cant find Hex.");
            enabled = false;
        }
    }

    private void OnMouseDown()
    {
        if (!isInitialized || myHex == null)
        {
            Debug.LogError("Door not initialized.");
            return;
        }

        if (!isOpen && IsPlayerAdjacent())
        {
            Open();
        }
    }

    private bool IsPlayerAdjacent()
    {
        if (HexGrid.Instance == null) return false;

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return false;

        Vector3Int playerHex = HexGrid.Instance.GetClosestHex(player.transform.position);
        var neighbors = HexGrid.Instance.GetNeighborsFor(myHex.hexCoords);

        return neighbors.Contains(playerHex);
    }

    public void Open()
    {
        isOpen = true;
        SetOpen();
        Debug.Log($"Tür zu Raum {roomID} geöffnet!");

        if (EnemyActivator.Instance != null)
        {
            EnemyActivator.Instance.ActivateEnemiesInRoom(roomID);
        }
        else
        {
            Debug.LogWarning("EnemyActivator nicht gefunden.");
        }
    }

    private void SetClosed()
    {
        myHex.HexType = HexType.Obstacle;
        if (rend != null) rend.material.color = Color.red;

        if (türGeschlossenObjekt != null) türGeschlossenObjekt.SetActive(true);
        if (türOffenObjekt != null) türOffenObjekt.SetActive(false);
    }

    private void SetOpen()
    {
        myHex.HexType = HexType.Default;
        if (rend != null) rend.material.color = Color.green;

        if (türGeschlossenObjekt != null) türGeschlossenObjekt.SetActive(false);
        if (türOffenObjekt != null) türOffenObjekt.SetActive(true);
    }

    private Hex GetHexBelow()
    {
        if (transform.parent != null)
        {
            Hex fromParent = transform.parent.GetComponent<Hex>();
            if (fromParent != null) return fromParent;
        }
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out RaycastHit hit, 2f))
        {
            Hex fromRaycast = hit.collider.GetComponent<Hex>();
            if (fromRaycast != null) return fromRaycast;
        }
        Vector3Int coords = HexGrid.Instance.GetClosestHex(transform.position);
        return HexGrid.Instance.GetTileAt(coords);
    }

    private void OnDrawGizmosSelected()
    {
        if (myHex != null)
        {
            Gizmos.color = isOpen ? Color.green : Color.red;
            Gizmos.DrawWireCube(myHex.transform.position + Vector3.up * 0.5f, Vector3.one * 0.9f);
        }
    }
}
