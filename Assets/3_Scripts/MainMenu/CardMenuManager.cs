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
    [SerializeField] private GameObject cardInListPrefab; // Hier kommt das Wrapper-Prefab rein
    [SerializeField] private Button saveDeckButton;

    [Header("Card Menu Referenzen")]
    [SerializeField] private Transform decksDisplayContainer;
    [SerializeField] private GameObject deckDisplayPrefab;
    [SerializeField] private Button editDeckButton;

    [Header("Zentrale Datenbank")]
    [SerializeField] private CardDatabaseSO cardDatabase;

    private List<CardData> allAvailableCards;
    private List<Deck> allPlayerDecks = new List<Deck>();
    private List<CardData> currentlySelectedCards = new List<CardData>();
    private Deck currentlyEditingDeck;

    private List<CardSelectorUI> spawnedCardUIs = new List<CardSelectorUI>();

    void Start()
    {
        if (cardDatabase != null) allAvailableCards = cardDatabase.allCards;

        mainMenuPanel.SetActive(true);
        cardMenuPanel.SetActive(false);
        deckEditorSlideout.SetActive(false);
        boostersSlideout.SetActive(false);
        editDeckButton.gameObject.SetActive(false);

        Sound_Manager.instance.Play("Level_Music");
    }


    private void PopulateAllCardsList()
    {
        foreach (Transform child in allCardsContainer)
        {
            Destroy(child.gameObject);
        }
        spawnedCardUIs.Clear();

        if (allAvailableCards == null) return;

        GridLayoutGroup gridLayout = allCardsContainer.GetComponent<GridLayoutGroup>();

        Vector2 targetCellSize = gridLayout.cellSize;

        foreach (var cardData in allAvailableCards)
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
        if (currentlySelectedCards.Contains(card))
        {
            currentlySelectedCards.Remove(card);
        }
        else
        {
            currentlySelectedCards.Add(card);
        }
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

    #region Restliche Methoden
    public void OpenCardMenu() 
    { 
        mainMenuPanel.SetActive(false); 
        cardMenuPanel.SetActive(true); 
        PopulateDeckDisplay(); 
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
        if (currentlyEditingDeck == null) 
            return; 

        currentlySelectedCards = new List<CardData>(currentlyEditingDeck.Cards); 
        deckEditorSlideout.SetActive(true); 
        PopulateAllCardsList(); 
        UpdateSaveButtonState(); 
    }
    public void OpenBoostersMenu() 
    { 
        cardMenuPanel.SetActive(false); 
        boostersSlideout.SetActive(true); 
    }
    public void SaveDeckAndCloseEditor() 
    { 
        if (currentlyEditingDeck != null) 
        { currentlyEditingDeck.Cards = new List<CardData>(currentlySelectedCards); 
        } 
        else 
        { 
            Deck newDeck = new Deck(); 
            newDeck.DeckName = "Neues Deck " + (allPlayerDecks.Count + 1); 
            newDeck.Cards = new List<CardData>(currentlySelectedCards); 
            allPlayerDecks.Add(newDeck); 
        } 
        deckEditorSlideout.SetActive(false); 
        PopulateDeckDisplay(); }
    public void BackToMainMenu() 
    { 
        cardMenuPanel.SetActive(false); 
        boostersSlideout.SetActive(false); 
        mainMenuPanel.SetActive(true); 
    }
    public void SelectDeckForEditing(Deck deck) 
    { 
        currentlyEditingDeck = deck; 
        editDeckButton.gameObject.SetActive(true); 
    }
    private void UpdateSaveButtonState() 
    { 
        if (saveDeckButton != null) 
        { 
            saveDeckButton.interactable = currentlySelectedCards.Count >= 15; 
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
            GameObject deckGO = Instantiate(deckDisplayPrefab, 
                decksDisplayContainer); 
            if (deckGO.TryGetComponent<DeckUI>(out var deckUI)) 
            { 
                deckUI.Initialize(deck, this); 
            } 
        } 
    }
    #endregion
}