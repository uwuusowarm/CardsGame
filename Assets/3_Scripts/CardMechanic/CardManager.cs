using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; }

    [SerializeField] private CardDatabaseSO cardDatabase;
    [SerializeField, Min(1)] private int drawCount = 4;
    [SerializeField] private Transform handGrid;
    [SerializeField] private Transform leftGrid;
    [SerializeField] private Transform rightGrid;
    [SerializeField] private Transform discardGrid;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private float playCooldown = 0.5f;
    [SerializeField] private float autoDiscardDelay = 3f;

    private List<CardData> deck = new List<CardData>();
    private List<CardData> hand = new List<CardData>();
    private List<CardData> leftZone = new List<CardData>();
    private List<CardData> rightZone = new List<CardData>();
    private List<CardData> discardPile = new List<CardData>();

    public int DrawCount
    {
        get { return drawCount; }
    }

    public RectTransform HandGridRect
    {
        get { return handGrid as RectTransform; }
    }
    public RectTransform LeftGridRect
    {
        get { return leftGrid as RectTransform; }
    }
    public RectTransform RightGridRect
    {
        get { return rightGrid as RectTransform; }
    }
    public RectTransform DiscardGridRect
    {
        get { return discardGrid as RectTransform; }
    }

    private bool isPlayingCard = false;
    private bool hasDrawnInitialHand = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        if (GameDataManager.Instance != null && GameDataManager.Instance.selectedDeck != null)
        {
            Debug.Log("Loading selected deck: " + GameDataManager.Instance.selectedDeck.DeckName);
            deck.Clear();
            deck.AddRange(GameDataManager.Instance.selectedDeck.Cards);
        }
        else
        {
            Debug.LogWarning("No deck selected. Loading all cards from database as a fallback.");
            if (cardDatabase == null)
            {
                Debug.LogError("FATAL: CardDatabaseSO has not been assigned in the CardManager!");
                return;
            }
            deck.Clear();
            deck.AddRange(cardDatabase.allCards);
        }

        Shuffle(deck);
        DrawInitialCards();
        UpdateAllUI();
    }

    public void DrawCard() => DrawExtraCards(1);

    public void DrawCard(int count) => DrawExtraCards(count);
    private void Shuffle(List<CardData> cardList)
    {
        Sound_Manager.instance.Play("Deck_Shuffel");
        for (int index = 0; index < cardList.Count; index++)
        {
            int randomIndex = Random.Range(index, cardList.Count);
            CardData temporaryCard = cardList[index];
            cardList[index] = cardList[randomIndex];
            cardList[randomIndex] = temporaryCard;
        }
    }

    public void DrawInitialCards()
    {
        if (hasDrawnInitialHand)
        {
            return;
        }
        hasDrawnInitialHand = true;
        DrawExtraCards(drawCount);
    }

    public void DrawExtraCards(int amountToDraw)
    {
        if (deck.Count == 0 && discardPile.Count > 0)
        {
            deck.AddRange(discardPile);
            discardPile.Clear();
            Shuffle(deck);
        }

        int cardsToDraw = Mathf.Min(amountToDraw, deck.Count);

        for (int index = 0; index < cardsToDraw; index++)
        {
            if (deck.Count > 0)
            {
                hand.Add(deck[0]);
                deck.RemoveAt(0);
            }
        }
        UpdateAllUI();
    }

    public void OnDeckClicked()
    {
        if (hand.Count > 0)
        {
            discardPile.AddRange(hand);
            hand.Clear();
            Sound_Manager.instance.Play("Discard");
        }

        DrawExtraCards(drawCount);
        Sound_Manager.instance.Play("Deck_Shuffel");
    }

    public void MoveToZone(CardData card, DropType type)
    {
        if (isPlayingCard)
        {
            return;
        }

        if ((type == DropType.Left && leftZone.Contains(card)) || (type == DropType.Right && rightZone.Contains(card)))
        {
            UpdateAllUI();
            return;
        }

        isPlayingCard = true;

        hand.Remove(card);

        switch (type)
        {
            case DropType.Left:
                leftZone.Add(card);
                break;
            case DropType.Right:
                rightZone.Add(card);
                break;
            case DropType.Discard:
                discardPile.Add(card);
                Sound_Manager.instance.Play("Discard");
                break;
            default:
                hand.Add(card);
                break;
        }

        UpdateAllUI();
        StartCoroutine(StartPlayCooldown());

        if (type == DropType.Left || type == DropType.Right)
        {
            StartCoroutine(AutoDiscard(card, type));
        }
    }

    private IEnumerator StartPlayCooldown()
    {
        yield return new WaitForSeconds(playCooldown);
        isPlayingCard = false;
    }

    private IEnumerator AutoDiscard(CardData card, DropType fromZone)
    {
        yield return new WaitForSeconds(autoDiscardDelay);

        if (fromZone == DropType.Left)
        {
            leftZone.Remove(card);
        }
        else if (fromZone == DropType.Right)
        {
            rightZone.Remove(card);
        }

        discardPile.Add(card);
        UpdateAllUI();
        Sound_Manager.instance.Play("Discard");
    }

    public void UpdateAllUI()
    {
        if (handGrid != null) RebuildContainer(handGrid, hand, true);
        if (leftGrid != null) RebuildContainer(leftGrid, leftZone, false);
        if (rightGrid != null) RebuildContainer(rightGrid, rightZone, false);
        if (discardGrid != null) RebuildContainer(discardGrid, discardPile, false);
    }

    private void RebuildContainer(Transform parentTransform, List<CardData> cardsToDisplay, bool isDraggable)
    {
        for (int index = parentTransform.childCount - 1; index >= 0; index--)
        {
            Destroy(parentTransform.GetChild(index).gameObject);
        }

        foreach (var cardData in cardsToDisplay)
        {
            GameObject cardObject = null;
            if (cardData.cardPrefab != null)
            {
                cardObject = Instantiate(cardData.cardPrefab, parentTransform);
            }
            else if (cardPrefab != null)
            {
                cardObject = Instantiate(cardPrefab, parentTransform);
            }

            if (cardObject == null)
            {
                continue;
            }

            if (parentTransform == discardGrid)
            {
                cardObject.transform.localScale *= 0.5f;
            }

            CardUI cardUserInterface = cardObject.GetComponent<CardUI>();
            if (cardUserInterface != null)
            {
                cardUserInterface.Initialize(cardData);
            }

            if (isDraggable)
            {
                if (cardObject.TryGetComponent<CardDragHandler>(out var dragHandler))
                {
                    dragHandler.Card = cardData;
                }
            }
            else
            {
                if (cardObject.TryGetComponent<CardDragHandler>(out var dragHandler))
                {
                    Destroy(dragHandler);
                }
            }
        }
    }
}