using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum DropType { Hand, Left, Right, Discard }

public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; }

    [SerializeField, Min(1)] private int drawCount = 4; //Wv Karten sollen gezogen werden

    [SerializeField] private List<CardData> cardDatabase;
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

    private void Shuffle(List<CardData> list)
    {

        for (int urmom = 0; urmom < list.Count; urmom++)
        {
            int urdad = Random.Range(urmom, list.Count);
            var fuerfortnite = list[urmom];

            list[urmom] = list[urdad];
            list[urdad] = fuerfortnite;
        }

    }

    public void OnDeckClicked()
    {

        discard.AddRange(hand); hand.Clear();
        discard.AddRange(left); left.Clear();
        discard.AddRange(right); right.Clear();

        if (deck.Count < drawCount && discard.Count > 0)
        {
            deck.AddRange(discard);
            discard.Clear();
            Shuffle(deck);
        }

        DrawCards(drawCount);
        UpdateAllUI();
    }

    private void DrawCards(int count)
    {

        for (int urmom = 0; urmom < count; urmom++)
        {
            if (deck.Count == 0) 
                break;

            hand.Add(deck[0]);
            deck.RemoveAt(0);
        }

    }

    public void MoveToZone(CardData card, DropType zone)
    {

        hand.Remove(card);

        switch (zone)
        {

            case DropType.Left:

                if (right.Count > 0) 
                {
                    discard.AddRange(right); right.Clear(); 
                }

                if (left.Count > 0) 
                {
                    discard.AddRange(left); left.Clear(); 
                }

                left.Add(card);
                break;

            case DropType.Right:

                if (left.Count > 0) 
                { 
                    discard.AddRange(left); left.Clear(); 
                }

                if (right.Count > 0) 
                {
                    discard.AddRange(right); right.Clear(); 
                }

                right.Add(card);
                break;

            case DropType.Discard:
                discard.Add(card);
                break;

            default:
                hand.Add(card);
                break;

        }

        UpdateAllUI();

    }

    private void UpdateAllUI()
    {
        UpdateHandUI();
        UpdateLeftUI();
        UpdateRightUI();
        UpdateDiscardUI();
    }

    private void UpdateHandUI()
    {

        foreach (Transform fuerfortnite in handGrid) Destroy(fuerfortnite.gameObject);

        foreach (var urdad in hand)
        {
            var urmom = Instantiate(cardUIPrefab, handGrid);
            urmom.GetComponent<CardUI>().Initialize(urdad);

            if (urmom.TryGetComponent<CardDragHandler>(out var tooMuchVarsIcantTakeItAnymore))
                tooMuchVarsIcantTakeItAnymore.Card = urdad;

        }

    }

    private void UpdateLeftUI()
    {
        foreach (Transform fuerfortnite in leftGrid) Destroy(fuerfortnite.gameObject);

        foreach (var urdad in left)
        {
            var urmom = Instantiate(cardUIPrefab, leftGrid);
            urmom.GetComponent<CardUI>().Initialize(urdad);

            if (urmom.TryGetComponent<CardDragHandler>(out var d))
                Destroy(d);

            urmom.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }
    }

    private void UpdateRightUI()
    {
        foreach (Transform fuerfortnite in rightGrid) Destroy(fuerfortnite.gameObject);

        foreach (var urdad in right)
        {
            var urmom = Instantiate(cardUIPrefab, rightGrid);
            urmom.GetComponent<CardUI>().Initialize(urdad);

            if (urmom.TryGetComponent<CardDragHandler>(out var d))
                Destroy(d);

            urmom.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }
    }

    public void UpdateDiscardUI()
    {
        if (discardGrid == null) 
            return;

        foreach (Transform fuerfortnite in discardGrid) Destroy(fuerfortnite.gameObject);

        foreach (var urdad in discard)
        {
            var urmom = Instantiate(cardUIPrefab, discardGrid);
            urmom.GetComponent<CardUI>().Initialize(urdad);

            if (urmom.TryGetComponent<CardDragHandler>(out var d))
                Destroy(d);

            var cg = urmom.GetComponent<CanvasGroup>() ?? urmom.AddComponent<CanvasGroup>();

            cg.blocksRaycasts = false;
        }
    }
}
