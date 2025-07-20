using UnityEngine;
using UnityEngine.EventSystems;

public class CardDragHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public CardData Card { get; set; }

    [HideInInspector] public Vector3 targetPosition;
    [HideInInspector] public Quaternion targetRotation;

    private RectTransform rectTransform;
    private bool isHovered = false;
    private bool isDragging = false;
    private Vector2 dragStartPositionOffset;
    private Canvas canvas;
    private int defaultSortOrder;

    private RectTransform leftZone;
    private RectTransform rightZone;
    private RectTransform discardZone;

    private const float ROTATION_SPEED = 12f;
    private const float SCALE_SPEED = 8f;
    private const float HOVER_SCALE_MULTIPLIER = 1.2f;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        if (CardManager.Instance != null)
        {
            leftZone = CardManager.Instance.LeftGridRect;
            rightZone = CardManager.Instance.RightGridRect;
            discardZone = CardManager.Instance.DiscardGridRect;
        }
    }

    void Update()
    {
        if (isDragging)
        {
            rectTransform.position = (Vector2)Input.mousePosition + dragStartPositionOffset;
            rectTransform.rotation = Quaternion.identity;
        }
        else
        {
            if (Vector3.Distance(rectTransform.position, targetPosition) > 0.1f)
                rectTransform.position = Vector3.Lerp(rectTransform.position, targetPosition, Time.deltaTime * 10f);
            else
                rectTransform.position = targetPosition;

            rectTransform.rotation = Quaternion.Slerp(rectTransform.rotation, targetRotation, Time.deltaTime * ROTATION_SPEED);
        }

        float targetScale = isHovered || isDragging ? HOVER_SCALE_MULTIPLIER : 1f;
        Vector3 newScale = Vector3.one * Mathf.Lerp(rectTransform.localScale.x, targetScale, Time.deltaTime * SCALE_SPEED);
        rectTransform.localScale = newScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isDragging) 
            return;
        isHovered = true;
        defaultSortOrder = rectTransform.GetSiblingIndex();
        rectTransform.SetAsLastSibling();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isDragging) 
            return;
        isHovered = false;
        rectTransform.SetSiblingIndex(defaultSortOrder);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
        isHovered = false;
        dragStartPositionOffset = (Vector2)rectTransform.position - eventData.position;
        rectTransform.SetAsLastSibling();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;

        if (leftZone != null && RectTransformUtility.RectangleContainsScreenPoint(leftZone, eventData.position, canvas.worldCamera))
        {
            GameManager.Instance.ProcessPlayedCard(Card, true);
        }
        else if (rightZone != null && RectTransformUtility.RectangleContainsScreenPoint(rightZone, eventData.position, canvas.worldCamera))
        {
            GameManager.Instance.ProcessPlayedCard(Card, false);
        }
        else if (discardZone != null && RectTransformUtility.RectangleContainsScreenPoint(discardZone, eventData.position, canvas.worldCamera))
        {
            CardManager.Instance.MoveToZone(Card, DropType.Discard);
        }
    }

    public bool IsBeingDragged()
    {
        return isDragging;
    }
}