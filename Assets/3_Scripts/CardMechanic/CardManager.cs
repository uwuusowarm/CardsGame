using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum DropType { Hand, Left, Right, Discard }

public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; }

    [Header("Einstellungen")]
    [SerializeField, Min(1)] private int drawCount = 4;
    [SerializeField] private List<CardData> cardDatabase;

    [Header("UI-Referenzen")]
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

    public RectTransform HandGridRect => handGrid as RectTransform;
    public RectTransform LeftGridRect => leftGrid as RectTransform;
    public RectTransform RightGridRect => rightGrid as RectTransform;
    public RectTransform DiscardGridRect => discardGrid as RectTransform;

    private bool isPlaying = false;
    private bool hasDrawnStartHand = false;
    public void DrawCard() => DrawCards(1);
    
    public int DrawCount => drawCount;

    public void DrawCard(int count) => DrawCards(count);

    private void Awake()
    {
        if (Instance != null && Instance != this) 
            Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        deck.Clear();
        deck.AddRange(cardDatabase);
        Shuffle(deck);
        DrawInitialCards();
        UpdateAllUI();
    }

    private void Shuffle(List<CardData> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(i, list.Count);
            var tmp = list[i];
            list[i] = list[r];
            list[r] = tmp;
        }
    }

    public void DrawInitialCards()
    {
        if (hasDrawnStartHand) 
            return;
        hasDrawnStartHand = true;
        Debug.Log("Drawing " + drawCount + " cards for the start of the game.");
        DrawCards(drawCount);
    }

    public void DrawCards(int count)
    {
        if (hand.Count > 0) return;

        if (deck.Count < count)
        {
            int remainingCards = deck.Count;
            for (int i = 0; i < remainingCards; i++)
            {
                hand.Add(deck[0]);
                deck.RemoveAt(0);
            }

            if (discardPile.Count > 0)
            {
                deck.AddRange(discardPile);
                discardPile.Clear();
                Shuffle(deck);
                ExhaustionSystem.Instance.AddExhaustionStack();
                ExhaustionSystem.Instance.ExhaustCards(deck);
                
                int additionalCards = Mathf.Min(count - remainingCards, deck.Count);
                for (int i = 0; i < additionalCards; i++)
                {
                    hand.Add(deck[0]);
                    deck.RemoveAt(0);
                }
            }
        }
        else
        {
            for (int i = 0; i < count; i++)
            {
                hand.Add(deck[0]);
                deck.RemoveAt(0);
            }
        }
        
        UpdateAllUI();
    }

    public void OnDeckClicked()
    {
        DrawCards(drawCount);
    }

    public void MoveToZone(CardData card, DropType type)
    {
        if ((type == DropType.Left && leftZone.Contains(card)) ||
            (type == DropType.Right && rightZone.Contains(card)))
        {
            Debug.Log($"[CardManager] Karte '{card.cardName}' wurde bereits in Zone '{type}' gelegt.");
            UpdateAllUI();
            return;
        }
        
        if ((type == DropType.Left || type == DropType.Right) && 
            ActionPointSystem.Instance.GetCurrentActionPoints() <= 0)
        {
            Debug.Log("Not enough action points to play this card!");
            hand.Add(card); 
            UpdateAllUI();
            return;
        }

        if (isPlaying) return;
        isPlaying = true;


        int idx = hand.IndexOf(card);
        if (idx >= 0) hand.RemoveAt(idx);

        switch (type)
        {
            case DropType.Left:
                leftZone.Add(card);
                GameManager.Instance.ProcessPlayedCard(card, true);
                break;
            case DropType.Right:
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
        StartCoroutine(PlayCooldown());

        if (type == DropType.Left || type == DropType.Right)
            StartCoroutine(AutoDiscard(card, type));
    }

    private IEnumerator PlayCooldown()
    {
        yield return new WaitForSeconds(playCooldown);
        isPlaying = false;
    }

    private IEnumerator AutoDiscard(CardData card, DropType fromZone)
    {
        yield return new WaitForSeconds(autoDiscardDelay);
        switch (fromZone)
        {
            case DropType.Left:
                leftZone.Remove(card);
                break;
            case DropType.Right:
                rightZone.Remove(card);
                break;
        }
        discardPile.Add(card);
        UpdateAllUI();
    }

    public void UpdateAllUI()
    {
        RebuildContainer(handGrid, hand, true);
        RebuildContainer(leftGrid, leftZone, false);
        RebuildContainer(rightGrid, rightZone, false);
        RebuildContainer(discardGrid, discardPile, false);
    }

    private void RebuildContainer(Transform parent, List<CardData> list, bool draggable)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
            DestroyImmediate(parent.GetChild(i).gameObject);

        foreach (var c in list)
        {
            var go = Instantiate(cardPrefab, parent);
            go.GetComponent<CardUI>().Initialize(c);
            if (draggable && go.TryGetComponent<CardDragHandler>(out var d))
                d.Card = c;
            else if (!draggable && go.TryGetComponent<CardDragHandler>(out var d2))
                Destroy(d2);
            go.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }
    }
}
