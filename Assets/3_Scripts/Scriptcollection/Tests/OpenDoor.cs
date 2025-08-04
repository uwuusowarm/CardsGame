using System.Collections;
using UnityEngine;

public class OpenDoor : MonoBehaviour
{
    public int roomID;
    public bool isOpen = false;

    private Hex myHex;
    private Renderer rend;
    private bool isInitialized = false;
    private Vector3Int oppositeHexCoords;

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
            
            yield return new WaitUntil(() => HexGrid.Instance.GetTileAt(myHex.hexCoords) != null);
            
            CalculateOppositeHex();

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

    private void CalculateOppositeHex()
    {
        if (myHex == null || HexGrid.Instance == null) return;
        Vector3 localForward = transform.forward;
        
        Vector3Int bestDirection = Vector3Int.zero;
        float bestDot = -1f;

        var directions = Direction.GetDirectionList(myHex.hexCoords.z);
        foreach (var dir in directions)
        {
            Hex neighborHex = HexGrid.Instance.GetTileAt(myHex.hexCoords + dir);
            if (neighborHex == null) continue;

            Vector3 toNeighbor = (neighborHex.transform.position - myHex.transform.position).normalized;
            float dot = Vector3.Dot(localForward, toNeighbor);
            
            if (dot > bestDot)
            {
                bestDot = dot;
                bestDirection = dir;
            }
        }
        if (bestDirection != Vector3Int.zero)
        {
            oppositeHexCoords = myHex.hexCoords + bestDirection;
            Hex oppositeHex = HexGrid.Instance.GetTileAt(oppositeHexCoords);
            if (oppositeHex == null || oppositeHex.IsOccupied())
            {
                foreach (var dir in directions)
                {
                    Vector3Int testCoords = myHex.hexCoords + dir;
                    Hex testHex = HexGrid.Instance.GetTileAt(testCoords);
                    if (testHex != null && !testHex.IsOccupied())
                    {
                        oppositeHexCoords = testCoords;
                        break;
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("Could not determine opposite hex direction");
            oppositeHexCoords = myHex.hexCoords;
        }
    }

    private void OnMouseDown()
    {
        if (!isInitialized || myHex == null)
        {
            Debug.LogError("Door not initialized.");
            return;
        }

        if (!IsPlayerAdjacent()) return;

        if (!isOpen)
        {
            Open();
        }
        
        TeleportPlayer();
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

    private void TeleportPlayer()
{
    var player = GameObject.FindGameObjectWithTag("Player");
    if (player == null) return;

    Unit playerUnit = player.GetComponent<Unit>();
    if (playerUnit == null) return;

    Hex targetHex = HexGrid.Instance.GetTileAt(oppositeHexCoords);
    if (targetHex == null || targetHex.IsOccupied())
    {
        Debug.LogWarning("Target hex is occupied or doesn't exist!");
        return;
    }
    float currentY = player.transform.position.y;
    Hex currentPlayerHex = HexGrid.Instance.GetTileAt(HexGrid.Instance.GetClosestHex(player.transform.position));
    if (currentPlayerHex != null) currentPlayerHex.ClearUnit();
    Vector3 targetPosition = targetHex.transform.position;
    targetPosition.y = currentY; 
    player.transform.position = targetPosition;
    
    playerUnit.currentHex = targetHex;
    targetHex.SetUnit(playerUnit);

    Debug.Log($"Player teleported to {oppositeHexCoords} (Y-position preserved: {currentY})");
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
        else
        {
            Debug.LogWarning("EnemyActivator not found.");
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

            if (HexGrid.Instance != null && isInitialized)
            {
                Gizmos.color = Color.blue;
                Hex oppositeHex = HexGrid.Instance.GetTileAt(oppositeHexCoords);
                if (oppositeHex != null)
                {
                    Gizmos.DrawWireCube(oppositeHex.transform.position + Vector3.up * 0.5f, Vector3.one * 0.9f);
                    Gizmos.DrawLine(myHex.transform.position, oppositeHex.transform.position);
                }
            }
        }
    }
}