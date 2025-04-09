using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardDisplay : MonoBehaviour
{
    public Card card;
    public TMP_Text nameText;
    public Image artworkImage;

    void Start()
    {
        nameText.text = card.cardName;
        artworkImage.sprite = card.artwork;
    }
}
