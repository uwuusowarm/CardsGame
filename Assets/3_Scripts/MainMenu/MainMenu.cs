using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Panel Management")]
    [Tooltip("Alle Panels, die von diesem Manager gesteuert werden sollen.")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject cardMenuPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject deckEditorSlideout;
    [SerializeField] private GameObject boostersSlideout;   

    [Header("Resolution")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    private Resolution[] resolutions;

    [Header("FPS Settings")]
    public int targetFPS;
    public Text selectedFPS;

    private static MainMenu singleton;
    private void Awake()
    {
        if (singleton == null)
            singleton = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        ReturnToMainMenu();

        #region Resolution Dropdown
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
        #endregion
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
        if (deckEditorSlideout != null) deckEditorSlideout.SetActive(false); 
        if (boostersSlideout != null) boostersSlideout.SetActive(false);  

        if (panelToShow != null)
        {
            panelToShow.SetActive(true);
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
        SceneManager.LoadScene(1);
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