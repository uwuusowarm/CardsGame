using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public List<Card> allCards;
    public Transform handArea;
    public GameObject cardPrefab;

    private List<Card> deck = new();

    void Start()
    {
        deck = new List<Card>(allCards);
        ShuffleDeck();
    }

    public void ShuffleDeck()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            Card temp = deck[i];
            int rand = Random.Range(i, deck.Count);
            deck[i] = deck[rand];
            deck[rand] = temp;
        }
    }

    public void DrawCard()
    {
        if (deck.Count == 0) return;

        Card cardToDraw = deck[0];
        deck.RemoveAt(0);

        GameObject newCard = Instantiate(cardPrefab, handArea);
        CardDisplay display = newCard.GetComponent<CardDisplay>();
        display.card = cardToDraw;
    }
}

