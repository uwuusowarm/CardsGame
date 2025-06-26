using UnityEngine;
using UnityEngine.EventSystems;

public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public CardData Card;

    RectTransform rt;
    CanvasGroup cg;
    Canvas gameCanvas;
    Vector2 startPos;
    Transform startParent;

    RectTransform handZone;
    RectTransform leftZone;
    RectTransform rightZone;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        cg = GetComponent<CanvasGroup>();
        gameCanvas = GetComponentInParent<Canvas>();

        var grab = CardManager.Instance;
        handZone = grab.HandGridRect;
        leftZone = grab.LeftGridRect;
        rightZone = grab.RightGridRect;
    }

    public void OnBeginDrag(PointerEventData e)
    {
        startParent = transform.parent;
        startPos = rt.anchoredPosition;

        transform.SetParent(gameCanvas.transform, true);

        cg.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData e)
    {
        rt.position = e.position;
    }

    public void OnEndDrag(PointerEventData e)
    {
        cg.blocksRaycasts = true;

        bool droppedOnLeft = RectTransformUtility.RectangleContainsScreenPoint(
            leftZone, e.position,
            gameCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : gameCanvas.worldCamera
        );

        bool droppedOnRight = RectTransformUtility.RectangleContainsScreenPoint(
            rightZone, e.position,
            gameCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : gameCanvas.worldCamera
        );
        
        if ((droppedOnLeft || droppedOnRight) && ActionPointSystem.Instance.GetCurrentActionPoints() <= 0)
        {
            Debug.Log("Not enough action points to play this card!");
            CardManager.Instance.MoveToZone(Card, DropType.Hand);
            Destroy(gameObject);
            return;
        }


        if (droppedOnLeft)
        {
            CardManager.Instance.MoveToZone(Card, DropType.Left);
        }
        else if (droppedOnRight)
        {
            CardManager.Instance.MoveToZone(Card, DropType.Right);
        }
        else
        {
            CardManager.Instance.MoveToZone(Card, DropType.Hand);
        }

        Destroy(gameObject);
    }
}
