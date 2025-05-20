using UnityEngine;
using UnityEngine.EventSystems;

public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public CardData Card;

    RectTransform rectTransform;
    CanvasGroup canvasGroup;
    Canvas canvas;
    Vector2 originalPosition;
    Transform originalParent;
    RectTransform handRect;
    RectTransform leftRect;
    RectTransform rightRect;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
        var mgr = CardManager.Instance;
        handRect = mgr.HandGridRect;
        leftRect = mgr.LeftGridRect;
        rightRect = mgr.RightGridRect;
    }

    public void OnBeginDrag(PointerEventData e)
    {
        originalParent = transform.parent;
        originalPosition = rectTransform.anchoredPosition;
        transform.SetParent(canvas.transform, true);
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData e)
    {
        rectTransform.position = e.position;
    }

    public void OnEndDrag(PointerEventData e)
    {
        canvasGroup.blocksRaycasts = true;

        bool inLeft = RectTransformUtility.RectangleContainsScreenPoint(
            leftRect, e.position,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : canvas.worldCamera
        );
        bool inRight = RectTransformUtility.RectangleContainsScreenPoint(
            rightRect, e.position,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : canvas.worldCamera
        );

        if (inLeft)
        {
            CardManager.Instance.MoveToZone(Card, DropType.Left);
            GameManager.Instance.ProcessPlayedCard(Card, true);
            Destroy(gameObject);
        }
        else if (inRight)
        {
            CardManager.Instance.MoveToZone(Card, DropType.Right);
            GameManager.Instance.ProcessPlayedCard(Card, false);
            Destroy(gameObject);
        }
        else
        {
            CardManager.Instance.MoveToZone(Card, DropType.Hand);
            Destroy(gameObject);
        }
    }

}
