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
    [SerializeField] private Transform SelectionContainer;
    [SerializeField] private Button playGameButton;

    private GameObject settingsPanel;
    private GameObject ActivePanel;

    private Deck SelectedDeckPlay;
    private DeckUI SelectedDeckUI;
    public bool isSettingsPanelActive = false;
    public bool Active = false;

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

        Active = panelToToggle.activeSelf;

        CloseAllPanels();

        if (!Active)
        {
            panelToToggle.SetActive(true);
            ActivePanel = panelToToggle;
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

        ActivePanel = null;
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
            ActivePanel = settingsPanel; 
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
        SelectedDeckPlay = null;
        SelectedDeckPlay = null;
        if (SelectedDeckUI != null)
        {
            SelectedDeckUI.SetHighlight(false);
            SelectedDeckUI = null;
        }

        foreach (Transform child in SelectionContainer)
        {
            Destroy(child.gameObject);
        }

        List<Deck> playerDecks = cardMenuManager.GetPlayerDecks();
        GameObject deckDisplayPrefab = cardMenuManager.DeckDisplayPrefab;

        foreach (var deck in playerDecks)
        {
            GameObject deckGameObject = Instantiate(deckDisplayPrefab, SelectionContainer);
            DeckUI deckUI = deckGameObject.GetComponent<DeckUI>();
            if (deckUI != null)
            {
                deckUI.Initialize
                    (deck, (selectedUI) => 
                {
                    SelectDeckForPlay(selectedUI);
                }
                );
            }
        }
    }

    private void SelectDeckForPlay(DeckUI selectedUI)
    {
        if (SelectedDeckUI == selectedUI)
        {
            selectedUI.SetHighlight(false);
            SelectedDeckUI = null;
            SelectedDeckPlay = null;
            if (playGameButton != null) 
                playGameButton.gameObject.SetActive(false);
        }
        else
        {
            if (SelectedDeckUI != null)
            {
                SelectedDeckUI.SetHighlight(false);
            }
            SelectedDeckUI = selectedUI;
            SelectedDeckPlay = selectedUI.GetAssignedDeck();
            SelectedDeckUI.SetHighlight(true);
            if (playGameButton != null) 
                playGameButton.gameObject.SetActive(true);
        }
    }

    public void LaunchGame()
    {
        if (SelectedDeckPlay != null && GameDataManager.Instance != null)
        {
            GameDataManager.Instance.selectedDeck = SelectedDeckPlay;
            SceneManager.LoadScene(5);
        }
/*        else
        {
            Debug.LogError("kein deck");
        }
*/
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}