using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("Panel Management")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject cardMenuPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject deckSelectionPanel;

    [Header("Resolution")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    private Resolution[] resolutions;

    [Header("FPS Settings")]
    public int targetFPS;
    public Text selectedFPS;

    [Header("Wichtige Referenzen")]
    [SerializeField] private CardMenuManager cardMenuManager;
    [SerializeField] private Transform deckSelectionContainer;
    [SerializeField] private Button playGameButton;

    private Deck currentlySelectedDeckForPlay;

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
            for (var i = 0; i < resolutions.Length; i++)
            {
                var option = resolutions[i].width + " x " + resolutions[i].height;
                options.Add(option);
                if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = i;
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

        if (panelToShow != null)
        {
            panelToShow.SetActive(true);
        }
    }

    public void OpenDeckSelectionScreen()
    {
        ShowPanel(deckSelectionPanel);
        playGameButton.gameObject.SetActive(false);
        currentlySelectedDeckForPlay = null;

        foreach (Transform child in deckSelectionContainer) Destroy(child.gameObject);

        List<Deck> playerDecks = cardMenuManager.GetPlayerDecks();
        GameObject deckDisplayPrefab = cardMenuManager.deckDisplayPrefab;

        foreach (var deck in playerDecks)
        {
            GameObject deckGO = Instantiate(deckDisplayPrefab, deckSelectionContainer);
            DeckUI deckUI = deckGO.GetComponent<DeckUI>();
            if (deckUI != null)
            {
                deckUI.Initialize(deck, (selectedDeck) => {
                    SelectDeckForPlay(selectedDeck);
                });
            }
        }
    }

    private void SelectDeckForPlay(Deck deck)
    {
        currentlySelectedDeckForPlay = deck;
        playGameButton.gameObject.SetActive(true);
    }

    public void LaunchGame()
    {
        if (currentlySelectedDeckForPlay != null && GameDataManager.Instance != null)
        {
            GameDataManager.Instance.selectedDeck = currentlySelectedDeckForPlay;
            SceneManager.LoadScene(1);
        }
        else
        {
            Debug.LogError("Kein Deck ausgewählt oder GameDataManager nicht gefunden!");
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