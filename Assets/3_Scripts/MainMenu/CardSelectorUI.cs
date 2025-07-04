using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

[RequireComponent(typeof(Button), typeof(LayoutElement))] 
public class CardSelectorUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Wrapper Referenzen")]
    [SerializeField] private GameObject highlightOverlay;
    [SerializeField] private Transform cardParent;

    [Header("Hover-Effekt Einstellungen")]
    [SerializeField] private float hoverScaleFactor = 1.1f;
    [SerializeField] private float scaleDuration = 0.1f;

    private CardData assignedCard;
    private CardMenuManager manager;
    private bool isCurrentlySelected = false;

    private Vector3 originalScale;
    private Coroutine scaleCoroutine;
    
    private LayoutElement layoutElement;

    private void Awake()
    {
        originalScale = transform.localScale;
        layoutElement = GetComponent<LayoutElement>();
    }

    public void Initialize(CardData card, CardMenuManager menuManager, bool isSelected, Vector2 targetSize)
    {
        this.assignedCard = card;
        this.manager = menuManager;
        this.isCurrentlySelected = isSelected;
        
        Graphic raycastTargetGraphic = GetComponent<Graphic>();
        if (raycastTargetGraphic != null)
        {
            raycastTargetGraphic.raycastTarget = true;
        }

        if (layoutElement != null)
        {
            layoutElement.ignoreLayout = false;
        }

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
    
    #region Unveränderte Methoden
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
        this.isCurrentlySelected = isHighlighted;
        if (highlightOverlay != null)
        {
            highlightOverlay.SetActive(this.isCurrentlySelected);
        }
    }

    public CardData GetCardData()
    {
        return this.assignedCard;
    }
    #endregion


    public void OnPointerEnter(PointerEventData eventData)
    {
        if (highlightOverlay != null)
        {
            highlightOverlay.SetActive(true);
        }

        StartScaling(originalScale * hoverScaleFactor);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (highlightOverlay != null)
        {
            highlightOverlay.SetActive(this.isCurrentlySelected);
        }

        StartScaling(originalScale);
    }
    
    private void StartScaling(Vector3 targetScale, System.Action onComplete = null)
    {
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
        }
        scaleCoroutine = StartCoroutine(ScaleRoutine(targetScale, onComplete));
    }

    private IEnumerator ScaleRoutine(Vector3 targetScale, System.Action onComplete)
    {
        float elapsedTime = 0f;
        Vector3 startingScale = transform.localScale;

        while (elapsedTime < scaleDuration)
        {
            transform.localScale = Vector3.Lerp(startingScale, targetScale, elapsedTime / scaleDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localScale = targetScale;
        onComplete?.Invoke();
    }

}