using UnityEngine;
using UnityEngine.EventSystems;

public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public CardData Card;

    private RectTransform rt;
    private CanvasGroup cg;
    private Canvas gameCanvas;
    private Vector2 startPos;
    private Transform startParent;

    private RectTransform handZone;
    private RectTransform leftZone;
    private RectTransform rightZone;
    private RectTransform discardZone;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        cg = GetComponent<CanvasGroup>();
        gameCanvas = GetComponentInParent<Canvas>();

        if (CardManager.Instance != null)
        {
            var grab = CardManager.Instance;
            handZone = grab.HandGridRect;
            leftZone = grab.LeftGridRect;
            rightZone = grab.RightGridRect;
            discardZone = grab.DiscardGridRect;
        }
    }

    public void OnBeginDrag(PointerEventData e)
    {
        if (CardManager.Instance == null) return;

        startParent = transform.parent;
        startPos = rt.anchoredPosition;

        transform.SetParent(gameCanvas.transform, true);

        cg.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData e)
    {
        if (CardManager.Instance == null) return;

        rt.position = e.position;
    }

    public void OnEndDrag(PointerEventData e)
    {
        if (CardManager.Instance == null) return;

        cg.blocksRaycasts = true;

        bool droppedOnLeft = RectTransformUtility.RectangleContainsScreenPoint(
            leftZone, e.position,
            gameCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : gameCanvas.worldCamera
        );

        bool droppedOnRight = RectTransformUtility.RectangleContainsScreenPoint(
            rightZone, e.position,
            gameCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : gameCanvas.worldCamera
        );

        bool droppedOnDiscard = discardZone != null && RectTransformUtility.RectangleContainsScreenPoint(
            discardZone, e.position,
            gameCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : gameCanvas.worldCamera
        );

        if (droppedOnLeft)
        {
            CardManager.Instance.MoveToZone(Card, DropType.Left);
        }
        else if (droppedOnRight)
        {
            CardManager.Instance.MoveToZone(Card, DropType.Right);
        }
        else if (droppedOnDiscard)
        {
            CardManager.Instance.MoveToZone(Card, DropType.Discard);
        }
        else
        {
            CardManager.Instance.MoveToZone(Card, DropType.Hand);
        }

        Destroy(gameObject);
    }
}