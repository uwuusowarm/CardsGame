using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour
{
    [Header("UI panels")]
    [SerializeField] private GameObject cardMenuPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject deckSelectionPanel;

    [Header("!references!")]
    [SerializeField] private CardMenuManager cardMenuManager;
    [SerializeField] private Transform deckSelectionContainer;
    [SerializeField] private Button playGameButton;

    private GameObject settingsPanel;
    private GameObject currentlyActivePanel;

    private Deck currentlySelectedDeckForPlay;
    private DeckUI currentlySelectedDeckUIForPlay;
    bool isSettingsPanelActive = false;
    bool isActive = false;

    private void Start()
    {
        if (SettingsManager.Instance != null)
        {
            settingsPanel = SettingsManager.Instance.GetOptionsPanelObject();
        }

        CloseAllPanels();
    }

    private void TogglePanel(GameObject panelToToggle)
    {
        if (panelToToggle == null) 
            return;

        isActive = panelToToggle.activeSelf;

        CloseAllPanels();

        if (!isActive)
        {
            panelToToggle.SetActive(true);
            currentlyActivePanel = panelToToggle;
        }
    }

    private void CloseAllPanels()
    {
        if (cardMenuPanel != null) 
            cardMenuPanel.SetActive(false);
        if (creditsPanel != null) 
            creditsPanel.SetActive(false);
        if (deckSelectionPanel != null) 
            deckSelectionPanel.SetActive(false);
        if (settingsPanel != null) 
            settingsPanel.SetActive(false);

        currentlyActivePanel = null;
    }

    public void DeckMenuButton()
    {
        TogglePanel(cardMenuPanel);
    }

    public void CreditsButton()
    {
        TogglePanel(creditsPanel);
    }

    public void SettingsButton()
    {
        isSettingsPanelActive = (settingsPanel != null && settingsPanel.activeSelf);

        CloseAllPanels();

        if (!isSettingsPanelActive && SettingsManager.Instance != null)
        {
            SettingsManager.Instance.ToggleOptionsPanel();
            currentlyActivePanel = settingsPanel; 
        }
    }

    public void StartButton()
    {
        TogglePanel(deckSelectionPanel);
        if (deckSelectionPanel != null && deckSelectionPanel.activeSelf)
        {
            DeckSelection();
        }
    }

    private void DeckSelection()
    {
        if (playGameButton != null) 
            playGameButton.gameObject.SetActive(false);
        currentlySelectedDeckForPlay = null;
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
            Debug.LogError("kein deck");
        }
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}