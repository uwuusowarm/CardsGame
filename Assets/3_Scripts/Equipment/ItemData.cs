using UnityEngine;

public enum ItemSlot
{
    None, 
    Head,
    Shoes,
    Torso,
    Weapon
}

public enum ItemClassType
{
    Base,
    Rogue,
    Warrior,
    Wizard,
    Any 
}

public enum StatBonusType
{
    None,
    Dmg,
    MS, 
    Def,
    Heal,
    MaxHP,
    AP 
}

[CreateAssetMenu(fileName = "NewItemData", menuName = "Game/Item Data", order = 1)]
public class ItemData : ScriptableObject
{
    [Header("Basic Info")]
    public int id;
    public ItemClassType itemClass = ItemClassType.Base; 
    public new string name; 
    public int tier;
    public ItemSlot itemSlot = ItemSlot.None; 

    [Header("Core Stats")]
    public int damageBonus;
    public int movementSpeedBonus; 
    public int defenseBonus;
    public int healBonus;
    public int maxHpBonus;
    public int maxApBonus;
    public int range;

    [Header("Class Specific Bonuses")]
    public StatBonusType classBonus1Type = StatBonusType.None;
    public int classBonus1Amount;
    public StatBonusType classBonus2Type = StatBonusType.None;
    public int classBonus2Amount;

    [Header("Overall")]
    public int totalValue;
    
    [Header("InGame")]
    public Sprite itemIcon;
    public GameObject itemPrefab;
}