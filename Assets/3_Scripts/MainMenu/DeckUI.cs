using UnityEngine;
using UnityEngine.UI;
using TMPro; 

[RequireComponent(typeof(Button))] 
public class DeckUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI deckNameText;

    private Deck assignedDeck;
    private CardMenuManager manager;

    /// <param name="deck"
    /// <param name="menuManager"
    public void Initialize(Deck deck, CardMenuManager menuManager)
    {
        this.assignedDeck = deck;
        this.manager = menuManager;

        if (deckNameText != null)
        {
            deckNameText.text = assignedDeck.DeckName;
        }

        GetComponent<Button>().onClick.AddListener(OnDeckClicked);
    }

    private void OnDeckClicked()
    {
        if (manager != null && assignedDeck != null)
        {
            manager.SelectDeckForEditing(assignedDeck);
        }
    }
}