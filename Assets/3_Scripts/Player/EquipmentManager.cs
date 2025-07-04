
using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance { get; private set; }

    private Dictionary<ItemSlot, ItemData> equippedItems = new Dictionary<ItemSlot, ItemData>();
    public ItemClassType playerClass = ItemClassType.Warrior; // Set this based on player's class

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

        InitializeEquipmentSlots();
    }

    private void InitializeEquipmentSlots()
    {
        equippedItems[ItemSlot.Head] = null;
        equippedItems[ItemSlot.Torso] = null;
        equippedItems[ItemSlot.Shoes] = null;
        equippedItems[ItemSlot.Weapon] = null;
    }

    public void EquipItem(ItemData item)
    {
        if (item == null) return;
        
        if (item.itemClass != ItemClassType.Any && item.itemClass != playerClass)
        {
            Debug.Log($"Cannot equip {item.name}: wrong class type!");
            return;
        }

        equippedItems[item.itemSlot] = item;
    }

    public void UnequipItem(ItemSlot slot)
    {
        equippedItems[slot] = null;
    }

    public int GetTotalDamageBonus()
    {
        int bonus = 0;
        foreach (var item in equippedItems.Values)
        {
            if (item == null) continue;
            
            bonus += item.damageBonus;
            
            if (item.classBonus1Type == StatBonusType.Dmg)
                bonus += item.classBonus1Amount;
            if (item.classBonus2Type == StatBonusType.Dmg)
                bonus += item.classBonus2Amount;
        }
        return bonus;
    }

    public int GetTotalDefenseBonus()
    {
        int bonus = 0;
        foreach (var item in equippedItems.Values)
        {
            if (item == null) continue;
            
            bonus += item.defenseBonus;
            
            if (item.classBonus1Type == StatBonusType.Def)
                bonus += item.classBonus1Amount;
            if (item.classBonus2Type == StatBonusType.Def)
                bonus += item.classBonus2Amount;
        }
        return bonus;
    }

    public int GetTotalHealBonus()
    {
        int bonus = 0;
        foreach (var item in equippedItems.Values)
        {
            if (item == null) continue;
            
            bonus += item.healBonus;
            
            if (item.classBonus1Type == StatBonusType.Heal)
                bonus += item.classBonus1Amount;
            if (item.classBonus2Type == StatBonusType.Heal)
                bonus += item.classBonus2Amount;
        }
        return bonus;
    }

    public int GetTotalMovementSpeedBonus()
    {
        int bonus = 0;
        foreach (var item in equippedItems.Values)
        {
            if (item == null) continue;
            
            bonus += item.movementSpeedBonus;
            
            if (item.classBonus1Type == StatBonusType.MS)
                bonus += item.classBonus1Amount;
            if (item.classBonus2Type == StatBonusType.MS)
                bonus += item.classBonus2Amount;
        }
        return bonus;
    }

    public int GetTotalMaxHPBonus()
    {
        int bonus = 0;
        foreach (var item in equippedItems.Values)
        {
            if (item == null) continue;
            
            bonus += item.maxHpBonus;
            
            if (item.classBonus1Type == StatBonusType.MaxHP)
                bonus += item.classBonus1Amount;
            if (item.classBonus2Type == StatBonusType.MaxHP)
                bonus += item.classBonus2Amount;
        }
        return bonus;
    }

    public int GetTotalAPBonus()
    {
        int bonus = 0;
        foreach (var item in equippedItems.Values)
        {
            if (item == null) continue;
            
            bonus += item.maxApBonus;
            
            if (item.classBonus1Type == StatBonusType.AP)
                bonus += item.classBonus1Amount;
            if (item.classBonus2Type == StatBonusType.AP)
                bonus += item.classBonus2Amount;
        }
        return bonus;
    }

    public int GetWeaponRange()
    {
        return equippedItems[ItemSlot.Weapon]?.range ?? 1;
    }

    public ItemData GetEquippedItem(ItemSlot slot)
    {
        return equippedItems.ContainsKey(slot) ? equippedItems[slot] : null;
    }
}