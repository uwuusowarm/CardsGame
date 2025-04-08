using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Main_Manu : MonoBehaviour
{
  public void startGame()
    {
        SceneManager.LoadScene(0);
    }

    public void OptionGame()
    {
        SceneManager.LoadScene(1);
    }

    public void CreditsGame()
    {
        SceneManager.LoadScene(2);
    }

    public void exitGame()
    {
        Application.Quit();
    }

    public void DeckGame()
    {
        SceneManager.LoadScene(3);
    }

    public void BackGame()
    {
        SceneManager.LoadScene(4);
    }

    public void Deck1()
    {
        SceneManager.LoadScene(5);
    }

}

