using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnityEngine.UI;

[System.Serializable]
public class HandLayoutSettings
{
    public float _maxCardRotation;
    public float _cardHeightDisplacement;
    public float _cardSpacing;
    public float _hoverScaleMultiplier = 1.2f;
}

public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; set; }

    [Header("Databese")]
    [SerializeField] CardDatabaseSO cardDatabase;

    [Header("Settings")]
    [SerializeField, Min(1)] int _drawCount = 4;
    public int _DrawCount => _drawCount;

    [Header("Hand Layout Settings")]
    [SerializeField] Transform handTransform;

    [Header("Playzones")]
    [SerializeField] Transform leftGrid;
    [SerializeField] Transform rightGrid;
    [SerializeField] Transform discardGrid;
    [SerializeField] GameObject cardPrefab;

    [Header("Gameplay")]
    [SerializeField] float autoDiscardDelay = 1f;

    List<CardData> deck = new List<CardData>();
    List<CardData> hand = new List<CardData>();
    List<CardData> leftZone = new List<CardData>();
    List<CardData> rightZone = new List<CardData>();
    List<CardData> discardPile = new List<CardData>();
    List<CardDragHandler> handCardObjects = new List<CardDragHandler>();
    bool hasDrawnHand = false;
    bool isPlayingCard = false;

    public RectTransform HandGrid => handTransform as RectTransform;
    public RectTransform LeftGrid => leftGrid as RectTransform;
    public RectTransform RightGrid => rightGrid as RectTransform;
    public RectTransform DiscardGrid => discardGrid as RectTransform;



    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else Instance = this;
    }

    void Start()
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
                //useless atm spammed mir nur die console voll
                // Debug.LogError(" CardDatabaseSO ist nicht zugewiesen du idiot"); 
                return;
            }
            deck.Clear();
            deck.AddRange(cardDatabase._allCards);
        }
        Shuffle(deck);
        DrawInitialCards();
    }

    void Update()
    {
        UpdateHandLayout();
    }

    void UpdateHandLayout()
    {
        if (handCardObjects.Count == 0)
            return;

        HandLayoutSettings currentSettings = null;

        switch (handCardObjects.Count)
        {
            case 4:
                currentSettings = new HandLayoutSettings
                {
                    _maxCardRotation = 20f,
                    _cardHeightDisplacement = 15f,
                    _cardSpacing = 70f,
                    _hoverScaleMultiplier = 1.5f
                };
                break;
            case 3:
                currentSettings = new HandLayoutSettings
                {
                    _maxCardRotation = 20f,
                    _cardHeightDisplacement = 15f,
                    _cardSpacing = 70f,
                    _hoverScaleMultiplier = 1.5f
                };
                break;
            case 2:
                currentSettings = new HandLayoutSettings
                {
                    _maxCardRotation = 10f,
                    _cardHeightDisplacement = 0f,
                    _cardSpacing = 70f,
                    _hoverScaleMultiplier = 1.5f
                };
                break;
            case 1:
                currentSettings = new HandLayoutSettings
                {
                    _maxCardRotation = 0f,
                    _cardHeightDisplacement = 0f,
                    _cardSpacing = 0f,
                    _hoverScaleMultiplier = 1.5f
                };
                break;
        }


        if (currentSettings == null)
            return;

        float HandWidth = (handCardObjects.Count - 1) * currentSettings._cardSpacing;
        float start = -(HandWidth / 2f);

        for (int abc123 = 0; abc123 < handCardObjects.Count; abc123++)
        {
            CardDragHandler cardHandler = handCardObjects[abc123];
            if (cardHandler == null)
                continue;

            cardHandler._hoverScaleMultiplier = currentSettings._hoverScaleMultiplier;

            if (cardHandler.IsBeingDragged())
                continue;

            float horizontPosition = start + (abc123 * currentSettings._cardSpacing);
            float normalPosition = (handCardObjects.Count > 1) ? (float)abc123 / (handCardObjects.Count - 1) : 0.5f;
            float verticPosition = Mathf.Sin(normalPosition * Mathf.PI) * currentSettings._cardHeightDisplacement;
            float Angle = Mathf.Lerp(currentSettings._maxCardRotation, -currentSettings._maxCardRotation, normalPosition);

            if (handCardObjects.Count <= 1)
                Angle = 0;

            cardHandler._targetPosition = handTransform.position + new Vector3(horizontPosition, verticPosition, 0);
            cardHandler._targetRotation = Quaternion.Euler(0, 0, Angle);
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

    System.Collections.IEnumerator AutoDiscard(CardData card, GameObject cardObject, DropType fromZone)
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

    void RebuildZoneContainer(Transform parent, List<CardData> list)
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
        DrawExtraCards(_drawCount);
    }

    public void DrawExtraCards(int amountToDraw)
    {
        if (deck.Count == 0 && discardPile.Count > 0)
        {
            deck.AddRange(discardPile);
            discardPile.Clear();
            Shuffle(deck);

            if (ExhaustionSystem.Instance != null)
            {
                ExhaustionSystem.Instance.AddExhaustionStack();
                ExhaustionSystem.Instance.ExhaustCards(deck);
            }
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

    void InstantiateCardInHand(CardData cardData)
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

            //wer tf macht die ganze zeit den soundmanager irgendwo rein

            //Sound Manager
            if (Sound_Manager.instance != null)
                Sound_Manager.instance.Play("Discard");

            DrawExtraCards(_drawCount);
            //Sound Manager
            if (Sound_Manager.instance != null)
                Sound_Manager.instance.Play("Deck_Shuffel");
        }
    }

    void Shuffle(List<CardData> cardList)
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