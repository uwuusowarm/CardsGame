using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnityEngine.UI;

[System.Serializable]
public class HandLayoutSettings
{
    public float maxCardRotation;
    public float cardHeightDisplacement;
    public float cardSpacing;
    public float hoverScaleMultiplier = 1.2f;
}

public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; }

    [Header("Databese")]
    [SerializeField] private CardDatabaseSO cardDatabase;

    [Header("Settings")]
    [SerializeField, Min(1)] private int drawCount = 4;
    public int DrawCount => drawCount;

    [Header("Hand Layout Settings")]
    [SerializeField] private Transform handTransform;

    [SerializeField] private HandLayoutSettings layout4Cards;
    [SerializeField] private HandLayoutSettings layout3Cards;
    [SerializeField] private HandLayoutSettings layout2Cards;
    [SerializeField] private HandLayoutSettings layout1Card;

    [Header("Playzones")]
    [SerializeField] private Transform leftGrid;
    [SerializeField] private Transform rightGrid;
    [SerializeField] private Transform discardGrid;
    [SerializeField] private GameObject cardPrefab;

    [Header("Gameplay")]
    [SerializeField] private float autoDiscardDelay = 1f;

    private List<CardData> deck = new List<CardData>();
    private List<CardData> hand = new List<CardData>();
    private List<CardData> leftZone = new List<CardData>();
    private List<CardData> rightZone = new List<CardData>();
    private List<CardData> discardPile = new List<CardData>();
    private List<CardDragHandler> handCardObjects = new List<CardDragHandler>();
    private bool hasDrawnHand = false;
    private bool isPlayingCard = false;

    public RectTransform HandGrid => handTransform as RectTransform;
    public RectTransform LeftGrid => leftGrid as RectTransform;
    public RectTransform RightGrid => rightGrid as RectTransform;
    public RectTransform DiscardGrid => discardGrid as RectTransform;



    private void Awake()
    {
        if (Instance != null && Instance != this) 
            Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        if (GameDataManager.Instance != null && GameDataManager.Instance.selectedDeck != null)
        {
            deck.Clear();
            deck.AddRange(GameDataManager.Instance.selectedDeck.Cards);
        }
        else
        {
            if (cardDatabase == null) 
            {
//                Debug.LogError(" CardDatabaseSO ist nicht zugewiesen"); 
                return; 
            }
            deck.Clear();
            deck.AddRange(cardDatabase.allCards);
        }
        Shuffle(deck);
        DrawInitialCards();
    }

    private void Update()
    {
        UpdateHandLayout();
    }

    private void UpdateHandLayout()
    {
        if (handCardObjects.Count == 0) 
            return;

        HandLayoutSettings currentSettings = null;
        switch (handCardObjects.Count)
        {
            case 4: currentSettings = layout4Cards; 
                break;
            case 3: currentSettings = layout3Cards; 
                break;
            case 2: currentSettings = layout2Cards; 
                break;
            case 1: currentSettings = layout1Card; 
                break;
        }

        if (currentSettings == null) 
            return;

        float HandWidth = (handCardObjects.Count - 1) * currentSettings.cardSpacing;
        float start = -(HandWidth / 2f);

        for (int abc123 = 0; abc123 < handCardObjects.Count; abc123++)
        {
            CardDragHandler cardHandler = handCardObjects[abc123];
            if (cardHandler == null) 
                continue;

            cardHandler.hoverScaleMultiplier = currentSettings.hoverScaleMultiplier;

            if (cardHandler.IsBeingDragged()) 
                continue;

            float horizontPosition = start + (abc123 * currentSettings.cardSpacing);
            float normalPosition = (handCardObjects.Count > 1) ? (float)abc123 / (handCardObjects.Count - 1) : 0.5f;

            float verticPosition = Mathf.Sin(normalPosition * Mathf.PI) * currentSettings.cardHeightDisplacement;
            float Angle = Mathf.Lerp(currentSettings.maxCardRotation, -currentSettings.maxCardRotation, normalPosition);
            if (handCardObjects.Count <= 1) 
                Angle = 0;

            cardHandler.targetPosition = handTransform.position + new Vector3(horizontPosition, verticPosition, 0);
            cardHandler.targetRotation = Quaternion.Euler(0, 0, Angle);
        }
    }

    public void MoveToZone(CardData card, DropType type)
    {
        if (isPlayingCard) 
            return;

        CardDragHandler handlerToRemove = handCardObjects.FirstOrDefault(h => h.Card == card);
        if (handlerToRemove == null) 
            return;

        hand.Remove(card);
        handCardObjects.Remove(handlerToRemove);

        switch (type)
        {
            case DropType.Left:
                leftZone.Add(card);
                handlerToRemove.transform.SetParent(leftGrid);
                StartCoroutine(AutoDiscard(card, handlerToRemove.gameObject, DropType.Left));
                break;
            case DropType.Right:
                rightZone.Add(card);
                handlerToRemove.transform.SetParent(rightGrid);
                StartCoroutine(AutoDiscard(card, handlerToRemove.gameObject, DropType.Right));
                break;
            case DropType.Discard:
                discardPile.Add(card);
                Destroy(handlerToRemove.gameObject);
                UpdateAllUI();
                break;
            default: // Hand
                hand.Add(card);
                handCardObjects.Add(handlerToRemove);
                break;
        }
    }

    private System.Collections.IEnumerator AutoDiscard(CardData card, GameObject cardObject, DropType fromZone)
    {
        if (cardObject != null)
        {
            if (cardObject.TryGetComponent<CardDragHandler>(out var dragHandler)) 
                Destroy(dragHandler);
            cardObject.transform.rotation = Quaternion.identity;
            cardObject.transform.localScale = Vector3.one;
            cardObject.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }

        yield return new WaitForSeconds(autoDiscardDelay);

        if (fromZone == DropType.Left) 
            leftZone.Remove(card);
        else if (fromZone == DropType.Right) 
            rightZone.Remove(card);

        discardPile.Add(card);
        Destroy(cardObject);
        UpdateAllUI();
    }

    public void UpdateAllUI()
    {
        RebuildZoneContainer(leftGrid, leftZone);
        RebuildZoneContainer(rightGrid, rightZone);
        RebuildZoneContainer(discardGrid, discardPile);
    }

    private void RebuildZoneContainer(Transform parent, List<CardData> list)
    {
        if (parent == null) 
            return;
        foreach (Transform child in parent) 
            Destroy(child.gameObject);
        foreach (var cardData in list)
        {
            GameObject cardObject = (cardData.cardPrefab != null) ? Instantiate(cardData.cardPrefab, parent) : Instantiate(cardPrefab, parent);
            if (cardObject.TryGetComponent<CardUI>(out var cardUI)) 
                cardUI.Initialize(cardData);
            if (cardObject.TryGetComponent<CardDragHandler>(out var dragHandler)) 
                Destroy(dragHandler);
            if (parent == discardGrid) cardObject.transform.localScale = Vector3.one * 1f;
        }
    }

    public void DrawCard() 
    { 
        DrawExtraCards(1); 
    }
    public void DrawCard(int count) 
    {
        DrawExtraCards(count); 
    }
    public void DrawInitialCards() 
    { 
        if (hasDrawnHand) 
            return; 
        hasDrawnHand = true; 
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
        for (int i = 0; i < cardsToDraw; i++)
        {
            if (deck.Count > 0)
            {
                CardData newCardData = deck[0];
                deck.RemoveAt(0);
                hand.Add(newCardData);
                InstantiateCardInHand(newCardData);
            }
        }
    }

    private void InstantiateCardInHand(CardData cardData)
    {
        GameObject cardObject = (cardData.cardPrefab != null) ? Instantiate(cardData.cardPrefab, handTransform) : Instantiate(cardPrefab, handTransform);
        if (cardObject != null)
        {
            if (cardObject.TryGetComponent<CardUI>(out var cardUI)) 
                cardUI.Initialize(cardData);
            if (cardObject.TryGetComponent<CardDragHandler>(out var dragHandler)) 
            { 
                dragHandler.Card = cardData; handCardObjects.Add(dragHandler); 
            }
        }
    }

    public void OnDeckClicked()
    {
        if (hand.Count >= 0 && ActionPointSystem.Instance.GetCurrentActionPoints() > 0)
        {
            discardPile.AddRange(hand);
            hand.Clear();
            ActionPointSystem.Instance.UseActionPoints(1);
            foreach (var cardObj in handCardObjects) 
                Destroy(cardObj.gameObject);
            handCardObjects.Clear();
//Sound Manager
            if (Sound_Manager.instance != null) 
                Sound_Manager.instance.Play("Discard");

            DrawExtraCards(drawCount);
//Sound Manager
            if (Sound_Manager.instance != null) 
                Sound_Manager.instance.Play("Deck_Shuffel");
        }
    }

    private void Shuffle(List<CardData> cardList) 
    { 
//Sound Manager
        if (Sound_Manager.instance != null) 
            Sound_Manager.instance.Play("Deck_Shuffel"); 
        for (int card = 0; card < cardList.Count; card++) 
        { 
            int GetMeOuttaThisFuckingShuffleHell = Random.Range(card, cardList.Count); 
            var tmp = cardList[card]; 
            cardList[card] = cardList[GetMeOuttaThisFuckingShuffleHell]; 
            cardList[card] = tmp; 
        } 
    }
}