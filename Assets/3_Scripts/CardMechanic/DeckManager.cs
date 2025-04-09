using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public List<Card> cardDeck = new List<Card>();
    public Transform handZone;
    public GameObject cardPrefab;

    public static DeckManager Instance;

    void Awake()
    {
        Instance = this;
    }

    public void DrawCard()
    {
        if (cardDeck.Count == 0) return;

        Card card = cardDeck[0];
        cardDeck.RemoveAt(0);

        GameObject newCard = Instantiate(cardPrefab, handZone);
        CardDisplay display = newCard.GetComponent<CardDisplay>();
        display.card = card;
    }
}

