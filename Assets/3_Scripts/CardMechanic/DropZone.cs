using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    public DropType zoneType;

    public void OnDrop(PointerEventData eventData)
    {
        var go = eventData.pointerDrag;
        if (go == null) 
            return;

        var drag = go.GetComponent<CardDragHandler>();
        if (drag == null) 
            return;

        _CardManager.Instance.MoveToZone(drag.Card, zoneType);
        drag.DropAccepted = true;
    }
}
