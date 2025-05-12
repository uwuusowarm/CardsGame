using System.Collections.Generic;
using UnityEngine;

public class CardGameManager : MonoBehaviour
{
    [SerializeField] private List<Card> deck = new List<Card>();
    [SerializeField] private List<Card> hand = new List<Card>();
    [SerializeField] private List<Card> discardPile = new List<Card>();

    [SerializeField, Range(1, 20)] public int cardsToDraw = 4; // Standardwert 4, Rage 1-20

    void Start()
    {
        InitializeDeck();
    }

    public Card DrawCardFromDeck()
    {
        if (deck.Count == 0 && discardPile.Count > 0)
        {
            ShuffleDiscardIntoDeck();
        }
        else if (deck.Count == 0)
        {
            Debug.LogWarning("Keine Karten mehr im Deck.");
            return null;
        }

        int index = Random.Range(0, deck.Count);
        Card drawnCard = deck[index];
        deck.RemoveAt(index);
        hand.Add(drawnCard);

        return drawnCard;
    }

    private void ShuffleDiscardIntoDeck()
    {
        deck.AddRange(discardPile);
        discardPile.Clear();
        ShuffleDeck();
    }

    private void InitializeDeck()
    {
       
        foreach (Card card in deck)
        {
            deck.Add(card);
        }
    }

    public List<Card> GetHand()
    {
        return hand;
    }

    private void ShuffleDeck()
    {
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            Card temp = deck[i];
            deck[i] = deck[j];
            deck[j] = temp;
        }
    }
}
