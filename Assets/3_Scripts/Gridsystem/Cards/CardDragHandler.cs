using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Drop Zones")]
    [SerializeField] private string leftZoneTag = "LeftDropZone";
    [SerializeField] private string rightZoneTag = "RightDropZone";

    private RectTransform leftActionZone;
    private RectTransform rightActionZone;
    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector2 startPosition;
    private Image cardImage;
    private bool isDragging;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        cardImage = GetComponent<Image>();
        canvas = GetComponentInParent<Canvas>();
        leftActionZone = GameObject.FindGameObjectWithTag(leftZoneTag)?.GetComponent<RectTransform>();
        rightActionZone = GameObject.FindGameObjectWithTag(rightZoneTag)?.GetComponent<RectTransform>();

        if (leftActionZone == null || rightActionZone == null)
        {
            Debug.LogError("DropZones nicht gefunden! Stellen Sie sicher, dass die Tags korrekt sind.");
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        isDragging = false;
        cardImage.raycastTarget = true;
        transform.localScale = Vector3.one;
        if (leftActionZone != null && rightActionZone != null)
        {
            bool isLeftAction = RectTransformUtility.RectangleContainsScreenPoint(leftActionZone, eventData.position);
            bool isRightAction = RectTransformUtility.RectangleContainsScreenPoint(rightActionZone, eventData.position);

            if (isLeftAction || isRightAction)
            {
                ActivateCardEffects(isLeftAction);
            }
        }

        rectTransform.anchoredPosition = startPosition;
    }

    private void ActivateCardEffects(bool isLeftAction)
    {
        CardUI cardUI = GetComponent<CardUI>();
        if (cardUI == null || cardUI.GetCardData() == null) return;

        var effects = isLeftAction ? cardUI.GetCardData().leftEffects : cardUI.GetCardData().rightEffects;

        foreach (var effect in effects)
        {
            if (effect == null) continue;

            switch (effect.effectType)
            {
                case CardEffect.EffectType.Move:
                    HexGrid.Instance?.AddMovementPoints(effect.value);
                    UnitManager.Instance?.ActivateMovement();
                    break;

                case CardEffect.EffectType.Attack:
                    if (UnitManager.Instance.SelectedUnit != null)
                    {
                        AttackManager.Instance.PrepareAttack(effect.value, effect.range);
                    }
                    break;
            }
        }

        CardManager.Instance?.DiscardCard(gameObject);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!UnitManager.Instance.PlayersTurn) return;

        startPosition = rectTransform.anchoredPosition;
        isDragging = true;
        cardImage.raycastTarget = false;
        transform.localScale = Vector3.one * 1.1f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }
}