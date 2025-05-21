using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

using UnityEngine;

public class StairsToMenu : MonoBehaviour
{
    private void OnMouseDown()
    {
        SceneManager.LoadScene(0);
    }
}
