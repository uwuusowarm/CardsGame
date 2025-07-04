using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class CardSelectorUI : MonoBehaviour
{
    [SerializeField] private GameObject highlightOverlay;
    [SerializeField] private Transform cardParent;

    private CardData assignedCard;
    private CardMenuManager manager;

    public void Initialize(CardData card, GameObject visualPrefab, CardMenuManager menuManager, bool isSelected)
    {
        this.assignedCard = card;
        this.manager = menuManager;

        foreach (Transform child in cardParent)
        {
            Destroy(child.gameObject);
        }

        if (cardParent != null && visualPrefab != null)
        {
            GameObject visualCardInstance = Instantiate(visualPrefab, cardParent);

            TextMeshProUGUI[] texts = visualCardInstance.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var text in texts)
            {
                Material editableMaterial = new Material(text.fontMaterial);
                editableMaterial.EnableKeyword("MASK_HARD");
                text.fontMaterial = editableMaterial;
            }

            CardDragHandler dragHandler = visualCardInstance.GetComponentInChildren<CardDragHandler>(true);
            if (dragHandler != null) Destroy(dragHandler);

            CanvasGroup canvasGroup = visualCardInstance.GetComponentInChildren<CanvasGroup>(true);
            if (canvasGroup != null) canvasGroup.blocksRaycasts = false;

            Graphic[] graphics = visualCardInstance.GetComponentsInChildren<Graphic>(true);
            foreach (Graphic g in graphics) g.raycastTarget = false;

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
        if (cardRect == null || parentRect == null) return;
        LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
        cardRect.anchorMin = cardRect.anchorMax = cardRect.pivot = new Vector2(0.5f, 0.5f);
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