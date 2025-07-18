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

    private const float POSITION_SPEED = 500f;
    private const float ROTATION_SPEED = 12f;
    private const float SCALE_SPEED = 8f;
    private const float HOVER_SCALE_MULTIPLIER = 1.2f;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    void Update()
    {
        if (isDragging)
        {
            rectTransform.position = (Vector2)Input.mousePosition + dragStartPositionOffset;
        }
        else
        {
            rectTransform.position = Vector3.Lerp(rectTransform.position, targetPosition, Time.deltaTime * POSITION_SPEED / Vector3.Distance(rectTransform.position, targetPosition));
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
    }

    public bool IsBeingDragged()
    {
        return isDragging;
    }
}