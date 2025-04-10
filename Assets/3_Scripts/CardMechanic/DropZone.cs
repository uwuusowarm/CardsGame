using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform), typeof(UnityEngine.UI.Graphic))]
public class DropZone : MonoBehaviour, IDropHandler
{
    public DropType zoneType = DropType.Hand;

    public void OnDrop(PointerEventData eventData)
    {
        GameObject card = eventData.pointerDrag;
        if (card == null || zoneType != DropType.Player2Ablage) return;

        HighlightAllEnemies();

        card.transform.SetParent(transform);
        card.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        StartCoroutine(MoveToGraveyardAfterDelay(card, 2f));
    }

    private void HighlightAllEnemies()
    {
        foreach (EnemyUnit enemy in FindObjectsOfType<EnemyUnit>())
        {
            enemy.ToggleHighlight();
        }
    }

    private System.Collections.IEnumerator MoveToGraveyardAfterDelay(GameObject card, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (card != null)
        {
            GameObject graveyard = GameObject.FindWithTag("Graveyard");
            if (graveyard != null)
            {
                card.transform.SetParent(graveyard.transform);
                card.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }

            // Highlight aller Gegner entfernen
            foreach (EnemyUnit enemy in FindObjectsOfType<EnemyUnit>())
            {
                if (enemy != null)
                    enemy.ToggleHighlight();
            }
        }
    }
}