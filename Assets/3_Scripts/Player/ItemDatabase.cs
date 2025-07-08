using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "ItemDatabase", order = 1)]
public class ItemDatabase : ScriptableObject
{
    public List<ItemData> allItems = new List<ItemData>();

    public ItemData GetRandomItem()
    {
        if (allItems == null || allItems.Count == 0)
        {
            Debug.Log("ItemDatabse is empty. Cannot get random Item!");
            return null;
        }
        
        int randomIndex = Random.Range(0, allItems.Count);
        return allItems[randomIndex];
    }
}

