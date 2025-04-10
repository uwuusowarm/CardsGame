using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public List<Card> allPossibleCards;
    public List<Card> cardDeck = new List<Card>();
    public Transform handZone;
    public GameObject cardPrefab;
    public int initialHandSize = 5;

    public static DeckManager Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        InitializeDeck();
        DrawInitialHand();
    }
    void InitializeDeck()
    {
        cardDeck = new List<Card>(allPossibleCards);
        ShuffleDeck();
    }

    void ShuffleDeck()
    {
        for (int i = 0; i < cardDeck.Count; i++)
        {
            Card temp = cardDeck[i];
            int randomIndex = Random.Range(i, cardDeck.Count);
            cardDeck[i] = cardDeck[randomIndex];
            cardDeck[randomIndex] = temp;
        }
    }

    void DrawInitialHand()
    {
        for (int i = 0; i < initialHandSize; i++)
        {
            if (cardDeck.Count > 0)
            {
                DrawCard();
            }
            else
            {
                Debug.LogWarning("Deck ist leer! Keine Karten mehr zum Ziehen.");
                break;
            }
        }
    }
    public void DrawCard()
    {
        if (cardDeck.Count == 0)
        {
            Debug.LogWarning("Deck ist leer!");
            return;
        }

        Card card = cardDeck[0];
        cardDeck.RemoveAt(0);

        GameObject newCard = Instantiate(cardPrefab, handZone);
        CardDisplay display = newCard.GetComponent<CardDisplay>();
        if (display != null)
        {
            display.card = card;
        }
        else
        {
            Debug.LogError("CardPrefab hat keine CardDisplay Komponente!");
        }
    }
    /*
    public void DrawCard()
    {
        if (cardDeck.Count == 0) return;

        Card card = cardDeck[0];
        cardDeck.RemoveAt(0);

        GameObject newCard = Instantiate(cardPrefab, handZone);
        CardDisplay display = newCard.GetComponent<CardDisplay>();
        display.card = card;
    }*/
}

