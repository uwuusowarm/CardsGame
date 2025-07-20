using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject cardMenuPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject deckSelectionPanel;
    [SerializeField] private GameObject deckEditorSlideout;
    [SerializeField] private GameObject boostersSlideout;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Text selectedFPS;
    [SerializeField] private CardMenuManager cardMenuManager;
    [SerializeField] private Transform deckSelectionContainer;
    [SerializeField] private Button playGameButton;

    public int targetFPS;

    private Resolution[] resolutions;
    private Deck currentlySelectedDeckForPlay;
    private DeckUI currentlySelectedDeckUIForPlay;

    private void Awake()
    {
        if (FindObjectOfType<GameDataManager>() == null)
        {
            new GameObject("GameDataManager").AddComponent<GameDataManager>();
        }
    }

    private void Start()
    {
        ReturnToMainMenu();

        if (resolutionDropdown != null)
        {
            resolutionDropdown.ClearOptions();
            var options = new List<string>();
            resolutions = Screen.resolutions;
            var currentResolutionIndex = 0;
            for (var index = 0; index < resolutions.Length; index++)
            {
                var option = resolutions[index].width + " x " + resolutions[index].height;
                options.Add(option);
                if (resolutions[index].width == Screen.currentResolution.width && resolutions[index].height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = index;
                }
            }
            resolutionDropdown.AddOptions(options);
            resolutionDropdown.RefreshShownValue();
            LoadSettings(currentResolutionIndex);
        }
    }

    void Update()
    {
        if (selectedFPS != null && int.TryParse(selectedFPS.text, out int fps) && fps >= 30)
        {
            targetFPS = fps;
        }
        else
        {
            targetFPS = 30;
        }
        Application.targetFrameRate = targetFPS;
    }

    private void ShowPanel(GameObject panelToShow)
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
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
            SceneManager.LoadScene(2);
        }
        else
        {
            Debug.LogError("No deck selected or GameDataManager not found!");
        }
    }

    public void ReturnToMainMenu()
    {
        ShowPanel(mainPanel);
    }

    public void OpenOptionsPanel()
    {
        ShowPanel(optionsPanel);
    }

    public void OpenCardMenuPanel()
    {
        ShowPanel(cardMenuPanel);
    }

    public void OpenCreditsPanel()
    {
        ShowPanel(creditsPanel);
    }

    public void StartButton()
    {
        OpenDeckSelectionScreen();
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }


    public void LoadSettings(int currentResolutionIndex)
    {
        if (resolutionDropdown != null)
        {
            resolutionDropdown.value = PlayerPrefs.HasKey("ResolutionPreference") ? PlayerPrefs.GetInt("ResolutionPreference") : currentResolutionIndex;
        }
    }
}