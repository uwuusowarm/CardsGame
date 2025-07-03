using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class CardSelectorUI : MonoBehaviour
{
    [SerializeField] private GameObject highlightOverlay; 
    [SerializeField] private Transform cardParent; 

    private CardData assignedCard;
    private CardMenuManager manager;

    public void Initialize(CardData card, CardMenuManager menuManager, bool isSelected)
    {
        this.assignedCard = card;
        this.manager = menuManager;

        foreach (Transform child in cardParent)
        {
            Destroy(child.gameObject);
        }

        if (cardParent != null && assignedCard.cardPrefab != null)
        {
            Instantiate(assignedCard.cardPrefab, cardParent);
        }

        highlightOverlay.SetActive(isSelected);

        GetComponent<Button>().onClick.AddListener(OnCardClicked);
    }

    private void OnCardClicked()
    {
        manager.ToggleCardSelection(assignedCard);
        highlightOverlay.SetActive(!highlightOverlay.activeSelf);
    }
}