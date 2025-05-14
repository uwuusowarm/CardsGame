using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class _CardManager : MonoBehaviour
{
    public static _CardManager Instance { get; private set; }

    [SerializeField] private List<CardData> cardDatabase;
    [SerializeField, Min(1)] private int drawCount = 4;

    [SerializeField] private Transform handGrid;
    [SerializeField] private Transform discardGrid;
    [SerializeField] private GameObject cardUIPrefab;
    [SerializeField] private Button deckButton;

    private List<CardData> deck = new List<CardData>();
    private List<CardData> hand = new List<CardData>();
    private List<CardData> discard = new List<CardData>();

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
        InitializeDeck();
        UpdateHandUI();
        UpdateDiscardUI();
    }

    private void InitializeDeck()
    {
        deck = new List<CardData>(cardDatabase);
        Shuffle(deck);
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
        
        DrawCards(drawCount);
    }

    private void DrawCards(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (deck.Count == 0)
            {
                RefillDeckFromDiscard();
                
                if (deck.Count == 0)
                    break;
            }

            var card = deck[0];
            deck.RemoveAt(0);
            hand.Add(card);
        }

        UpdateHandUI();
    }

    private void RefillDeckFromDiscard()
    {
        deck.AddRange(discard);
        discard.Clear();
        Shuffle(deck);
        UpdateDiscardUI();
    }

    public void MoveToZone(CardData card, DropType zone)
    {
        deck.Remove(card);
        hand.Remove(card);
        discard.Remove(card);

        if (zone == DropType.Hand)
            hand.Add(card);
        else if (zone == DropType.Discard)
            discard.Add(card);

        UpdateHandUI();
        UpdateDiscardUI();
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

            if (drag != null) 
                drag.Card = card;
        }
    }

    private void UpdateDiscardUI()
    {
        if (discardGrid == null)
            return;

        foreach (Transform t in discardGrid) Destroy(t.gameObject);
        foreach (var card in discard)
        {
            var go = Instantiate(cardUIPrefab, discardGrid);
            var ui = go.GetComponent<CardUI>();
            ui.Initialize(card);
            var drag = go.GetComponent<CardDragHandler>();

            if (drag != null) 
                Destroy(drag);
        }
    }
}
