using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class CardMenuManager : MonoBehaviour
{
    [Header("Panel-Referenzen")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject cardMenuPanel;
    [SerializeField] private GameObject deckEditorSlideout;
    [SerializeField] private GameObject boostersSlideout;

    [Header("Deck Editor Referenzen")]
    [SerializeField] private Transform allCardsContainer;
    [SerializeField] private GameObject cardInListPrefab;
    [SerializeField] private Button saveDeckButton;

    [Header("Card Menu Referenzen")]
    [SerializeField] private Transform decksDisplayContainer;
    public GameObject deckDisplayPrefab;

    [Header("Zentrale Datenbank")]
    [SerializeField] private CardDatabaseSO cardDatabase;

    private List<Deck> allPlayerDecks = new List<Deck>();
    private List<CardData> currentlySelectedCards = new List<CardData>();
    private Deck currentlyEditingDeck;
    private DeckUI currentlySelectedDeckUI;
    private List<CardSelectorUI> spawnedCardUIs = new List<CardSelectorUI>();

    void Start()
    {
        allPlayerDecks = SaveSystem.LoadDecks();
    }

    public List<Deck> GetPlayerDecks()
    {
        return allPlayerDecks;
    }

    public void SelectDeckForEditing(DeckUI selectedUI)
    {
        if (currentlySelectedDeckUI != null && currentlySelectedDeckUI != selectedUI)
        {
            currentlySelectedDeckUI.HideActions();
        }
        currentlySelectedDeckUI = selectedUI;
        if (currentlySelectedDeckUI != null)
        {
            currentlyEditingDeck = currentlySelectedDeckUI.GetAssignedDeck();
            currentlySelectedDeckUI.ShowActions();
        }
    }

    public void OpenEditDeckEditor()
    {
        if (currentlyEditingDeck == null) return;
        currentlySelectedCards = new List<CardData>(currentlyEditingDeck.Cards);
        if (deckEditorSlideout != null) deckEditorSlideout.SetActive(true);
        PopulateAllCardsList();
        UpdateSaveButtonState();
    }

    public void DeleteSelectedDeck()
    {
        if (currentlyEditingDeck == null) return;
        allPlayerDecks.Remove(currentlyEditingDeck);
        SaveSystem.SaveDecks(allPlayerDecks);
        currentlyEditingDeck = null;
        currentlySelectedDeckUI = null;
        PopulateDeckDisplay();
    }

    public void SaveDeckAndCloseEditor()
    {
        if (currentlyEditingDeck != null)
        {
            currentlyEditingDeck.Cards = new List<CardData>(currentlySelectedCards);
        }
        else
        {
            Deck newDeck = new Deck();
            newDeck.DeckName = "Deck " + (allPlayerDecks.Count + 1);
            newDeck.Cards = new List<CardData>(currentlySelectedCards);
            allPlayerDecks.Add(newDeck);
        }
        SaveSystem.SaveDecks(allPlayerDecks);
        if (deckEditorSlideout != null) deckEditorSlideout.SetActive(false);
        PopulateDeckDisplay();
    }

    private void PopulateAllCardsList()
    {
        foreach (Transform child in allCardsContainer) Destroy(child.gameObject);
        spawnedCardUIs.Clear();
        if (cardDatabase.allCards == null) return;

        GridLayoutGroup gridLayout = allCardsContainer.GetComponent<GridLayoutGroup>();
        Vector2 targetCellSize = (gridLayout != null) ? gridLayout.cellSize : new Vector2(100, 150);

        foreach (var cardData in cardDatabase.allCards)
        {
            if (cardData.cardPrefab == null) continue;
            GameObject wrapperGO = Instantiate(cardInListPrefab, allCardsContainer);
            wrapperGO.name = "Wrapper_" + cardData.cardName;
            CardSelectorUI cardUI = wrapperGO.GetComponent<CardSelectorUI>();
            if (cardUI != null)
            {
                bool isSelected = currentlySelectedCards.Contains(cardData);
                cardUI.Initialize(cardData, this, isSelected, targetCellSize);
                spawnedCardUIs.Add(cardUI);
            }
        }
        UpdateAllCardHighlights();
    }

    public void ToggleCardSelection(CardData card)
    {
        if (currentlySelectedCards.Contains(card)) currentlySelectedCards.Remove(card);
        else currentlySelectedCards.Add(card);
        UpdateSaveButtonState();
        UpdateAllCardHighlights();
    }

    private void UpdateAllCardHighlights()
    {
        foreach (var cardUI in spawnedCardUIs)
        {
            CardData data = cardUI.GetCardData();
            if (data != null)
            {
                cardUI.SetHighlight(currentlySelectedCards.Contains(data));
            }
        }
    }

    public void OpenNewDeckEditor()
    {
        currentlyEditingDeck = null;
        currentlySelectedCards.Clear();
        if (deckEditorSlideout != null) deckEditorSlideout.SetActive(true);
        PopulateAllCardsList();
        UpdateSaveButtonState();
    }

    private void UpdateSaveButtonState()
    {
        if (saveDeckButton != null) saveDeckButton.interactable = currentlySelectedCards.Count >= 15;
    }

    public void PopulateDeckDisplay()
    {
        if (decksDisplayContainer == null) return;
        currentlySelectedDeckUI = null;
        currentlyEditingDeck = null;

        foreach (Transform child in decksDisplayContainer) Destroy(child.gameObject);
        foreach (Deck deck in allPlayerDecks)
        {
            GameObject deckGO = Instantiate(deckDisplayPrefab, decksDisplayContainer);
            if (deckGO.TryGetComponent<DeckUI>(out var deckUI))
            {
                deckUI.Initialize(deck, this);
            }
        }
    }
}