using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class CardUI : MonoBehaviour
{
    private CardData cardData;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI leftEffectText;
    [SerializeField] private TextMeshProUGUI rightEffectText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image cardImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image borderImage;

    public void Initialize(CardData data)
    {
        cardData = data;
        if (data == null)
        {
            Debug.LogError("CardData is null!");
            return;
        }

        nameText.text = data.cardName ?? "No Name";
        costText.text = data.manaCost.ToString();
        descriptionText.text = data.description ?? "";

        if (cardImage != null && data.cardArt != null)
        {
            cardImage.sprite = data.cardArt;
        }

        leftEffectText.text = FormatEffects(data.leftEffects);
        rightEffectText.text = FormatEffects(data.rightEffects);
        if (backgroundImage != null)
        {
        }
        if (borderImage != null)
        {
            borderImage.color = GetRarityColor(data.rarity);
        }
    }

    private Color GetRarityColor(CardRarity rarity)
    {
        switch (rarity)
        {
            case CardRarity.Common: return new Color(0.65f, 0.5f, 0.35f);
            case CardRarity.Rare: return new Color(0.75f, 0.75f, 0.75f);
            case CardRarity.Legendary: return new Color(1f, 0.84f, 0f);
            default: return Color.white;
        }
    }

    private string FormatEffects(List<CardEffect> effects)
    {
        if (effects == null || effects.Count == 0) return "No Effects";

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (var effect in effects)
        {
            if (effect != null)
            {
                string effectText = $"{effect.effectType}: {effect.value}";
                if (effect.effectType == CardEffect.EffectType.Attack)
                {
                    effectText += $" (Range: {effect.range})";
                }
                sb.AppendLine(effectText);
            }
        }
        return sb.ToString();
    }

    public CardData GetCardData()
    {
        return cardData;
    }
}