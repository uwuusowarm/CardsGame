using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; set; }

    [Header("Options panel")]
    [SerializeField] GameObject optionsPanel;

    [Header("UI")]
    [SerializeField] TMP_Dropdown resolutionDropdown;
    [SerializeField] Slider masterVolumeSlider;

    Resolution[] resolutions;
    bool isPanelActive = false;
    public int _saveResolutionIndex;
    public int _currentResolutionIndex = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (optionsPanel != null)
            {
                optionsPanel.SetActive(false);
            }

            Resolutions();
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (optionsPanel.activeSelf) 
            {
                ToggleOptionsPanel();
            }
        }
    }

    public void ToggleOptionsPanel()
    {
        if (optionsPanel == null) 
            return;

        isPanelActive = !optionsPanel.activeSelf;
        optionsPanel.SetActive(isPanelActive);
        Time.timeScale = isPanelActive ? 0f : 1f;
    }
    public GameObject GetOptionsPanelObject()
    {
        return optionsPanel;
    }

    public void SaveAndCloseOptions()
    {
        SaveSettings();
        if (optionsPanel.activeSelf)
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
        if (resolutions == null || resolutionIndex >= resolutions.Length) 
            return;
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void SaveSettings()
    {
        if (resolutionDropdown != null)
        {
            PlayerPrefs.SetInt("Resolution", resolutionDropdown.value);
        }
        if (masterVolumeSlider != null)
        {
            PlayerPrefs.SetFloat("MasterVolume", masterVolumeSlider.value);
        }
        PlayerPrefs.Save();
//        Debug.Log("Settings saved");
    }

    public void LoadSettings()
    {
        if (resolutionDropdown != null)
        {
            _saveResolutionIndex = PlayerPrefs.GetInt("Resolution", -1);
            if (_saveResolutionIndex != -1)
            {
                resolutionDropdown.value = _saveResolutionIndex;
            }
        }

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
            SetMasterVolume(masterVolumeSlider.value);
        }
    }

    //shit sollte jz gehen wenn nicht spring ich
    void Resolutions()
    {
        if (resolutionDropdown == null) 
            return;
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        for (int res = 0; res < resolutions.Length; res++)
        {
            string option = resolutions[res].width + " x " + resolutions[res].height;
            options.Add(option);
            if (resolutions[res].width == Screen.currentResolution.width &&
                resolutions[res].height == Screen.currentResolution.height)
            {
                _currentResolutionIndex = res; 
            }
        }
        resolutionDropdown.AddOptions(options);

        _saveResolutionIndex = PlayerPrefs.GetInt("ResolutionPreference", _currentResolutionIndex);
        resolutionDropdown.value = _saveResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }
}