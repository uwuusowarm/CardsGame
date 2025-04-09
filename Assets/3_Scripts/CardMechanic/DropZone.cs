using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform), typeof(UnityEngine.UI.Graphic))]
public class DropZone : MonoBehaviour, IDropHandler
{
    public DropType zoneType = DropType.Hand; 

    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedCard = eventData.pointerDrag;
        if (droppedCard == null) return;

        if (zoneType != DropType.Player1Ablage &&
            zoneType != DropType.Player2Ablage &&
            zoneType != DropType.Hand)
        {
            Debug.Log($"Ablegen auf Zone '{gameObject.name}' vom Typ '{zoneType}' nicht erlaubt.");
            return;
        }

        Debug.Log($"Karte '{droppedCard.name}' auf erlaubter Zone '{gameObject.name}' Typ '{zoneType}' abgelegt.");

        CardState cardState = droppedCard.GetComponent<CardState>();
        if (cardState != null)
        {
            cardState.currentZone = this.zoneType; 
             Debug.Log($"'{droppedCard.name}' Status aktualisiert auf Zone: {cardState.currentZone}");

            if (zoneType == DropType.Player1Ablage || zoneType == DropType.Player2Ablage)
            {
                 cardState.StartDelayedMoveToGraveyard(2.0f); 
            }
        }
        else
        {
            Debug.LogWarning($"Abgelegte Karte '{droppedCard.name}' hat keine CardState Komponente!", droppedCard);
        }

        droppedCard.transform.SetParent(transform);
        RectTransform cardRect = droppedCard.GetComponent<RectTransform>();
        if (cardRect != null)
        {
            cardRect.anchoredPosition = Vector2.zero;
            cardRect.localRotation = Quaternion.identity;
            cardRect.localScale = Vector3.one;
        }
    }
}