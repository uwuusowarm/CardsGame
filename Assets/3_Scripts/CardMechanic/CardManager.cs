using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.MaterialProperty;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; }

    [Header("Zentrale Kartendatenbank")]
    [Tooltip("Die ScriptableObject-Datei, die die Master-Liste aller Karten enthält.")]
    [SerializeField] private CardDatabaseSO cardDatabase;

    [Header("Einstellungen")]
    [SerializeField, Min(1)] private int drawCount = 4;
    public int DrawCount => drawCount;

    [Header("UI-Referenzen (Nur für Spiel-Szene)")]
    [Tooltip("Die UI-Elemente, die als Zonen für die Karten im Spiel dienen.")]
    [SerializeField] private Transform handGrid;
    [SerializeField] private Transform leftGrid;
    [SerializeField] private Transform rightGrid;
    [SerializeField] private Transform discardGrid;
    [SerializeField] private GameObject cardPrefab;

    [Header("Gameplay")]
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
            Debug.Log("Lade ausgewähltes Deck: " + GameDataManager.Instance.selectedDeck.DeckName);
            deck.Clear();
            deck.AddRange(GameDataManager.Instance.selectedDeck.Cards);
        }
        else
        {
            Debug.LogWarning("Kein Deck ausgewählt. Lade alle Karten aus der Datenbank als Fallback.");
            if (cardDatabase == null)
            {
                Debug.LogError("FATAL: CardDatabaseSO wurde nicht im CardManager zugewiesen!");
                return;
            }
            deck.Clear();
            deck.AddRange(cardDatabase.allCards);
        }

        Shuffle(deck);
        DrawInitialCards();
        UpdateAllUI();
    }

    public void DrawCard() => DrawCards(1);
    public void DrawCard(int count) => DrawCards(count);

    private void Shuffle(List<CardData> list)
    {
        Sound_Manager.instance.Play("Deck_Shuffel");
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
        if (hasDrawnStartHand) return;
        hasDrawnStartHand = true;
        DrawCards(drawCount);
    }

    public void DrawCards(int count)
    {
        if (hand.Count > 0) return;

        if (deck.Count == 0 && discardPile.Count > 0)
        {
            deck.AddRange(discardPile);
            discardPile.Clear();
            Shuffle(deck);
        }

        int toDraw = Mathf.Min(count, deck.Count);

        for (int i = 0; i < toDraw; i++)
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
        
        DrawCards(drawCount);
        Sound_Manager.instance.Play("Deck_Shuffel");
    }

    public void MoveToZone(CardData card, DropType type)
    {
        if ((type == DropType.Left && leftZone.Contains(card)) ||
            (type == DropType.Right && rightZone.Contains(card)))
        {
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
                break;
            case DropType.Right:
                rightZone.Add(card);
                break;
            case DropType.Discard:
                discardPile.Add(card);
                Sound_Manager.instance.Play("Discard");
                break;
            default: // Hand
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
        Sound_Manager.instance.Play("Discard");
    }

    public void UpdateAllUI()
    {
        if (handGrid != null) RebuildContainer(handGrid, hand, true);
        if (leftGrid != null) RebuildContainer(leftGrid, leftZone, false);
        if (rightGrid != null) RebuildContainer(rightGrid, rightZone, false);
        if (discardGrid != null) RebuildContainer(discardGrid, discardPile, false);
    }

    private void RebuildContainer(Transform parent, List<CardData> list, bool draggable)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
            Destroy(parent.GetChild(i).gameObject);

        foreach (var cardData in list)
        {
            GameObject cardObject = null;
            if (cardData.cardPrefab != null)
            {
                cardObject = Instantiate(cardData.cardPrefab, parent);
            }
            else if (cardPrefab != null)
            {
                cardObject = Instantiate(cardPrefab, parent);
            }
            else
            {
                continue;
            }
            
            if (parent == discardGrid)
            {
                cardObject.transform.localScale *= 0.5f;
            }
            
            var cardUI = cardObject.GetComponent<CardUI>();
            if (cardUI != null)
            {
                cardUI.Initialize(cardData);
            }

            if (draggable)
            {
                if (cardObject.TryGetComponent<CardDragHandler>(out var dragHandler))
                    dragHandler.Card = cardData;
            }
            else
            {
                if (cardObject.TryGetComponent<CardDragHandler>(out var dragHandler))
                    Destroy(dragHandler);
            }

            cardObject.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }
    }
}