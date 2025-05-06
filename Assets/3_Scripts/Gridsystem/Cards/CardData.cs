using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Cards/Card Data")]
public class CardData : ScriptableObject
{
    public string cardName;
    public int manaCost;
    public Sprite cardArt;

    [Header("Effects")]
    public List<CardEffect> leftEffects = new List<CardEffect>();
    public List<CardEffect> rightEffects = new List<CardEffect>();
    public List<CardEffect> alwaysEffects = new List<CardEffect>();

    [TextArea] public string description;
}