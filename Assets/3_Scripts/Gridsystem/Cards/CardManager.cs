using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; }

    [Header("Deck Settings")]
    [SerializeField] private List<CardData> initialDeck = new List<CardData>();
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform handContainer;

    private List<CardData> deck = new List<CardData>();
    private List<CardData> discardPile = new List<CardData>();
    private List<GameObject> currentHand = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeDeck();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void InitializeDeck()
    {
        deck = new List<CardData>(initialDeck);
        ShuffleDeck();
    }

    public void DrawInitialCards()
    {
        DrawCards(2);
    }

    public void DrawCards(int amount)
    {
        if (cardPrefab == null || handContainer == null)
        {
            Debug.LogError("Card references not set!");
            return;
        }

        for (int i = 0; i < amount; i++)
        {
            if (deck.Count == 0)
            {
                ReshuffleDiscardPile();
                if (deck.Count == 0) return;
            }

            CardData drawnCard = deck[0];
            deck.RemoveAt(0);

            GameObject cardObj = Instantiate(cardPrefab, handContainer);
            cardObj.GetComponent<CardUI>().Initialize(drawnCard);
            currentHand.Add(cardObj);
        }
    }

    private void ReshuffleDiscardPile()
    {
        deck.AddRange(discardPile);
        discardPile.Clear();
        ShuffleDeck();
    }

    private void ShuffleDeck()
    {
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            CardData temp = deck[i];
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }

    public void DiscardCard(GameObject cardObject)
    {
        if (cardObject != null)
        {

            currentHand.Remove(cardObject);

            CardUI cardUI = cardObject.GetComponent<CardUI>();
            if (cardUI != null && cardUI.GetCardData() != null)
            {
                discardPile.Add(cardUI.GetCardData());
            }

            Destroy(cardObject);
        }
    }

    public void DiscardHand()
    {
        foreach (var cardObject in currentHand.ToArray())
        {
            DiscardCard(cardObject);
        }
        currentHand.Clear();
    }
}