using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class DeckUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI deckNameText;
    [SerializeField] GameObject editButtonObject;
    [SerializeField] GameObject deleteButtonObject;
    [SerializeField] GameObject highlightOverlay;
    [SerializeField] Button editButton;
    [SerializeField] Button deleteButton;

    Deck assignedDeck;
    CardMenuManager cardMenuManager;
    Action<DeckUI> onDeckSelectedCallback;

    public void Initialize(Deck deck, CardMenuManager menuManager)
    {
        this.cardMenuManager = menuManager;
        InternalSetup(deck);

        GetComponent<Button>().onClick.RemoveAllListeners();
        editButton?.onClick.RemoveAllListeners();
        deleteButton?.onClick.RemoveAllListeners();

        GetComponent<Button>().onClick.AddListener(OnDeckSelectedForEdit);
        if (editButton != null)
        {
            editButton.onClick.AddListener(OnEditButtonClicked);
        }
        if (deleteButton != null)
        {
            deleteButton.onClick.AddListener(OnDeleteButtonClicked);
        }
    }

    public void Initialize(Deck deck, Action<DeckUI> onSelected)
    {
        InternalSetup(deck);
        this.onDeckSelectedCallback = onSelected;
        GetComponent<Button>().onClick.RemoveAllListeners();
        GetComponent<Button>().onClick.AddListener(() =>
        {
            if (onDeckSelectedCallback != null)
            {
                onDeckSelectedCallback(this);
            }
        });
    }

    void InternalSetup(Deck deck)
    {
        this.assignedDeck = deck;
        if (deckNameText != null && assignedDeck != null)
        {
            deckNameText.text = assignedDeck.DeckName;
        }
        HideActions();
        SetHighlight(false);
    }

    void OnDeckSelectedForEdit()
    {
        if (cardMenuManager != null)
        {
            cardMenuManager.SelectDeckForEditing(this);
        }
    }

    void OnEditButtonClicked()
    {
        if (cardMenuManager != null)
        {
            cardMenuManager.OpenEditDeckEditor();
        }
    }

    void OnDeleteButtonClicked()
    {
        if (cardMenuManager != null)
        {
            cardMenuManager.DeleteSelectedDeck();
        }
    }

    public void ShowActions()
    {
        if (editButtonObject != null)
            editButtonObject.SetActive(true);
        if (deleteButtonObject != null)
            deleteButtonObject.SetActive(true);
    }

    public void HideActions()
    {
        if (editButtonObject != null)
            editButtonObject.SetActive(false);
        if (deleteButtonObject != null)
            deleteButtonObject.SetActive(false);
    }

    public void SetHighlight(bool isHighlighted)
    {
        if (highlightOverlay != null)
        {
            highlightOverlay.SetActive(isHighlighted);
        }
    }

    public Deck GetAssignedDeck()
    {
        return assignedDeck;
    }
}