using UnityEngine;
using UnityEngine.EventSystems;

public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public CardData Card;

    private bool isInMenu = false;

    RectTransform rt;
    CanvasGroup cg;
    Canvas gameCanvas;
    Vector2 startPos;
    Transform startParent;

    RectTransform handZone;
    RectTransform leftZone;
    RectTransform rightZone;
    RectTransform discardZone;

    void Awake()
    {
        isInMenu = FindObjectOfType<CardMenuManager>() != null;
        
        if (isInMenu) return;

        rt = GetComponent<RectTransform>();
        cg = GetComponent<CanvasGroup>();
        gameCanvas = GetComponentInParent<Canvas>();

        var grab = CardManager.Instance;
        handZone = grab.HandGridRect;
        leftZone = grab.LeftGridRect;
        rightZone = grab.RightGridRect;
        discardZone = grab.DiscardGridRect;
    }

    public void OnBeginDrag(PointerEventData e)
    {
        if (isInMenu) return;

        Sound_Manager.instance.PlayRandomFromGroup("Draw_Card");


        startParent = transform.parent;
        startPos = rt.anchoredPosition;

        transform.SetParent(gameCanvas.transform, true);

        cg.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData e)
    {
        if (isInMenu) return;
        rt.position = e.position;
        
    }

    public void OnEndDrag(PointerEventData e)
    {
        if (isInMenu) return;

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

        bool droppedOnDiscard = RectTransformUtility.RectangleContainsScreenPoint(
            discardZone, e.position,
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
        }
        else
        {
            CardManager.Instance.MoveToZone(Card, DropType.Hand);
        }

        Destroy(gameObject);
    }
}