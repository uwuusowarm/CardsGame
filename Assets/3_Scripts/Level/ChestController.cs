
using System.Collections;
using System.Collections.Generic; 
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ChestController : MonoBehaviour
{
    [Tooltip("Reference to the ItemDatabase asset that holds all possible loot.")]
    [SerializeField] private ItemDatabase itemDatabase;

    [Header("Testing")]
    [SerializeField, Tooltip("If true, this chest will open on click regardless of player position, turn, or AP cost.")]
    private bool testMode = false;

    [Header("Visuals")]
    [SerializeField] private GameObject chestClosedObject;
    [SerializeField] private GameObject chestOpenObject;
    
    private Hex currentHex;
    private bool isOpen = false;

    private void Start()
    {
        if (itemDatabase == null)
        {
            itemDatabase = FindObjectOfType<ItemDatabase>();
            if(itemDatabase == null)
                Debug.LogError("No ItemDatabase found in the scene or assigned to the chest!", gameObject);
        }
        
        StartCoroutine(InitializeHexPosition());
    }

    public void OnChestClicked()
    {
        if (isOpen) return;
        
        if (testMode)
        {
            Debug.LogWarning("CHEST IN TEST MODE: Bypassing all checks and opening immediately.");
            OpenChestAndGiveLoot();
            return; 
        }
        
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance is null!");
            return;
        }
        
        if (!GameManager.Instance.IsPlayerTurn)
        {
            Debug.Log("Cannot open chest, it is not the player's turn.");
            return;
        }

        if (ActionPointSystem.Instance == null)
        {
            Debug.LogError("ActionPointSystem.Instance is null!");
            return;
        }

        if (ActionPointSystem.Instance.GetCurrentActionPoints() < 1)
        {
            Debug.Log("Not enough Action Points to open the chest.");
            return;
        }

        Unit playerUnit = GameManager.Instance.PlayerUnit;
        if (playerUnit == null)
        {
            Debug.LogError("Player unit not found in GameManager!");
            return;
        }

        if (!IsPlayerAdjacentToChest())
        {
            Debug.Log("Player must be adjacent to the chest to open it.");
            return;
        }

        Debug.Log("Opening chest!");

        ActionPointSystem.Instance.UseActionPoints(1);

        OpenChestAndGiveLoot();
    }

    private IEnumerator InitializeHexPosition()
    {
        yield return new WaitUntil(() => HexGrid.Instance != null);

        HexGrid hexGrid = HexGrid.Instance;
        Vector3Int hexCoords = hexGrid.GetClosestHex(transform.position);
        currentHex = hexGrid.GetTileAt(hexCoords);

        if (currentHex != null)
        {
            currentHex.SetChest(this);
            Debug.Log($"Chest initialized at hex {hexCoords}");
        }
        else
        {
            Debug.LogError($"Chest failed to find a valid hex at position {transform.position}");
        }
    }
    
    private bool IsPlayerAdjacentToChest()
    {
        Debug.Log("=== CHEST ADJACENCY DEBUG ===");
        
        if (currentHex == null)
        {
            Debug.LogError("Chest hex not initialized!");
            return false;
        }
        Debug.Log($"Chest is at hex: {currentHex.HexCoords}");

        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance is null in IsPlayerAdjacentToChest!");
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
        Debug.Log($"Chest world position: {transform.position}");

        float worldDistance = Vector3.Distance(playerUnit.transform.position, transform.position);
        Debug.Log($"World distance between player and chest: {worldDistance:F2}");

        if (worldDistance <= 3.0f) 
        {
            Debug.Log("WORLD DISTANCE CHECK: Player is close enough - ADJACENT!");
            return true;
        }

        Vector3Int playerHexCoords = CalculateHexFromWorldPos(playerUnit.transform.position);
        Vector3Int chestHexCoords = CalculateHexFromWorldPos(transform.position);
        
        Debug.Log($"Manual calculation - Player hex: {playerHexCoords}, Chest hex: {chestHexCoords}");

        int hexDistance = Mathf.Abs(playerHexCoords.x - chestHexCoords.x) + 
                         Mathf.Abs(playerHexCoords.z - chestHexCoords.z);
        Debug.Log($"Manual hex distance: {hexDistance}");

        if (hexDistance == 1)
        {
            Debug.Log("MANUAL HEX DISTANCE CHECK: Player is 1 hex away - ADJACENT!");
            return true;
        }

        Debug.Log("Player is NOT adjacent to chest.");
        Debug.Log("============================");
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


    private void OpenChestAndGiveLoot()
    {
        if (isOpen) return;
        isOpen = true;

        if (Sound_Manager.instance != null)
        {
            Sound_Manager.instance.Play("Chest_Open");
        }
        else
        {
            Debug.LogWarning("Sound_Manager.instance is null - skipping sound");
        }

        ItemData randomItem = itemDatabase.GetRandomItem();
        if (randomItem != null && EquipmentManager.Instance != null)
        {
            Debug.Log($"Player opened a chest and found: {randomItem.name}!");
            EquipmentManager.Instance.EquipItem(randomItem);
        }
        if (chestClosedObject != null) chestClosedObject.SetActive(false);
        if (chestOpenObject != null) chestOpenObject.SetActive(true);
    }
}