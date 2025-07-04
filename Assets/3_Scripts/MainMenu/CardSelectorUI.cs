using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class CardSelectorUI : MonoBehaviour
{
    [Header("Wrapper Referenzen")]
    [SerializeField] private GameObject highlightOverlay;
    [SerializeField] private Transform cardParent;

    private CardData assignedCard;
    private CardMenuManager manager;

    public void Initialize(CardData card, CardMenuManager menuManager, bool isSelected, Vector2 targetSize)
    {
        this.assignedCard = card;
        this.manager = menuManager;

        foreach (Transform child in cardParent)
        {
            Destroy(child.gameObject);
        }

        if (cardParent != null && card.cardPrefab != null)
        {
            GameObject visualCardInstance = Instantiate(card.cardPrefab, cardParent);
            TameCardInstance(visualCardInstance);
            ScaleCardToFit(visualCardInstance, cardParent, targetSize);
        }

        SetHighlight(isSelected);

        GetComponent<Button>().onClick.RemoveAllListeners();
        GetComponent<Button>().onClick.AddListener(OnCardClicked);
    }
    
    private void TameCardInstance(GameObject cardInstance)
    {
        if (cardInstance.TryGetComponent<CardDragHandler>(out var dragHandler))
        {
            Destroy(dragHandler);
        }

        if (cardInstance.TryGetComponent<CanvasGroup>(out var canvasGroup))
        {
            canvasGroup.blocksRaycasts = false;
        }

        TextMeshProUGUI[] texts = cardInstance.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var text in texts)
        {
            if (text.fontMaterial.name.Contains("_Masked")) continue; 

            Material editableMaterial = new Material(text.fontMaterial);
            editableMaterial.EnableKeyword("MASK_HARD");
            text.fontMaterial = editableMaterial;
        }
    }

    private void ScaleCardToFit(GameObject cardInstance, Transform parentContainer, Vector2 targetSize)
    {
        RectTransform cardRect = cardInstance.GetComponent<RectTransform>(); 
        RectTransform parentRect = parentContainer.GetComponent<RectTransform>();
        if (cardRect == null || parentRect == null) return;

        LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
        Canvas.ForceUpdateCanvases();

        cardRect.anchorMin = cardRect.anchorMax = cardRect.pivot = new Vector2(0.5f, 0.5f);
    
        Vector2 availableSpace = targetSize;

        float cardWidth = cardRect.rect.width;
        float cardHeight = cardRect.rect.height;

        if (cardWidth <= 0 || cardHeight <= 0) return;

        float scaleRatio = Mathf.Min(
            availableSpace.x / cardWidth,
            availableSpace.y / cardHeight
        );

        cardRect.localScale = new Vector3(scaleRatio, scaleRatio, 1f);
        cardRect.anchoredPosition = Vector2.zero;
    }

    private void OnCardClicked()
    {
        manager.ToggleCardSelection(assignedCard);
    }

    public void SetHighlight(bool isHighlighted)
    {
        if (highlightOverlay != null)
        {
            highlightOverlay.SetActive(isHighlighted);
        }
    }

    public CardData GetCardData()
    {
        return this.assignedCard;
    }
}