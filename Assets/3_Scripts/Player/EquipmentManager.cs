using System; 
using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance { get; private set; }

    public event Action<ItemSlot> OnEquipmentChanged;

    private Dictionary<ItemSlot, ItemData> equippedItems = new Dictionary<ItemSlot, ItemData>();
    public ItemClassType playerClass = ItemClassType.Warrior; 

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

        if (equippedItems[item.itemSlot] != null)
        {
            Debug.Log($"Replaced '{equippedItems[item.itemSlot].name}' with '{item.name}' in slot {item.itemSlot}.");
        }

        equippedItems[item.itemSlot] = item;
        Debug.Log($"Equipped '{item.name}'.");

        OnEquipmentChanged?.Invoke(item.itemSlot);
    }

    public void UnequipItem(ItemSlot slot)
    {
        if (equippedItems.ContainsKey(slot) && equippedItems[slot] != null)
        {
            Debug.Log($"Unequipped {equippedItems[slot].name} from slot {slot}.");
            equippedItems[slot] = null;
            
            OnEquipmentChanged?.Invoke(slot);
        }
    }

     public int GetTotalDamageBonus()
    {
        int bonus = 0;
        foreach (var item in equippedItems.Values)
        {
            if (item == null) continue;
            bonus += item.damageBonus;
            
            if (playerClass == item.itemClass)
            {
                if (item.classBonus1Type == StatBonusType.Dmg) bonus += item.classBonus1Amount;
                if (item.classBonus2Type == StatBonusType.Dmg) bonus += item.classBonus2Amount;
            }
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
            
            if (playerClass == item.itemClass)
            {
                if (item.classBonus1Type == StatBonusType.Def) bonus += item.classBonus1Amount;
                if (item.classBonus2Type == StatBonusType.Def) bonus += item.classBonus2Amount;
            }
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
            
            if (playerClass == item.itemClass)
            {
                if (item.classBonus1Type == StatBonusType.Heal) bonus += item.classBonus1Amount;
                if (item.classBonus2Type == StatBonusType.Heal) bonus += item.classBonus2Amount;
            }
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
            
            if (playerClass == item.itemClass)
            {
                if (item.classBonus1Type == StatBonusType.MS) bonus += item.classBonus1Amount;
                if (item.classBonus2Type == StatBonusType.MS) bonus += item.classBonus2Amount;
            }
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
            
            if (playerClass == item.itemClass)
            {
                if (item.classBonus1Type == StatBonusType.MaxHP) bonus += item.classBonus1Amount;
                if (item.classBonus2Type == StatBonusType.MaxHP) bonus += item.classBonus2Amount;
            }
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
            
            if (playerClass == item.itemClass)
            {
                if (item.classBonus1Type == StatBonusType.AP) bonus += item.classBonus1Amount;
                if (item.classBonus2Type == StatBonusType.AP) bonus += item.classBonus2Amount;
            }
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