using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsQuit : MonoBehaviour
{
    public void QuitGame()
    {
        SceneManager.LoadScene(0);
    }
}
