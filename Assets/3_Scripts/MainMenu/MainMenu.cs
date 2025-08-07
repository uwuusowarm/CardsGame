using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour
{
    public static MainMenu Instance { get; private set; }

    [Header("UI Panels")]
    [SerializeField] private GameObject cardMenuPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject deckSelectionPanel;

    [Header("Main Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button deckMenuButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button quitButton;

    [Header("Wichtige Referenzen")]
    [SerializeField] private CardMenuManager cardMenuManager;
    [SerializeField] private Transform deckSelectionContainer; 
    [SerializeField] private Button playGameButton;

    private GameObject settingsPanel;
    private GameObject currentlyActivePanel;
    private Deck currentlySelectedDeckForPlay;
    private DeckUI currentlySelectedDeckUIForPlay;

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    private void Start()
    {
        if (SettingsManager.Instance != null)
        {
            settingsPanel = SettingsManager.Instance.GetOptionsPanelObject();
        }
        CloseAllPanels();
    }

    public void OnStartButtonPressed()
    {
        TogglePanel(deckSelectionPanel);
        if (deckSelectionPanel != null && deckSelectionPanel.activeSelf)
        {
            PopulateDeckSelection();
        }
    }

    private void PopulateDeckSelection()
    {
        if (playGameButton != null) playGameButton.gameObject.SetActive(false);
        currentlySelectedDeckForPlay = null;
        if (currentlySelectedDeckUIForPlay != null)
        {
            currentlySelectedDeckUIForPlay.SetHighlight(false);
            currentlySelectedDeckUIForPlay = null;
        }
        foreach (Transform child in deckSelectionContainer)
        {
            Destroy(child.gameObject);
        }

        cardMenuManager.ReloadDecksFromFile();

        List<Deck> playerDecks = cardMenuManager.GetPlayerDecks();
        GameObject deckDisplayPrefab = cardMenuManager.DeckDisplayPrefab;

        foreach (var deck in playerDecks)
        {
            GameObject deckGameObject = Instantiate(deckDisplayPrefab, deckSelectionContainer);
            DeckUI deckUI = deckGameObject.GetComponent<DeckUI>();
            if (deckUI != null)
            {
                deckUI.Initialize(deck, (selectedUI) => {
                    SelectDeckForPlay(selectedUI);
                });
            }
        }
    }

    #region Unchanged Code
    public void SetMainMenuButtonsInteractable(bool isInteractable)
    {
        if (startButton != null) startButton.interactable = isInteractable;
        if (deckMenuButton != null) deckMenuButton.interactable = isInteractable;
        if (settingsButton != null) settingsButton.interactable = isInteractable;
        if (creditsButton != null) creditsButton.interactable = isInteractable;
        if (quitButton != null) quitButton.interactable = isInteractable;
    }

    private void TogglePanel(GameObject panelToToggle)
    {
        if (panelToToggle == null) return;
        bool isAlreadyActive = panelToToggle.activeSelf;
        CloseAllPanels();

        if (!isAlreadyActive)
        {
            panelToToggle.SetActive(true);
            currentlyActivePanel = panelToToggle;
        }
    }

    private void CloseAllPanels()
    {
        if (cardMenuPanel != null) cardMenuPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (deckSelectionPanel != null) deckSelectionPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        currentlyActivePanel = null;
    }

    public void OnDeckMenuButtonPressed()
    {
        TogglePanel(cardMenuPanel);
    }

    public void OnCreditsButtonPressed()
    {
        TogglePanel(creditsPanel);
    }

    public void OnSettingsButtonPressed()
    {
        bool isSettingsPanelActive = (settingsPanel != null && settingsPanel.activeSelf);
        CloseAllPanels();
        if (!isSettingsPanelActive && SettingsManager.Instance != null)
        {
            SettingsManager.Instance.ToggleOptionsPanel();
            currentlyActivePanel = settingsPanel;
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
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
    #endregion
}