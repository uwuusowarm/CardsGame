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

    public void Initialize(CardScriptable scriptableObject)
    {
        if (scriptableObject == null)
        {
            Debug.LogError("CardScriptable is null!");
            return;
        }

        nameText.text = scriptableObject.cardName ?? "No Name";
        costText.text = scriptableObject.manaCost.ToString();

        if (cardImage != null && scriptableObject.cardArt != null)
        {
            cardImage.sprite = scriptableObject.cardArt;
        }

        leftEffectText.text = FormatEffects(scriptableObject.leftEffects);
        rightEffectText.text = FormatEffects(scriptableObject.rightEffects);
    }

    public void Initialize(CardData data)
    {
        cardData = data;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (cardData == null)
        {
            Debug.LogError("CardData is null!");
            return;
        }

        nameText.text = cardData.cardName ?? "No Name";
        costText.text = cardData.manaCost.ToString();

        if (cardImage != null && cardData.cardArt != null)
        {
            cardImage.sprite = cardData.cardArt;
        }

        leftEffectText.text = FormatEffects(cardData.leftEffects);
        rightEffectText.text = FormatEffects(cardData.rightEffects);
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
