using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio; 
using System.Collections.Generic;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [Header("Audio")]
    [SerializeField] private AudioMixer mainAudioMixer; 
    [SerializeField] private Slider musicVolumeSlider; 
    [SerializeField] private Slider sfxVolumeSlider;  

    [Header("Options Panel")]
    [SerializeField] private GameObject optionsPanel;

    [Header("Display")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;

    private Resolution[] resolutions;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            if (optionsPanel != null) optionsPanel.SetActive(false);
            Resolutions();
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetMusicVolume(float volume)
    {
        float decibelValue = Mathf.Log10(volume) * 20;
        Debug.Log("MUSIK-SLIDER: Setze MusicVolume auf " + decibelValue + " dB (Slider-Wert: " + volume + ")");
        mainAudioMixer.SetFloat("MusicVolume", decibelValue);
    }

    public void SetSFXVolume(float volume)
    {
        float decibelValue = Mathf.Log10(volume) * 20;
        Debug.Log("SFX-SLIDER: Setze SFXVolume auf " + decibelValue + " dB (Slider-Wert: " + volume + ")");
        mainAudioMixer.SetFloat("SFXVolume", decibelValue);
    }

    public void SaveSettings()
    {
        if (resolutionDropdown != null) PlayerPrefs.SetInt("Resolution", resolutionDropdown.value);
        if (musicVolumeSlider != null) PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);
        if (sfxVolumeSlider != null) PlayerPrefs.SetFloat("SFXVolume", sfxVolumeSlider.value);

        PlayerPrefs.Save();
    }

    public void LoadSettings()
    {
        if (resolutionDropdown != null)
        {
            int savedResolutionIndex = PlayerPrefs.GetInt("Resolution", -1);
            if (savedResolutionIndex != -1) resolutionDropdown.value = savedResolutionIndex;
        }

        if (musicVolumeSlider != null)
        {
            float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
            musicVolumeSlider.value = musicVolume;
            SetMusicVolume(musicVolume);
        }
        if (sfxVolumeSlider != null)
        {
            float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
            sfxVolumeSlider.value = sfxVolume;
            SetSFXVolume(sfxVolume);
        }
    }

    #region Unveränderter Code
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (optionsPanel.activeSelf) ToggleOptionsPanel();
        }
    }

    public void ToggleOptionsPanel()
    {
        if (optionsPanel == null) return;
        bool isPanelActive = !optionsPanel.activeSelf;
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
        if (optionsPanel.activeSelf) ToggleOptionsPanel();
    }

    public void SetResolution(int resolutionIndex)
    {
        if (resolutions == null || resolutionIndex >= resolutions.Length) return;
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    void Resolutions()
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
        int savedResolutionIndex = PlayerPrefs.GetInt("Resolution", currentResolutionIndex);
        resolutionDropdown.value = savedResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }
    #endregion
}