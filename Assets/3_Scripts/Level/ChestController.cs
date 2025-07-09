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
    
    private void OnMouseDown()
    {
        if (isOpen) return;
        
        if (testMode)
        {
            Debug.LogWarning("CHEST IN TEST MODE: Bypassing all checks and opening immediately.");
            OpenChestAndGiveLoot();
            return; 
        }
        
        if (!GameManager.Instance.IsPlayerTurn)
        {
            Debug.Log("Cannot open chest, it is not the player's turn.");
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

        float distance = HexGrid.Instance.GetDistance(playerUnit.GetCurrentHex(), this.currentHex);

        if (distance > 1) 
        {
            Debug.Log("Player is too far away to open the chest.");
            return;
        }

        Debug.Log("Opening chest!");

        ActionPointSystem.Instance.UseActionPoints(1);

        OpenChestAndGiveLoot();
    }
    
    private void OpenChestAndGiveLoot()
    {
        if (isOpen) return;
        isOpen = true;

        ItemData randomItem = itemDatabase.GetRandomItem();
        if (randomItem != null && EquipmentManager.Instance != null)
        {
            Debug.Log($"Player opened a chest and found: {randomItem.name}!");
            EquipmentManager.Instance.EquipItem(randomItem);
        }

        if (currentHex != null)
        {
            currentHex.ClearChest();
        }
        
        Destroy(gameObject);
    }
}
