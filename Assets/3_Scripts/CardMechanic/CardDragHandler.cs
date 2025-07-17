using UnityEngine;
using UnityEngine.EventSystems;

public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public CardData Card;

    private bool isInMenu = false;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas gameCanvas;
    private Vector2 startPosition;
    private Transform startParent;

    private RectTransform handZone;
    private RectTransform leftZone;
    private RectTransform rightZone;
    private RectTransform discardZone;

    void Awake()
    {
        isInMenu = FindObjectOfType<CardMenuManager>() != null;
        if (isInMenu)
        {
            return;
        }

        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        gameCanvas = GetComponentInParent<Canvas>();

        var cardManager = CardManager.Instance;
        handZone = cardManager.HandGridRect;
        leftZone = cardManager.LeftGridRect;
        rightZone = cardManager.RightGridRect;
        discardZone = cardManager.DiscardGridRect;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isInMenu)
        {
            return;
        }

        Sound_Manager.instance.PlayRandomFromGroup("Draw_Card");

        startParent = transform.parent;
        startPosition = rectTransform.anchoredPosition;

        transform.SetParent(gameCanvas.transform, true);

        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isInMenu)
        {
            return;
        }
        rectTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isInMenu)
        {
            return;
        }

        canvasGroup.blocksRaycasts = true;

        bool droppedOnLeft = RectTransformUtility.RectangleContainsScreenPoint(
            leftZone, eventData.position,
            gameCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : gameCanvas.worldCamera
        );

        bool droppedOnRight = RectTransformUtility.RectangleContainsScreenPoint(
            rightZone, eventData.position,
            gameCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : gameCanvas.worldCamera
        );

        if ((droppedOnLeft || droppedOnRight) && ActionPointSystem.Instance.GetCurrentActionPoints() <= 0)
        {
            Debug.Log("Not enough action points!");
            CardManager.Instance.MoveToZone(Card, DropType.Hand);
            Destroy(gameObject);
            return;
        }

        bool droppedOnDiscard = RectTransformUtility.RectangleContainsScreenPoint(
            discardZone, eventData.position,
            gameCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : gameCanvas.worldCamera
        );

        if (droppedOnLeft)
        {
            CardManager.Instance.MoveToZone(Card, DropType.Left);
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ProcessPlayedCard(Card, true);
            }
        }
        else if (droppedOnRight)
        {
            CardManager.Instance.MoveToZone(Card, DropType.Right);
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ProcessPlayedCard(Card, false);
            }
        }
        else if (droppedOnDiscard)
        {
            CardManager.Instance.MoveToZone(Card, DropType.Discard);
            Sound_Manager.instance.Play("Discard");
        }
        else
        {
            CardManager.Instance.MoveToZone(Card, DropType.Hand);
        }

        Destroy(gameObject);
    }
}