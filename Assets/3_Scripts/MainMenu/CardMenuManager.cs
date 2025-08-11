using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class CardMenuManager : MonoBehaviour
{

    public void OpenEditDeckEditor()
    {
        if (currentlyEditingDeck == null)
            return;

        if (MainMenu.Instance != null)
        {
            MainMenu.Instance.CloseAllPanels();
        }

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
        if (MainMenu.Instance != null)
        {
            MainMenu.Instance.CloseAllPanels();
        }

        currentlyEditingDeck = null;
        SetMainMenuButtonsInteractable(false);
        SelectedCards.Clear();
        if (deckEditorSlideout != null)
            deckEditorSlideout.SetActive(true);
        PopulateAllCardsList();
        UpdateSaveButtonState();
        UpdateCardCounter();
    }

    [Header("UI Panels")]
    [SerializeField] GameObject deckEditorSlideout;

    [Header("Deck Display")]
    [SerializeField] Transform decksDisplayContainer;
    [SerializeField] public GameObject deckDisplayPrefab;
    [SerializeField] Button newDeck;
    [SerializeField] TextMeshProUGUI deckCounter;

    [Header("Deck Editor")]
    [SerializeField] Transform CardsContainer;
    [SerializeField] GameObject cardInListPrefab;
    [SerializeField] Button saveDeck;
    [SerializeField] TextMeshProUGUI cardCounter;

    [Header("Data")]
    [SerializeField] CardDatabaseSO cardDatabase;

    List<Deck> allDecks = new List<Deck>();
    List<CardData> SelectedCards = new List<CardData>();
    Deck currentlyEditingDeck;
    DeckUI DeckUI;
    List<CardSelectorUI> spawnedCardUI = new List<CardSelectorUI>();

    const int maxDeckCount = 12;

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

    void UpdateSaveButtonState()
    {
        if (saveDeck != null)
        {
            saveDeck.interactable = (SelectedCards.Count == 15);
        }
    }

    void SetMainMenuButtonsInteractable(bool isInteractable)
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

        int currentDeckCount = allDecks.Count;
        int emptySlots = maxDeckCount - currentDeckCount;

        /*
        if (emptyDeckSlotPrefab != null)
        {
            for (int i = 0; i < emptySlots; i++)
            {
                Instantiate(emptyDeckSlotPrefab, decksDisplayContainer);
            }
        }
        */

        UpdateDeckCounterAndNewButton();
    }

    void UpdateDeckCounterAndNewButton()
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

    void UpdateCardCounter()
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
    {
        get
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

    void PopulateAllCardsList()
    {
        foreach (Transform child in CardsContainer)
            Destroy(child.gameObject);
        spawnedCardUI.Clear();
        if (cardDatabase._allCards == null)
            return;

        GridLayoutGroup gridLayout = CardsContainer.GetComponent<GridLayoutGroup>();
        Vector2 targetCellSize = (gridLayout != null) ? gridLayout.cellSize : new Vector2(100, 150);

        foreach (var cardData in cardDatabase._allCards)
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

    void UpdateAllCardHighlights()
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
}