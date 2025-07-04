using System.Collections.Generic;

[System.Serializable]
public class Deck
{
    public string DeckName;
    public List<CardData> Cards = new List<CardData>();
}