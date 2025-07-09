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
        myHex = GetComponent<Hex>();
        rend = GetComponentInChildren<Renderer>();
        if (myHex == null)
            Debug.LogError("OpenDoor: No hex!");
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
        myHex.SetPlacedObject(gameObject);
        myHex.HexType = HexType.Obstacle;
        if (rend != null) rend.material.color = Color.red; 
    }

    private void SetOpen()
    {
        myHex.HexType = HexType.Default;
        if (rend != null) rend.material.color = Color.green; 
    }
}
