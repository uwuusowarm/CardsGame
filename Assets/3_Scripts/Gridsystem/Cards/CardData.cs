using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Cards/Card Data")]
public class CardData : ScriptableObject
{
    public string cardName;
    public int manaCost;
    public Sprite cardArt;
    public CardClass cardClass;
    public Sprite backgroundSprite;
    public CardRarity rarity;
    public Sprite borderSprite;

    [Header("Effects")]
    public List<CardEffect> leftEffects = new List<CardEffect>();
    public List<CardEffect> rightEffects = new List<CardEffect>();
    public List<CardEffect> alwaysEffects = new List<CardEffect>();
    [Header("Icons")]
    public Sprite leftEffectIcon;
    public Sprite rightEffectIcon;

    [TextArea] public string description;
}

public enum CardClass
{
    Base,
    Wizard,    
    Warrior,   
    Rogue,     
    Monster
}

public enum CardRarity
{
    Common,
    Rare,
    Legendary,
    Monster
}