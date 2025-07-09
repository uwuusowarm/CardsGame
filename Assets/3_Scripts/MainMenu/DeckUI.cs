using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

[RequireComponent(typeof(Button))]
public class DeckUI : MonoBehaviour
{
    [Header("UI-Elemente im Prefab")]
    [SerializeField] private TextMeshProUGUI deckNameText;
    [SerializeField] private GameObject editButtonObject;
    [SerializeField] private GameObject deleteButtonObject;
    [SerializeField] private GameObject highlightOverlay;

    [SerializeField] private Button editButton;
    [SerializeField] private Button deleteButton;

    private Deck assignedDeck;
    private CardMenuManager manager;
    private Action<DeckUI> onDeckSelectedCallback_Play;

    public void Initialize(Deck deck, CardMenuManager menuManager)
    {
        this.manager = menuManager;
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
        this.onDeckSelectedCallback_Play = onSelected;
        GetComponent<Button>().onClick.RemoveAllListeners();
        GetComponent<Button>().onClick.AddListener(() => onDeckSelectedCallback_Play?.Invoke(this));
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
        if (manager != null)
        {
            manager.SelectDeckForEditing(this);
        }
    }

    private void OnEditButtonClicked()
    {
        if (manager != null)
        {
            manager.OpenEditDeckEditor();
        }
    }

    private void OnDeleteButtonClicked()
    {
        if (manager != null)
        {
            manager.DeleteSelectedDeck();
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