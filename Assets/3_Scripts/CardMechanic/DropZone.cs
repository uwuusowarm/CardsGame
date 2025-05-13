using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform), typeof(Image))]
public class DropZone : MonoBehaviour, IDropHandler
{
    [Tooltip("Wähle, ob diese Zone für Hand oder Ablagestapel ist")]
    public DropType zoneType = DropType.Hand;

    public void OnDrop(PointerEventData eventData)
    {
        GameObject go = eventData.pointerDrag;
        if (go == null) return;

        var ui = go.GetComponent<CardUI>();
        if (ui == null) return;

        var data = ui.GetCardData();

        _CardManager.Instance.MoveToZone(data, zoneType);

        go.transform.SetParent(transform);
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;
    }
}
