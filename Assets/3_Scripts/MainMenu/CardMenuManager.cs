using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class Deck
{
    public string DeckName;
    public List<CardData> Cards = new List<CardData>();
}

public class CardMenuManager : MonoBehaviour
{
    [Header("Zentrale Database")]
    [SerializeField] private CardDatabaseSO cardDatabase;
    [Header("Panel References")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject cardMenuPanel;
    [SerializeField] private GameObject deckEditorSlideout;
    [SerializeField] private GameObject boostersSlideout;

    [Header("Deck Editor References")]
    [SerializeField] private Transform allCardsContainer;
    [SerializeField] private GameObject cardInListPrefab; 
    [SerializeField] private Button saveDeckButton;

    [Header("Card Menu References")]
    [SerializeField] private Transform decksDisplayContainer; 
    [SerializeField] private GameObject deckDisplayPrefab;
    [SerializeField] private Button editDeckButton;

    [Header("Booster References")]
    [SerializeField] private TextMeshProUGUI goldAnzeigeText;

    private List<CardData> allAvailableCards;
    private List<Deck> allPlayerDecks = new List<Deck>(); 

    private List<CardData> currentlySelectedCards = new List<CardData>();
    private Deck currentlyEditingDeck;

    void Start()
    {
        if (cardDatabase != null)
        {
            allAvailableCards = cardDatabase.allCards;
        }
        else
        {
            Debug.LogError("CardDatabaseSO wurde nicht im CardMenuManager zugewiesen!");
        }

        mainMenuPanel.SetActive(true);
        cardMenuPanel.SetActive(false);
        deckEditorSlideout.SetActive(false);
        boostersSlideout.SetActive(false);
        editDeckButton.gameObject.SetActive(false);
    }

    #region Navigation
    public void OpenCardMenu()
    {
        mainMenuPanel.SetActive(false);
        cardMenuPanel.SetActive(true);

    }

    public void OpenNewDeckEditor()
    {
        currentlyEditingDeck = null; 
        currentlySelectedCards.Clear();

        deckEditorSlideout.SetActive(true);
        PopulateAllCardsList();
        UpdateSaveButtonState();
    }

    public void OpenEditDeckEditor()
    {
        if (currentlyEditingDeck == null) return; 

        currentlySelectedCards = new List<CardData>(currentlyEditingDeck.Cards);

        deckEditorSlideout.SetActive(true);
        PopulateAllCardsList();
        UpdateSaveButtonState();
    }

    public void OpenBoostersMenu()
    {
        cardMenuPanel.SetActive(false);
        boostersSlideout.SetActive(true);
        goldAnzeigeText.text = "Gold: 999"; 
    }

    public void SaveDeckAndCloseEditor()
    {
        if (currentlyEditingDeck != null)
        {
            currentlyEditingDeck.Cards = new List<CardData>(currentlySelectedCards);
            Debug.Log("Deck '" + currentlyEditingDeck.DeckName + "' mit " + currentlySelectedCards.Count + " Karten aktualisiert.");
        }
        else
        {
            Deck newDeck = new Deck();
            newDeck.DeckName = "Neues Deck " + (allPlayerDecks.Count + 1);
            newDeck.Cards = new List<CardData>(currentlySelectedCards);
            allPlayerDecks.Add(newDeck);
            Debug.Log("Neues Deck mit " + currentlySelectedCards.Count + " Karten erstellt.");
        }

        deckEditorSlideout.SetActive(false);
        PopulateDeckDisplay(); 
    }

    public void BackToMainMenu()
    {
        cardMenuPanel.SetActive(false);
        boostersSlideout.SetActive(false);
        mainMenuPanel.SetActive(true);
    }
    #endregion

    #region Logik & UI-Population

    private void PopulateAllCardsList()
    {
        foreach (Transform child in allCardsContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (CardData cardData in allAvailableCards)
        {
            GameObject wrapperGO = Instantiate(cardInListPrefab, allCardsContainer);

            CardSelectorUI cardUI = wrapperGO.GetComponent<CardSelectorUI>();
            if (cardUI != null)
            {
                bool isSelected = currentlySelectedCards.Contains(cardData);
                cardUI.Initialize(cardData, this, isSelected);
            }
        }
    }

    private void PopulateDeckDisplay()
    {
        foreach (Transform child in decksDisplayContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (Deck deck in allPlayerDecks)
        {
            GameObject deckGO = Instantiate(deckDisplayPrefab, decksDisplayContainer);
            deckGO.GetComponent<DeckUI>().Initialize(deck, this);
        }
    }

    public void ToggleCardSelection(CardData card)
    {
        if (currentlySelectedCards.Contains(card))
        {
            currentlySelectedCards.Remove(card);
        }
        else
        {
            currentlySelectedCards.Add(card);
        }
        UpdateSaveButtonState();
    }

    public void SelectDeckForEditing(Deck deck)
    {
        currentlyEditingDeck = deck;
        editDeckButton.gameObject.SetActive(true);

        Debug.Log("Deck '" + deck.DeckName + "' zur Bearbeitung ausgewählt.");
    }

    private void UpdateSaveButtonState()
    {
        saveDeckButton.interactable = currentlySelectedCards.Count >= 15;
    }

    #endregion
}