using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class DeckUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI deckNameText;
    [SerializeField] private GameObject editButtonObject;
    [SerializeField] private GameObject deleteButtonObject;
    [SerializeField] private GameObject highlightOverlay;
    [SerializeField] private Button editButton;
    [SerializeField] private Button deleteButton;

    private Deck assignedDeck;
    private CardMenuManager cardMenuManager;
    private Action<DeckUI> onDeckSelectedCallback;

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

    private void InternalSetup(Deck deck)
    {
        this.assignedDeck = deck;
        if (deckNameText != null && assignedDeck != null)
        {
            deckNameText.text = assignedDeck.DeckName;
        }
        HideActions();
        SetHighlight(false);
    }

    private void OnDeckSelectedForEdit()
    {
        if (cardMenuManager != null)
        {
            cardMenuManager.SelectDeckForEditing(this);
        }
    }

    private void OnEditButtonClicked()
    {
        if (cardMenuManager != null)
        {
            cardMenuManager.OpenEditDeckEditor();
        }
    }

    private void OnDeleteButtonClicked()
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