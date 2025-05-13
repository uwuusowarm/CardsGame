using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewCardScriptable", menuName = "Cards/Card Scriptable")]
public class CardScriptable : ScriptableObject
{
    public string cardName;
    public int manaCost;
    public Sprite cardArt;

    [Header("Effects")]
    public List<CardEffect> leftEffects = new List<CardEffect>();
    public List<CardEffect> rightEffects = new List<CardEffect>();

    // Neue Liste für alwaysEffects
    public List<CardEffect> alwaysEffects = new List<CardEffect>();

    [TextArea] public string description;
}
