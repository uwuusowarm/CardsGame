using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; }

    [SerializeField] private List<CardData> cardDatabase;
    [SerializeField, Min(1)] private int drawCount = 4;

    [SerializeField] private Transform handGrid;
    [SerializeField] private Transform discardGrid;
    [SerializeField] private Transform leftGrid;
    [SerializeField] private Transform rightGrid;
    [SerializeField] private GameObject cardUIPrefab;
    [SerializeField] private Button deckButton;

    private List<CardData> deck = new List<CardData>();
    private List<CardData> hand = new List<CardData>();
    private List<CardData> discard = new List<CardData>();
    private List<CardData> left = new List<CardData>();
    private List<CardData> right = new List<CardData>();


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
//        deckButton.onClick.AddListener(OnDeckClicked);
    }

    private void Start()
    {
        deck = new List<CardData>(cardDatabase);
        Shuffle(deck);
        UpdateHandUI();
        UpdateDiscardUI();
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

    public void OnDeckClicked()
    {
        if (hand.Count > 0)
        {
            discard.AddRange(hand);
            hand.Clear();
            UpdateDiscardUI();
        }
        DrawCard(drawCount);
    }

    public void DrawCard(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (deck.Count == 0)
            {
                deck.AddRange(discard);
                discard.Clear();
                Shuffle(deck);
                UpdateDiscardUI();
                if (deck.Count == 0) break;
            }
            var card = deck[0];
            deck.RemoveAt(0);
            hand.Add(card);
        }
        UpdateHandUI();
    }

    public void DrawInitialCards()
    {
        DrawCard(drawCount);
    }
    public void MoveToZone(CardData card, DropType zone)
    {
        if (hand.Contains(card)) 
            hand.Remove(card);

        if (zone == DropType.Discard)
            discard.Add(card);
        else if (zone == DropType.Left)
            left.Add(card);
        else if (zone == DropType.Right)
            right.Add(card);
        else
            hand.Add(card);

        UpdateHandUI();
        UpdateRightLeftUI();
        UpdateDiscardUI();
    }
    
    private void UpdateRightLeftUI()
    {
        foreach (Transform t in leftGrid) Destroy(t.gameObject);
        foreach (var card in left)
        {
            var go = Instantiate(cardUIPrefab, leftGrid);
            var ui = go.GetComponent<CardUI>();
            ui.Initialize(card);
            var drag = go.GetComponent<CardDragHandler>();
            if (drag != null) drag.Card = card;
        }
        
        foreach (Transform t in rightGrid) Destroy(t.gameObject);
        foreach (var card in right)
        {
            var go = Instantiate(cardUIPrefab, rightGrid);
            var ui = go.GetComponent<CardUI>();
            ui.Initialize(card);
            var drag = go.GetComponent<CardDragHandler>();
            if (drag != null) drag.Card = card;       
        }
    }
    
    private void UpdateHandUI()
    {
        foreach (Transform t in handGrid) Destroy(t.gameObject);
        foreach (var card in hand)
        {
            var go = Instantiate(cardUIPrefab, handGrid);
            var ui = go.GetComponent<CardUI>();
            ui.Initialize(card);
            var drag = go.GetComponent<CardDragHandler>();
            if (drag != null) drag.Card = card;
        }
    }

    public void UpdateDiscardUI()
    {
        if (discardGrid == null) return;
        foreach (Transform t in discardGrid) Destroy(t.gameObject);
        foreach (var card in discard)
        {
            var go = Instantiate(cardUIPrefab, discardGrid);
            var ui = go.GetComponent<CardUI>();
            ui.Initialize(card);
            var drag = go.GetComponent<CardDragHandler>();
            if (drag != null) Destroy(drag);
        }
    }

    public RectTransform HandGridRect => handGrid as RectTransform;
    public RectTransform LeftGridRect => leftGrid as RectTransform;
    public RectTransform RightGridRect => rightGrid as RectTransform;
    public RectTransform DiscardGridRect => discardGrid as RectTransform;
}
