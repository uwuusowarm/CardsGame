using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class CardMenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject deckEditorSlideout;

    [Header("Deck Display")]
    [SerializeField] private Transform decksDisplayContainer;
    [SerializeField] public GameObject deckDisplayPrefab;
    [SerializeField] private Button newDeck;
    [SerializeField] private TextMeshProUGUI deckCounter;

    [Header("Deck Editor")]
    [SerializeField] private Transform CardsContainer;
    [SerializeField] private GameObject cardInListPrefab;
    [SerializeField] private Button saveDeck;
    [SerializeField] private TextMeshProUGUI cardCounter;

    [Header("Data")]
    [SerializeField] private CardDatabaseSO cardDatabase;

    private List<Deck> allDecks = new List<Deck>();
    private List<CardData> SelectedCards = new List<CardData>();
    private Deck currentlyEditingDeck;
    private DeckUI DeckUI;
    private List<CardSelectorUI> spawnedCardUI = new List<CardSelectorUI>();

    private const int maxDeckCount = 12;

    void Start()
    {
        ReloadDecksFromFile();
        PopulateDeckDisplay();
    }

    public void ReloadDecksFromFile()
    {
        allDecks = SaveSystem.LoadDecks();
        if (allDecks == null)
        {
            allDecks = new List<Deck>();
        }
    }

    public void OpenEditDeckEditor()
    {
        if (currentlyEditingDeck == null) 
            return;
        SetMainMenuButtonsInteractable(false);
        SelectedCards = new List<CardData>(currentlyEditingDeck.Cards);
        if (deckEditorSlideout != null) 
            deckEditorSlideout.SetActive(true);
        PopulateAllCardsList();
        UpdateSaveButtonState(); 
        UpdateCardCounter();
    }

    public void OpenNewDeckEditor()
    {
        currentlyEditingDeck = null;
        SetMainMenuButtonsInteractable(false);
        SelectedCards.Clear();
        if (deckEditorSlideout != null) 
            deckEditorSlideout.SetActive(true);
        PopulateAllCardsList();
        UpdateSaveButtonState();
        UpdateCardCounter();
    }


    private void UpdateSaveButtonState()
    {
        if (saveDeck != null)
        {
            saveDeck.interactable = (SelectedCards.Count == 15);
        }
    }

    #region Unchanged Code
    private void SetMainMenuButtonsInteractable(bool isInteractable)
    {
        if (MainMenu.Instance != null)
        {
            MainMenu.Instance.SetMainMenuButtonsInteractable(isInteractable);
        }
        if (newDeck != null) 
            newDeck.interactable = isInteractable;
        if (saveDeck != null) 
            saveDeck.interactable = false;
    }

    public void SaveDeckAndCloseEditor()
    {
        if (currentlyEditingDeck != null)
        {
            currentlyEditingDeck.Cards = new List<CardData>(SelectedCards);
        }
        else
        {
            if (allDecks.Count >= maxDeckCount) 
                return;

            Deck newDeck = new Deck();
            newDeck.DeckName = "" + (allDecks.Count + 1);
            newDeck.Cards = new List<CardData>(SelectedCards);
            allDecks.Add(newDeck);
        }
        SaveSystem.SaveDecks(allDecks);
        if (deckEditorSlideout != null) 
            deckEditorSlideout.SetActive(false);
        PopulateDeckDisplay();
        SetMainMenuButtonsInteractable(true);
    }

    public void DeleteSelectedDeck()
    {
        if (currentlyEditingDeck == null) 
            return;
        allDecks.Remove(currentlyEditingDeck);
        SaveSystem.SaveDecks(allDecks);
        currentlyEditingDeck = null;
        DeckUI = null;
        PopulateDeckDisplay();
    }

    public void PopulateDeckDisplay()
    {
        if (decksDisplayContainer == null) 
            return;
        DeckUI = null;
        currentlyEditingDeck = null;
        foreach (Transform child in decksDisplayContainer)
            Destroy(child.gameObject);

        foreach (Deck deck in allDecks)
        {
            GameObject deckGameObject = Instantiate(deckDisplayPrefab, decksDisplayContainer);
            if (deckGameObject.TryGetComponent<DeckUI>(out var deckUI))
            {
                deckUI.Initialize(deck, this);
            }
        }

        UpdateDeckCounterAndNewButton();
    }

    private void UpdateDeckCounterAndNewButton()
    {
        int currentDeckCount = allDecks.Count;

        if (deckCounter != null)
        {
            deckCounter.text = currentDeckCount + "/" + maxDeckCount + " Decks";
        }

        if (newDeck != null)
        {
            newDeck.interactable = (currentDeckCount < maxDeckCount);
        }
    }

    public void ToggleCardSelection(CardData card)
    {
        if (SelectedCards.Contains(card))
        {
            SelectedCards.Remove(card);
        }
        else
        {
            if (SelectedCards.Count < 15)
            {
                SelectedCards.Add(card);
            }
        }
        UpdateSaveButtonState();
        UpdateAllCardHighlights();
        UpdateCardCounter();
    }

    private void UpdateCardCounter()
    {
        if (cardCounter != null)
        {
            cardCounter.text = SelectedCards.Count + "/15 Cards";
        }
    }

    public List<Deck> GetPlayerDecks()
    {
        return allDecks;
    }

    public GameObject DeckDisplayPrefab 
    { get 
        { 
            return deckDisplayPrefab; 
        } 
    }

    public void SelectDeckForEditing(DeckUI selectedUI)
    {
        if (DeckUI != null && DeckUI != selectedUI)
        {
            DeckUI.HideActions();
        }
        DeckUI = selectedUI;
        if (DeckUI != null)
        {
            currentlyEditingDeck = DeckUI.GetAssignedDeck();
            DeckUI.ShowActions();
        }
    }

    private void PopulateAllCardsList()
    {
        foreach (Transform child in CardsContainer) 
            Destroy(child.gameObject);
        spawnedCardUI.Clear();
        if (cardDatabase.allCards == null) 
            return;

        GridLayoutGroup gridLayout = CardsContainer.GetComponent<GridLayoutGroup>();
        Vector2 targetCellSize = (gridLayout != null) ? gridLayout.cellSize : new Vector2(100, 150);

        foreach (var cardData in cardDatabase.allCards)
        {
            if (cardData.cardPrefab == null) 
                continue;
            GameObject wrapperGameObject = Instantiate(cardInListPrefab, CardsContainer);
            CardSelectorUI cardUI = wrapperGameObject.GetComponent<CardSelectorUI>();
            if (cardUI != null)
            {
                bool isSelected = SelectedCards.Contains(cardData);
                cardUI.Initialize(cardData, this, isSelected, targetCellSize);
                spawnedCardUI.Add(cardUI);
            }
        }
        UpdateAllCardHighlights();
    }

    private void UpdateAllCardHighlights()
    {
        foreach (var cardUI in spawnedCardUI)
        {
            CardData cardData = cardUI.GetCardData();
            if (cardData != null)
            {
                cardUI.SetHighlight(SelectedCards.Contains(cardData));
            }
        }
    }
    #endregion
}