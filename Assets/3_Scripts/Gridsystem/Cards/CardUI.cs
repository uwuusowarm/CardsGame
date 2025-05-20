using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CardUI : MonoBehaviour
{
    [Header("Basic Info")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image cardImage;

    [Header("Visual Elements")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image borderImage;

    [Header("Left Effect")]
    [SerializeField] private TextMeshProUGUI leftEffectValue;
    [SerializeField] private Image leftEffectIcon;

    [Header("Right Effect")]
    [SerializeField] private TextMeshProUGUI rightEffectValue;
    [SerializeField] private Image rightEffectIcon;

    private CardData cardData;

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
        
        if (data.leftEffects != null && data.leftEffects.Count > 0)
        {
            leftEffectValue.text = data.leftEffects[0].value.ToString();
            leftEffectIcon.sprite = data.leftEffectIcon;
            leftEffectIcon.gameObject.SetActive(data.leftEffectIcon != null);
        }

        if (data.rightEffects != null && data.rightEffects.Count > 0)
        {
            rightEffectValue.text = data.rightEffects[0].value.ToString();
            rightEffectIcon.sprite = data.rightEffectIcon;
            rightEffectIcon.gameObject.SetActive(data.rightEffectIcon != null);
        }
        
        if (cardImage != null && data.cardArt != null)
        {
            cardImage.sprite = data.cardArt;
        }
    }

    public void SetBackground(Sprite background)
    {
        if (backgroundImage != null)
        {
            backgroundImage.sprite = background;
        }
    }

    public void SetBorder(Sprite border)
    {
        if (borderImage != null)
        {
            borderImage.sprite = border;
        }
    }

    public void SetLeftEffect(int value, Sprite icon)
    {
        if (leftEffectValue != null)
        {
            leftEffectValue.text = value.ToString();
        }

        if (leftEffectIcon != null)
        {
            leftEffectIcon.sprite = icon;
            leftEffectIcon.gameObject.SetActive(icon != null);
        }
    }

    public void SetRightEffect(int value, Sprite icon)
    {
        if (rightEffectValue != null)
        {
            rightEffectValue.text = value.ToString();
        }

        if (rightEffectIcon != null)
        {
            rightEffectIcon.sprite = icon;
            rightEffectIcon.gameObject.SetActive(icon != null);
        }
    }

    public CardData GetCardData()
    {
        return cardData;
    }
}