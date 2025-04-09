using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardDisplay : MonoBehaviour
{
    public Card card;
    public Image artworkImage;
    public TextMeshProUGUI nameText;

    void Start()
    {
        nameText.text = card.cardName;
        artworkImage.sprite = card.artwork;
    }
}
