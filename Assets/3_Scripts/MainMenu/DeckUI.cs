using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

[RequireComponent(typeof(Button))]
public class DeckUI : MonoBehaviour
{
    [Header("UI-Elemente im Prefab")]
    [SerializeField] private TextMeshProUGUI deckNameText;
    [SerializeField] private Button editButton;
    [SerializeField] private Button deleteButton;

    private Deck assignedDeck;
    private CardMenuManager manager;

    public void Initialize(Deck deck, CardMenuManager menuManager)
    {
        this.assignedDeck = deck;
        this.manager = menuManager;

        if (deckNameText != null && assignedDeck != null)
        {
            deckNameText.text = assignedDeck.DeckName;
        }

        HideActions();

        GetComponent<Button>().onClick.AddListener(OnDeckSelected);

        if (editButton != null)
        {
            editButton.onClick.AddListener(OnEditButtonClicked);
        }
        if (deleteButton != null)
        {
            deleteButton.onClick.AddListener(OnDeleteButtonClicked);
        }
    }

    public void Initialize(Deck deck, Action<Deck> onSelected)
    {
        this.assignedDeck = deck;
        if (deckNameText != null)
        {
            deckNameText.text = assignedDeck.DeckName;
        }
        GetComponent<Button>().onClick.RemoveAllListeners();
        GetComponent<Button>().onClick.AddListener(() => onSelected(deck));

        if (editButton != null) editButton.gameObject.SetActive(false);
        if (deleteButton != null) deleteButton.gameObject.SetActive(false);
    }

    private void OnDeckSelected()
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
        if (editButton != null) editButton.gameObject.SetActive(true);
        if (deleteButton != null) deleteButton.gameObject.SetActive(true);
    }

    public void HideActions()
    {
        if (editButton != null) editButton.gameObject.SetActive(false);
        if (deleteButton != null) deleteButton.gameObject.SetActive(false);
    }

    public Deck GetAssignedDeck()
    {
        return assignedDeck;
    }
}