using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class CardSelectorUI : MonoBehaviour
{
    [SerializeField] private GameObject highlightOverlay;
    [SerializeField] private Transform cardParent;

    private CardData assignedCard;
    private CardMenuManager manager;

    public void Initialize(CardData card, CardMenuManager menuManager, bool isSelected)
    {
        this.assignedCard = card;
        this.manager = menuManager;

        foreach (Transform child in cardParent)
        {
            Destroy(child.gameObject);
        }

        if (cardParent != null && assignedCard.cardPrefab != null)
        {
            GameObject visualCardInstance = Instantiate(assignedCard.cardPrefab, cardParent);

            // --- FINALE LÖSUNG ZUR DEAKTIVIERUNG ALLER INTERAKTIONEN ---

            // 1. Zerstöre den DragHandler komplett, anstatt ihn nur zu deaktivieren.
            CardDragHandler dragHandler = visualCardInstance.GetComponentInChildren<CardDragHandler>(true);
            if (dragHandler != null)
            {
                Destroy(dragHandler);
            }

            // 2. Deaktiviere die Raycast-Blockade der Canvas Group.
            CanvasGroup canvasGroup = visualCardInstance.GetComponentInChildren<CanvasGroup>(true);
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = false;
            }

            // 3. Deaktiviere "Raycast Target" auf ALLEN grafischen Elementen (Images, Text etc.) der Karte.
            // Dies ist der aggressivste und sicherste Schritt, um die Karte "durchklickbar" zu machen.
            Graphic[] graphics = visualCardInstance.GetComponentsInChildren<Graphic>(true);
            foreach (Graphic g in graphics)
            {
                g.raycastTarget = false;
            }

            // --- ENDE FINALE LÖSUNG ---

            ScaleCardToFit(visualCardInstance, cardParent);
        }

        highlightOverlay.SetActive(isSelected);

        GetComponent<Button>().onClick.RemoveAllListeners();
        GetComponent<Button>().onClick.AddListener(OnCardClicked);
    }

    private void ScaleCardToFit(GameObject cardInstance, Transform parentContainer)
    {
        RectTransform cardRect = cardInstance.GetComponent<RectTransform>();
        RectTransform parentRect = parentContainer.GetComponent<RectTransform>();

        if (cardRect == null || parentRect == null)
        {
            Debug.LogError("RectTransform für Skalierung nicht gefunden!");
            return;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);

        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.pivot = new Vector2(0.5f, 0.5f);

        float cardWidth = cardRect.rect.width;
        float cardHeight = cardRect.rect.height;

        float parentWidth = parentRect.rect.width;
        float parentHeight = parentRect.rect.height;

        if (cardWidth == 0 || cardHeight == 0) return;

        float scaleRatio = Mathf.Min(parentWidth / cardWidth, parentHeight / cardHeight);

        cardRect.localScale = new Vector3(scaleRatio, scaleRatio, 1f);
        cardRect.anchoredPosition = Vector2.zero;
    }

    private void OnCardClicked()
    {
        manager.ToggleCardSelection(assignedCard);
        highlightOverlay.SetActive(!highlightOverlay.activeSelf);
    }
}