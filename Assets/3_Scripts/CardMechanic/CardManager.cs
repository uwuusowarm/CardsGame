using System.Collections;
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

    private List<CardData> deck = new List<CardData>();
    private List<CardData> hand = new List<CardData>();
    private List<CardData> discardPile = new List<CardData>();
    private List<CardData> leftZone = new List<CardData>();
    private List<CardData> rightZone = new List<CardData>();
    public RectTransform HandGridRect => handGrid as RectTransform;
    public RectTransform LeftGridRect => leftGrid as RectTransform;
    public RectTransform RightGridRect => rightGrid as RectTransform;
    public RectTransform DiscardGridRect => discardGrid as RectTransform;

    public void DrawInitialCards() => DrawCards(drawCount);
    public void DrawCard() => DrawCards(drawCount);
    public void DrawCard(int c) => DrawCards(c);

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

    public void MoveToZone(CardData card, DropType zone)
    {
        hand.Remove(card);

        switch (zone)
        {
            case DropType.Left:
                leftZone.Add(card);
                StartCoroutine(ZoneCooldown(card, DropType.Left));
                GameManager.Instance.ProcessPlayedCard(card, true);

                break;
            case DropType.Right:
                rightZone.Add(card);
                StartCoroutine(ZoneCooldown(card, DropType.Right));
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

    private IEnumerator ZoneCooldown(CardData card, DropType zone)
    {
        yield return new WaitForSeconds(3f);

        if (zone == DropType.Left) 
            leftZone.Remove(card);

        else rightZone.Remove(card);

        discardPile.Add(card);
        UpdateAllUI();
    }

    private void UpdateAllUI()
    {
        UpdateHandUI();
        UpdateZoneUI(leftGrid, leftZone, false, hide: false);
        UpdateZoneUI(rightGrid, rightZone, false, hide: false);
        UpdateZoneUI(discardGrid, discardPile, false, hide: false);
    }

    private void UpdateHandUI()
    {
        foreach (Transform t in handGrid) 
            Destroy(t.gameObject);
        foreach (var c in hand)
        {
            var go = Instantiate(cardUIPrefab, handGrid);

            go.GetComponent<CardUI>().Initialize(c);

            if (go.TryGetComponent<CardDragHandler>(out var d))
                d.Card = c;
        }
    }

    private void UpdateZoneUI(Transform grid, List<CardData> cards, bool draggable, bool hide)
    {
        foreach (Transform t in grid) 
            Destroy(t.gameObject);
        foreach (var c in cards)
        {
            var go = Instantiate(cardUIPrefab, grid);

            go.GetComponent<CardUI>().Initialize(c);

            if (draggable && go.TryGetComponent<CardDragHandler>(out var d))
                d.Card = c;

            else if (go.TryGetComponent<CardDragHandler>(out var d2))
                Destroy(d2);

            if (hide)
            {
                var cg = go.GetComponent<CanvasGroup>() ?? go.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                cg.blocksRaycasts = false;
            }

            go.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }
    }
    public void UpdateDiscardUI()
    {
        UpdateZoneUI(discardGrid, discardPile, false, hide: false);
    }
}
