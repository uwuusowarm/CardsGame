using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Windows;

public class MainMenu : MonoBehaviour
{

    [Header("Resolution")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    private GameObject resolution;
    private Resolution[] resolutions;
    public int targetFPS;
    public Text selectedFPS;
    private static MainMenu singleton;

    public void StartButton()
    {
        SceneManager.LoadSceneAsync(1);
    }

    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#endif
    }

    private void Start()
    {
        #region Resolution Dropdown
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
        #endregion
    }

    void Update()
    {
         string input = selectedFPS.text;


        if (int.TryParse(input, out int fps) && fps >= 30)
        {
            targetFPS = fps;
        }
        else
        {
            targetFPS = 30;
        }


        Application.targetFrameRate = targetFPS;
    }

    public void LoadSettings(int currentResolutionIndex)
    {
        resolutionDropdown.value = PlayerPrefs.HasKey("ResolutionPreference") ? PlayerPrefs.GetInt("ResolutionPreference") : currentResolutionIndex;
    }
}

