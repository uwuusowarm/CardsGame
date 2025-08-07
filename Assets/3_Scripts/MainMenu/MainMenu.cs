using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("Panel Management")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject cardMenuPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject deckSelectionPanel;
    [SerializeField] private GameObject deckEditorSlideout;
    [SerializeField] private GameObject boostersSlideout;

    [Header("Wichtige Referenzen")]
    [SerializeField] private CardMenuManager cardMenuManager;
    [SerializeField] private Transform deckSelectionContainer;
    [SerializeField] private Button playGameButton;

    private Deck currentlySelectedDeckForPlay;
    private DeckUI currentlySelectedDeckUIForPlay;

    private void Awake()
    {
        if (FindObjectOfType<GameDataManager>() == null)
        {
            new GameObject("GameDataManager").AddComponent<GameDataManager>();
        }
        if (FindObjectOfType<SettingsManager>() == null)
        {
            Debug.LogWarning("Kein SettingsManager in der Szene gefunden.");
        }
    }

    private void Start()
    {
        ReturnToMainMenu();
    }

    private void ShowPanel(GameObject panelToShow)
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (cardMenuPanel != null) cardMenuPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (deckSelectionPanel != null) deckSelectionPanel.SetActive(false);
        if (deckEditorSlideout != null) deckEditorSlideout.SetActive(false);
        if (boostersSlideout != null) boostersSlideout.SetActive(false);

        if (panelToShow != null)
        {
            panelToShow.SetActive(true);
        }
    }

    public void OpenOptionsPanel()
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.ToggleOptionsPanel();
        }
    }

    public void OpenDeckSelectionScreen()
    {
        ShowPanel(deckSelectionPanel);
        if (playGameButton != null) playGameButton.gameObject.SetActive(false);
        currentlySelectedDeckForPlay = null;
        if (currentlySelectedDeckUIForPlay != null)
        {
            currentlySelectedDeckUIForPlay.SetHighlight(false);
            currentlySelectedDeckUIForPlay = null;
        }

        foreach (Transform child in deckSelectionContainer) Destroy(child.gameObject);

        List<Deck> playerDecks = cardMenuManager.GetPlayerDecks();
        GameObject deckDisplayPrefab = cardMenuManager.deckDisplayPrefab;

        foreach (var deck in playerDecks)
        {
            GameObject deckGO = Instantiate(deckDisplayPrefab, deckSelectionContainer);
            DeckUI deckUI = deckGO.GetComponent<DeckUI>();
            if (deckUI != null)
            {
                deckUI.Initialize(deck, (selectedUI) => {
                    SelectDeckForPlay(selectedUI);
                });
            }
        }
    }

    private void SelectDeckForPlay(DeckUI selectedUI)
    {
        if (currentlySelectedDeckUIForPlay == selectedUI)
        {
            selectedUI.SetHighlight(false);
            currentlySelectedDeckUIForPlay = null;
            currentlySelectedDeckForPlay = null;
            if (playGameButton != null) playGameButton.gameObject.SetActive(false);
        }
        else
        {
            if (currentlySelectedDeckUIForPlay != null)
            {
                currentlySelectedDeckUIForPlay.SetHighlight(false);
            }
            currentlySelectedDeckUIForPlay = selectedUI;
            currentlySelectedDeckForPlay = selectedUI.GetAssignedDeck();
            currentlySelectedDeckUIForPlay.SetHighlight(true);
            if (playGameButton != null) playGameButton.gameObject.SetActive(true);
        }
    }

    public void LaunchGame()
    {
        if (currentlySelectedDeckForPlay != null && GameDataManager.Instance != null)
        {
            GameDataManager.Instance.selectedDeck = currentlySelectedDeckForPlay;
            SceneManager.LoadScene(5);
        }
        else
        {
            Debug.LogError("Kein Deck ausgewählt oder GameDataManager nicht gefunden!");
        }
    }

    public void ReturnToMainMenu() { ShowPanel(mainPanel); }
    public void OpenCardMenuPanel() { ShowPanel(cardMenuPanel); }
    public void OpenCreditsPanel() { ShowPanel(creditsPanel); }
    public void StartButton() { OpenDeckSelectionScreen(); }
    public void QuitGame()
    {
        Application.Quit(); 
        #if UNITY_EDITOR 
        UnityEditor.EditorApplication.isPlaying = false; 
        #endif 
    }
}