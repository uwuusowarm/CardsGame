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
    RectTransform leftRect;
    RectTransform rightRect;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
        handRect = CardManager.Instance.HandGridRect;
        leftRect = CardManager.Instance.LeftGridRect;
        rightRect = CardManager.Instance.RightGridRect;
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
        
        bool insideLeft = RectTransformUtility.RectangleContainsScreenPoint(
            leftRect,
            e.position,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : canvas.worldCamera
        );
        
        bool insideRight = RectTransformUtility.RectangleContainsScreenPoint(
            rightRect,
            e.position,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : canvas.worldCamera
        );

        if (insideLeft)
        {
            CardManager.Instance.MoveToZone(Card, DropType.Left);
            GameManager.Instance.ProcessPlayedCard(Card, true);
        }
        else if (insideRight)
        {
            CardManager.Instance.MoveToZone(Card, DropType.Right);
            GameManager.Instance.ProcessPlayedCard(Card, false);
        }
        else
        {
            CardManager.Instance.MoveToZone(Card, DropType.Hand);
        }

        Destroy(gameObject);
    }
}
