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
        // Quelle des Drags ist das Card-GameObject
        GameObject go = eventData.pointerDrag;
        if (go == null) return;

        // Wir erwarten, dass das Prefab ein CardUI hat
        var ui = go.GetComponent<CardUI>();
        if (ui == null) return;

        // Hole das CardData-Objekt
        var data = ui.GetCardData();

        // Notify CardManager
        _CardManager.Instance.MoveToZone(data, zoneType);

        // Reparent ins DropZone-Transform und zentrieren
        go.transform.SetParent(transform);
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;
    }
}
