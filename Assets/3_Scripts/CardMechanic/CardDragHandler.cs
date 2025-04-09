using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CardState))]
public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private Vector2 originalPosition;
    private Canvas parentCanvas;
    private CardState cardState; 

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        cardState = GetComponent<CardState>(); 

        if (cardState == null) 
        {
             Debug.LogError("CardDragHandler requires a CardState component!", this);
        }
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        parentCanvas = GetComponentInParent<Canvas>();
        if(parentCanvas == null)
        {
            Debug.LogError("CardDragHandler requires a Canvas component in its parent hierarchy!", this);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (cardState != null)
        {
            DropType currentZone = cardState.currentZone;
            if (currentZone == DropType.Player1Ablage ||
                currentZone == DropType.Player2Ablage ||
                currentZone == DropType.Graveyard)
            {
                Debug.Log($"Dragging not allowed from zone: {currentZone}.");
                eventData.pointerDrag = null; 
                return; 
            }
           
        }
        else 
        {
             Debug.LogError("Cannot check card state - CardState component missing!", this);
             eventData.pointerDrag = null;
             return;
        }


        if (rectTransform == null || canvasGroup == null || parentCanvas == null) return;

        cardState.StopGraveyardTimer(); 

        Debug.Log("Drag gestartet (from allowed zone): " + gameObject.name);
        originalParent = transform.parent;
        originalPosition = rectTransform.anchoredPosition;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
        transform.SetParent(parentCanvas.transform, true);
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.pointerDrag != gameObject) return; 
        if (rectTransform == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform,
            eventData.position,
            parentCanvas.worldCamera,
            out Vector2 localPointerPosition);
        rectTransform.localPosition = localPointerPosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.pointerDrag != gameObject && eventData.pointerDrag != null)
        {
             Debug.LogWarning("OnEndDrag called, but pointerDrag is not this object.");
             if(canvasGroup != null)
             {
                 canvasGroup.blocksRaycasts = true;
                 canvasGroup.alpha = 1f;
             }
            return;
        }
        if (eventData.pointerDrag == null && canvasGroup != null) {
             canvasGroup.blocksRaycasts = true;
             canvasGroup.alpha = 1f;
             return;
        }


        if (canvasGroup == null) return;
        Debug.Log("Drag beendet: " + gameObject.name);
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        GameObject pointerEnterObj = eventData.pointerEnter;
        DropZone dropZone = null;
        bool droppedOnValidZone = false;

        if (pointerEnterObj != null)
        {
            dropZone = pointerEnterObj.GetComponent<DropZone>();
            if (dropZone != null)
            {
                if (dropZone.zoneType == DropType.Player1Ablage ||
                    dropZone.zoneType == DropType.Player2Ablage ||
                    dropZone.zoneType == DropType.Hand)
                {
                    droppedOnValidZone = true;
                    Debug.Log($"Auf gültiger Zone ({dropZone.zoneType}) abgelegt: {dropZone.gameObject.name}");
                }
                else
                {
                     Debug.Log($"Ablegen auf Zone {dropZone.zoneType} nicht erlaubt.");
                }
            }
        }

        if (!droppedOnValidZone)
        {
            Debug.Log("Nicht auf gültiger DropZone abgelegt, kehre zurück.");
            transform.SetParent(originalParent, false);
            rectTransform.anchoredPosition = originalPosition;
            rectTransform.localRotation = Quaternion.identity;
            rectTransform.localScale = Vector3.one;
        }
    }
}