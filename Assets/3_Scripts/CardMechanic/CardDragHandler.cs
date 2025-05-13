using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // WIRD VOM _CardManager BESETZT:
    // Referenz auf das CardData-Objekt, das diese UI repräsentiert
    public CardData Card;

    // interne Referenzen
    private RectTransform _rectTransform;
    private CanvasGroup _canvasGroup;
    private Canvas _canvas;
    private Vector2 _originalPosition;
    private Transform _originalParent;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
        // Canvas suchen, um während Drag über alle UI-Elemente zu liegen
        _canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // merken, wo die Karte herkommt
        _originalPosition = _rectTransform.anchoredPosition;
        _originalParent = transform.parent;

        // raus aus der Grid-Hierarchie, damit sie über allem liegt
        transform.SetParent(_canvas.transform, true);
        // Raycasts temporär aussetzen, damit DropZones die Karte sehen
        _canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Karte folgt dem Pointer
        _rectTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Raycasts wieder einschalten
        _canvasGroup.blocksRaycasts = true;

        // Wenn die Karte nicht in eine neue DropZone gezogen wurde, zurücksetzen
        if (transform.parent == _canvas.transform || transform.parent == null)
        {
            transform.SetParent(_originalParent, true);
            _rectTransform.anchoredPosition = _originalPosition;
        }
    }
}
