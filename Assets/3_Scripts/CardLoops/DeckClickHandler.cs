using UnityEngine;

public class DeckClickHandler : MonoBehaviour
{
    [SerializeField] private CardUI cardUIPrefab;
    [SerializeField] private Transform handParent;

    public void OnCardClicked(CardData card)
    {
        var cardUI = Instantiate(cardUIPrefab, handParent);
        cardUI.Initialize(card);
    }

    public void OnCardScriptableClicked(CardScriptable card)
    {
        var cardUI = Instantiate(cardUIPrefab, handParent);
        cardUI.Initialize(card);
    }
}
