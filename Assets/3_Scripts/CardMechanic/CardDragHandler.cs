using UnityEngine;
using UnityEngine.EventSystems;

public class CardDragHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public CardData Card { get; set; }

    [HideInInspector] public Vector3 _targetPosition;
    [HideInInspector] public Quaternion _targetRotation;
    [HideInInspector] public float _hoverScaleMultiplier = 1.2f;

    RectTransform rectTransform;
    bool isHovered = false;
    bool isDragging = false;
    int defaultSortOrder;
    Vector2 dragStartPositionOffset;
    Canvas canvas;
    
    RectTransform leftZone;
    RectTransform rightZone;
    RectTransform discardZone;

    const float ROTATION_SPEED = 12f;
    const float SCALE_SPEED = 8f;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        if (CardManager.Instance != null)
        {
            leftZone = CardManager.Instance.LeftGrid;
            rightZone = CardManager.Instance.RightGrid;
            discardZone = CardManager.Instance.DiscardGrid;
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
            if (Vector3.Distance(rectTransform.position, _targetPosition) > 0.1f)
                rectTransform.position = Vector3.Lerp(rectTransform.position, _targetPosition, Time.deltaTime * 10f);
            else rectTransform.position = _targetPosition;

            rectTransform.rotation = Quaternion.Slerp(rectTransform.rotation, _targetRotation, Time.deltaTime * ROTATION_SPEED);
        }

        float targetScale = isHovered || isDragging ? _hoverScaleMultiplier : 1f;

        Vector3 newScale = Vector3.one * Mathf.Lerp(rectTransform.localScale.x, targetScale, Time.deltaTime * SCALE_SPEED);
        rectTransform.localScale = newScale;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;

        if (ActionPointSystem.Instance != null && !ActionPointSystem.Instance.CanUseActionPoints(1))
        {
            CardManager.Instance.MoveToZone(Card, DropType.Hand);
            return;
        }

        if (leftZone != null && RectTransformUtility.RectangleContainsScreenPoint(leftZone, eventData.position, canvas.worldCamera))
        {
            CardManager.Instance.MoveToZone(Card, DropType.Left);
            GameManager.Instance.ProcessPlayedCard(Card, true);
        }
        else if (rightZone != null && RectTransformUtility.RectangleContainsScreenPoint(rightZone, eventData.position, canvas.worldCamera))
        {
            CardManager.Instance.MoveToZone(Card, DropType.Right);
            GameManager.Instance.ProcessPlayedCard(Card, false);
        }
        else if (discardZone != null && RectTransformUtility.RectangleContainsScreenPoint(discardZone, eventData.position, canvas.worldCamera))
        {
            CardManager.Instance.MoveToZone(Card, DropType.Discard);
        }
        else
        {
            CardManager.Instance.MoveToZone(Card, DropType.Hand);
        }
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
    public bool IsBeingDragged()
    {
        return isDragging;
    }
}