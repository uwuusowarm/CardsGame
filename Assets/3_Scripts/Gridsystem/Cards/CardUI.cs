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
    [SerializeField] private Image cardImage;

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

        if (cardImage != null && data.cardArt != null)
        {
            cardImage.sprite = data.cardArt;
        }

        leftEffectText.text = FormatEffects(data.leftEffects);
        rightEffectText.text = FormatEffects(data.rightEffects);
    }

    private string FormatEffects(List<CardEffect> effects)
    {
        if (effects == null || effects.Count == 0) return "No Effects";

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (var effect in effects)
        {
            if (effect != null)
            {
                sb.AppendLine($"{effect.effectType}: {effect.value}");
            }
        }
        return sb.ToString();
    }
    public interface ICardDataHolder
    {
        CardData GetCardData();
    }

    public CardData GetCardData()
    {
        return cardData;
    }
}