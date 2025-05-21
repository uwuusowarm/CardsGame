using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum DropType { Hand, Left, Right, Discard }

public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; }

    [SerializeField, Min(1)] private int drawCount = 4;
    [SerializeField] private List<CardData> cardDatabase;
    [SerializeField] private Transform handGrid;
    [SerializeField] private Transform discardGrid;
    [SerializeField] private Transform leftGrid;
    [SerializeField] private Transform rightGrid;
    [SerializeField] private GameObject cardUIPrefab;
    [SerializeField] private Button deckButton;

    private List<CardData> deck = new();
    private List<CardData> hand = new();
    private List<CardData> discardPile = new();
    private List<CardData> leftZone = new();
    private List<CardData> rightZone = new();

    public RectTransform HandGridRect => handGrid as RectTransform;
    public RectTransform LeftGridRect => leftGrid as RectTransform;
    public RectTransform RightGridRect => rightGrid as RectTransform;
    public RectTransform DiscardGridRect => discardGrid as RectTransform;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        deckButton.onClick.AddListener(OnDeckClicked);
    }

    private void Start()
    {
        deck = new List<CardData>(cardDatabase);
        Shuffle(deck);
        UpdateAllUI();
    }

    public void DrawInitialCards() => DrawCards(drawCount);
    public void DrawCard() => DrawCards(drawCount);
    public void DrawCard(int urmom) => DrawCards(urmom);
    public void OnDeckClicked()
    {
        discardPile.AddRange(hand);
        discardPile.AddRange(leftZone);
        discardPile.AddRange(rightZone);

        hand.Clear();
        leftZone.Clear();
        rightZone.Clear();

        if (deck.Count < drawCount && discardPile.Count > 0)
        {
            deck.AddRange(discardPile);
            discardPile.Clear();
            Shuffle(deck);
        }

        DrawCards(drawCount);
        UpdateAllUI();
    }

    private void DrawCards(int count)
    {
        for (int i = 0; i < count && deck.Count > 0; i++)
        {
            hand.Add(deck[0]);
            deck.RemoveAt(0);
        }
    }

    private void Shuffle(List<CardData> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }

    public void MoveToZone(CardData card, DropType zone)
    {
        hand.Remove(card);

        switch (zone)
        {
            case DropType.Left:
                discardPile.AddRange(rightZone);
                discardPile.AddRange(leftZone);
                rightZone.Clear();
                leftZone.Clear();
                leftZone.Add(card);
                GameManager.Instance.ProcessPlayedCard(card, true);

                break;

            case DropType.Right:
                discardPile.AddRange(leftZone);
                discardPile.AddRange(rightZone);
                leftZone.Clear();
                rightZone.Clear();
                rightZone.Add(card);
                GameManager.Instance.ProcessPlayedCard(card, false);
                break;

            case DropType.Discard:
                discardPile.Add(card);
                break;

            default:
                hand.Add(card);
                break;
        }

        UpdateAllUI();
        
    }

    public void UpdateDiscardUI()
    {
        UpdateZoneUI(discardGrid, discardPile, false, disableRaycast: true);
    }

    private void UpdateAllUI()
    {
        UpdateZoneUI(handGrid, hand, true);
        UpdateZoneUI(leftGrid, leftZone, false);
        UpdateZoneUI(rightGrid, rightZone, false);
        UpdateZoneUI(discardGrid, discardPile, false, disableRaycast: true);
      
    }

    private void UpdateZoneUI(Transform grid, List<CardData> cards, bool draggable, bool disableRaycast = false)
    {
        foreach (Transform child in grid)
            Destroy(child.gameObject);

        foreach (var card in cards)
        {
            var cardUI = Instantiate(cardUIPrefab, grid);
            cardUI.GetComponent<CardUI>().Initialize(card);

            if (draggable)
            {
                if (cardUI.TryGetComponent<CardDragHandler>(out var handler))
                    handler.Card = card;
            }
            else
            {
                if (cardUI.TryGetComponent<CardDragHandler>(out var handler))
                    Destroy(handler);

                cardUI.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }

            if (disableRaycast)
            {
                var canvasGroup = cardUI.GetComponent<CanvasGroup>() ?? cardUI.AddComponent<CanvasGroup>();
                canvasGroup.blocksRaycasts = false;
            }
        }
    }
}
