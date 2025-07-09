using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Game/PlayerData")]
public class PlayerDataSO : ScriptableObject
{
    private const int DEFAULT_STARTING_HEALTH = 5;

    [System.Serializable]
    public struct EquippedItemEntry
    {
        public ItemSlot slot;
        public ItemData itemData;
    }

    [Header("Player Health")]
    public int savedCurrentHealth = DEFAULT_STARTING_HEALTH;
    public int savedUnlockedExtraHealthSlots = 0;

    [Header("Player Stats")]
    public int savedExhaustionStacks = 0;

    [Header("Player Equipment")]
    public List<EquippedItemEntry> savedEquippedItems = new List<EquippedItemEntry>();
    
    public void ResetToDefaults()
    {
        savedCurrentHealth = DEFAULT_STARTING_HEALTH;
        savedUnlockedExtraHealthSlots = 0;
        savedExhaustionStacks = 0;
        savedEquippedItems.Clear();
        
        Debug.Log("PlayerDataSO has been reset to default values for a new run.");
    }
}