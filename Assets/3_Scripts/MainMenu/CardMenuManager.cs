using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

[System.Serializable]
public struct CardVisualLookup
{
    public CardData cardData;
    public GameObject visualPrefab;
}

public class CardMenuManager : MonoBehaviour
{
    [Header("Panel-Referenzen")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject cardMenuPanel;
    [SerializeField] private GameObject deckEditorSlideout;
    [SerializeField] private GameObject boostersSlideout;

    [Header("Deck Editor Referenzen")]
    [SerializeField] private RectTransform allCardsContainer;
    [SerializeField] private GameObject cardInListPrefab;
    [SerializeField] private Button saveDeckButton;

    [Header("Card Menu Referenzen")]
    [SerializeField] private Transform decksDisplayContainer;
    [SerializeField] private GameObject deckDisplayPrefab;
    [SerializeField] private Button editDeckButton;

    [Header("Booster Referenzen")]
    [SerializeField] private TextMeshProUGUI goldAnzeigeText;

    [Header("Neues Manuelles Layout")]
    [SerializeField] private int numberOfColumns = 4;
    [SerializeField] private Vector2 cardSize = new Vector2(150, 210);
    [SerializeField] private Vector2 spacing = new Vector2(20, 20);
    [SerializeField] private RectOffset padding;

    [Header("Zentrale Datenbank")]
    [SerializeField] private CardDatabaseSO cardDatabase;

    [Header("Visuelle Zuordnung (Nur für Menü)")]
    [SerializeField] private List<CardVisualLookup> cardVisuals;

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

    private void PopulateAllCardsList()
    {
        foreach (Transform child in allCardsContainer)
        {
            Destroy(child.gameObject);
        }

        if (allAvailableCards == null || allAvailableCards.Count == 0) return;

        for (int i = 0; i < allAvailableCards.Count; i++)
        {
            CardData cardData = allAvailableCards[i];
            GameObject wrapperGO = Instantiate(cardInListPrefab, allCardsContainer);
            wrapperGO.name = "Card_" + i + "_" + cardData.name;

            RectTransform wrapperRect = wrapperGO.GetComponent<RectTransform>();
            wrapperRect.anchorMin = new Vector2(0, 1);
            wrapperRect.anchorMax = new Vector2(0, 1);
            wrapperRect.pivot = new Vector2(0, 1);
            wrapperRect.sizeDelta = cardSize;

            int column = i % numberOfColumns;
            int row = i / numberOfColumns;

            float xPos = padding.left + column * (cardSize.x + spacing.x);
            float yPos = -padding.top - row * (cardSize.y + spacing.y);

            wrapperRect.anchoredPosition = new Vector2(xPos, yPos);

            CardSelectorUI cardUI = wrapperGO.GetComponent<CardSelectorUI>();
            if (cardUI != null)
            {
                GameObject visualPrefab = GetVisualPrefabForCard(cardData);
                bool isSelected = currentlySelectedCards.Contains(cardData);
                cardUI.Initialize(cardData, visualPrefab, this, isSelected);
            }
        }

        int totalRows = Mathf.CeilToInt((float)allAvailableCards.Count / numberOfColumns);
        float totalHeight = padding.top + totalRows * cardSize.y + Mathf.Max(0, totalRows - 1) * spacing.y + padding.bottom;
        allCardsContainer.sizeDelta = new Vector2(allCardsContainer.sizeDelta.x, totalHeight);
    }

    private GameObject GetVisualPrefabForCard(CardData data)
    {
        foreach (var lookup in cardVisuals)
        {
            // --- HIER IST DIE ÄNDERUNG ---
            // Wir vergleichen jetzt die Namen der Assets, nicht die Objekte selbst.
            if (lookup.cardData != null && lookup.cardData.name == data.name)
            {
                return lookup.visualPrefab;
            }
        }
        Debug.LogWarning("Kein visuelles Prefab für die Karte " + data.name + " in der Lookup-Tabelle gefunden!");
        return null;
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
        }
        else
        {
            Deck newDeck = new Deck();
            newDeck.DeckName = "Neues Deck " + (allPlayerDecks.Count + 1);
            newDeck.Cards = new List<CardData>(currentlySelectedCards);
            allPlayerDecks.Add(newDeck);
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
    }

    private void UpdateSaveButtonState()
    {
        saveDeckButton.interactable = currentlySelectedCards.Count >= 15;
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
            if (deckGO.TryGetComponent<DeckUI>(out var deckUI))
            {
                deckUI.Initialize(deck, this);
            }
        }
    }
    #endregion
}