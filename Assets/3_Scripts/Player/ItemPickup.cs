using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Tooltip("The ScriptableObject that defines this item's properties.")]
    [SerializeField]
    private ItemData itemData;

    public ItemData Data => itemData;
    
    public void Equip()
    {
        if (itemData == null)
        {
            Debug.LogError("ItemData is not assigned on this ItemPickup component!", gameObject);
            return;
        }

        if (EquipmentManager.Instance != null)
        {
            EquipmentManager.Instance.EquipItem(itemData);
            
            Debug.Log($"Player picked up and equipped {itemData.name}.");
            Destroy(gameObject);
        }
        else
        {
            Debug.LogError("EquipmentManager could not be found. Cannot equip item.");
        }
    }
}
