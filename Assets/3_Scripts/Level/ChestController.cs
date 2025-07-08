using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ChestController : MonoBehaviour
{
    [Tooltip("Reference to the ItemDatabase Asset that holds all possible loot.")]
    [SerializeField] private ItemDatabase itemDatabase;

    private Hex currentHex;
    private bool isOpened = false;

    private void Start()
    {
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
            Debug.Log($"Chest registered at {hexCoords}");
        }
        else
        {
            Debug.LogError($"Chest not on grid! ");
        }
    }

    public void OpenChest()
    {
        if (isOpened || itemDatabase == null) return;
        
        isOpened = true;
        
        ItemData randomItem = itemDatabase.GetRandomItem();
        if (randomItem != null && EquipmentManager.Instance != null)
        {
            Debug.Log($"Chest opened! Picking up {randomItem.name}");
            
            EquipmentManager.Instance.EquipItem(randomItem);
        }

        if (currentHex != null)
        {
            currentHex.ClearChest();
        }
        Destroy(gameObject, 0.3f);
    }
}
