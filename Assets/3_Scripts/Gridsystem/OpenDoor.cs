using System.Collections;
using UnityEngine;

public class OpenDoor : MonoBehaviour
{
    public int roomID;
    public bool isOpen = false;
    private Hex myHex;
    private Renderer rend;

    private void Awake()
    {
        myHex = GetHexBelow();
        if (myHex == null)
        {
            Debug.LogError("OpenDoor: No Hex found below the door!");
            enabled = false;
            return;
        }
        roomID = myHex.RoomID; 
        rend = GetComponentInChildren<Renderer>();
    }

    private Hex GetHexBelow()
    {
        if (transform.parent != null)
            return transform.parent.GetComponent<Hex>();
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, 2f))
            return hit.collider.GetComponent<Hex>();

        return null;
    }

    private void Start()
    {
        if (!isOpen)
            SetClosed();
        else
            SetOpen();
    }

    private void OnMouseDown()
    {
        if (isOpen) return;
        if (!IsPlayerAdjacent()) return;

        Open();
    }

    private bool IsPlayerAdjacent()
    {
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
        EnemyActivator.Instance.ActivateEnemiesInRoom(roomID);
    }

    private void SetClosed()
    {
        myHex.HexType = HexType.Obstacle;
        if (rend != null) rend.material.color = Color.red;
    }

    private void SetOpen()
    {
        myHex.HexType = HexType.Default;
        if (rend != null) rend.material.color = Color.green;
    }
}
