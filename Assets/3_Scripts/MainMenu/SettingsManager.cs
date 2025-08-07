using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [Header("Options-Panel")]
    [SerializeField] private GameObject optionsPanel;

    [Header("UI-Elemente")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Slider masterVolumeSlider;

    private Resolution[] resolutions;
    private bool isPanelActive = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (optionsPanel != null)
            {
                optionsPanel.SetActive(false);
            }

            SetupResolutions();
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPanelActive)
            {
                ToggleOptionsPanel();
            }
        }
    }

    public void ToggleOptionsPanel()
    {
        isPanelActive = !isPanelActive;
        optionsPanel.SetActive(isPanelActive);
        Time.timeScale = isPanelActive ? 0f : 1f;
    }

    public void SaveAndCloseOptions()
    {
        SaveSettings();
        if (isPanelActive)
        {
            ToggleOptionsPanel();
        }
    }

    public void SetMasterVolume(float volume)
    {
        AudioListener.volume = volume;
    }

    public void SetResolution(int resolutionIndex)
    {
        if (resolutions == null || resolutionIndex >= resolutions.Length) return;
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void SaveSettings()
    {
        if (resolutionDropdown != null)
        {
            PlayerPrefs.SetInt("ResolutionPreference", resolutionDropdown.value);
        }
        if (masterVolumeSlider != null)
        {
            PlayerPrefs.SetFloat("MasterVolumePreference", masterVolumeSlider.value);
        }
        PlayerPrefs.Save();
        Debug.Log("Settings saved!");
    }

    public void LoadSettings()
    {
        if (resolutionDropdown != null)
        {
            resolutionDropdown.value = PlayerPrefs.GetInt("ResolutionPreference", resolutionDropdown.value);
        }
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolumePreference", 1f);
            SetMasterVolume(masterVolumeSlider.value);
        }
    }

    private void SetupResolutions()
    {
        if (resolutionDropdown == null) return;
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        int currentResolutionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);
            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }
}