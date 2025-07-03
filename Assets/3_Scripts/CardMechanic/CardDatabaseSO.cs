using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardDatabase", menuName = "Cards/Card Database", order = 1)]
public class CardDatabaseSO : ScriptableObject
{
    public List<CardData> allCards;
}