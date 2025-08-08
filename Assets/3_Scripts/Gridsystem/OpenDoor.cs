using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenDoor : MonoBehaviour
{
    public int roomID;
    public bool isOpen = false;
    
    //[Header("Verlinkte Hex-Felder")]
    //[SerializeField] private List<Hex> linkedHexes;

    [Header("Tür Visuals")]
    [SerializeField] private GameObject türGeschlossenObjekt;
    [SerializeField] private GameObject türOffenObjekt;

    private Hex myHex;
    private bool isInitialized = false;

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => HexGrid.Instance != null);
        yield return new WaitForSeconds(0.1f);

        myHex = GetHexBelow();

        if (myHex != null)
        {
            roomID = myHex.RoomID;
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
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    HandleDoorClick();
                }
            }
        }
    }
    private void HandleDoorClick()
    {
        if (!isInitialized || myHex == null) return;

        Debug.Log("Door clicked via raycast");

        if (!IsPlayerAdjacent())
        {
            Debug.Log("Player not adjacent");
            return;
        }

        if (!isOpen)
        {
            Open();
        }
    }

    /*private void OnMouseDown()
    {
        Debug.Log("Mouse click detected");
        if (!isInitialized || myHex == null || isOpen) return;

        if (IsPlayerAdjacent())
        {
            Open();
        }
    }*/

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
        Debug.Log($"Door open to room {roomID}!");

        if (EnemyActivator.Instance != null)
        {
            EnemyActivator.Instance.ActivateEnemiesInRoom(roomID);
        }
    }

    private void SetClosed()
    {
        myHex.HexType = HexType.Obstacle;
        if (türGeschlossenObjekt != null) türGeschlossenObjekt.SetActive(true);
        if (türOffenObjekt != null) türOffenObjekt.SetActive(false);
    }

    private void SetOpen()
    {
        myHex.HexType = HexType.Road;
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
}