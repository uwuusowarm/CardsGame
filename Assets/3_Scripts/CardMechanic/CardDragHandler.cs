using UnityEngine;
using UnityEngine.EventSystems;

public class CardDragHandler : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public CardData Card;

    RectTransform rectTransform;
    CanvasGroup canvasGroup;
    Canvas canvas;
    Vector2 originalPosition;
    Transform originalParent;
    RectTransform handRect;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
        handRect = _CardManager.Instance.HandGridRect;
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

        bool insideHand = RectTransformUtility.RectangleContainsScreenPoint(
            handRect,
            e.position,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : canvas.worldCamera
        );

        if (insideHand)
            _CardManager.Instance.MoveToZone(Card, DropType.Hand);
        else
            _CardManager.Instance.MoveToZone(Card, DropType.Discard);

        Destroy(gameObject);
    }
}
