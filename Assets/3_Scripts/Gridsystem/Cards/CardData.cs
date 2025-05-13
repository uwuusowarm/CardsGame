using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Cards/Card Data")]
public class CardData : ScriptableObject
{
    public string cardName;
    public int manaCost;
    public Sprite cardArt;
    public CardClass cardClass;
    public CardRarity rarity;

    [Header("Effects")]
    public List<CardEffect> leftEffects = new List<CardEffect>();
    public List<CardEffect> rightEffects = new List<CardEffect>();
    public List<CardEffect> alwaysEffects = new List<CardEffect>();

    [TextArea] public string description;
}

public enum CardClass
{
    Mage,
    Knight,
    Archer,
    Priest,
    Rogue
}

public enum CardRarity
{
    Common,
    Rare,
    Legendary
}