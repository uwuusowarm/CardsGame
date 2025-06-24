/*
 *     ▄████████  ▄██████▄     ▄███████▄ ▄██   ▄      ▄████████  ▄█     ▄██████▄     ▄█    █▄        ███          ▀█████████▄  ▄██   ▄               
 *    ███    ███ ███    ███   ███    ███ ███   ██▄   ███    ███ ███    ███    ███   ███    ███   ▀█████████▄        ███    ███ ███   ██▄             
 *    ███    █▀  ███    ███   ███    ███ ███▄▄▄███   ███    ███ ███▌   ███    █▀    ███    ███      ▀███▀▀██        ███    ███ ███▄▄▄███             
 *    ███        ███    ███   ███    ███ ▀▀▀▀▀▀███  ▄███▄▄▄▄██▀ ███▌  ▄███         ▄███▄▄▄▄███▄▄     ███   ▀       ▄███▄▄▄██▀  ▀▀▀▀▀▀███             
 *    ███        ███    ███ ▀█████████▀  ▄██   ███ ▀▀███▀▀▀▀▀   ███▌ ▀▀███ ████▄  ▀▀███▀▀▀▀███▀      ███          ▀▀███▀▀▀██▄  ▄██   ███             
 *    ███    █▄  ███    ███   ███        ███   ███ ▀███████████ ███    ███    ███   ███    ███       ███            ███    ██▄ ███   ███             
 *    ███    ███ ███    ███   ███        ███   ███   ███    ███ ███    ███    ███   ███    ███       ███            ███    ███ ███   ███             
 *    ████████▀   ▀██████▀   ▄████▀       ▀█████▀    ███    ███ █▀     ████████▀    ███    █▀       ▄████▀        ▄█████████▀   ▀█████▀              
 *                                                   ███    ███                                                                                      
 *       ▄█   ▄█▄ ███▄▄▄▄    ▄█   ▄█        ▄████████    ▄█    █▄     ▄██████▄  ███▄▄▄▄      ▄████████ ███    █▄      ███        ▄████████ ███▄▄▄▄   
 *      ███ ▄███▀ ███▀▀▀██▄ ███  ███       ███    ███   ███    ███   ███    ███ ███▀▀▀██▄   ███    ███ ███    ███ ▀█████████▄   ███    ███ ███▀▀▀██▄ 
 *      ███▐██▀   ███   ███ ███▌ ███       ███    █▀    ███    ███   ███    ███ ███   ███   ███    ███ ███    ███    ▀███▀▀██   ███    █▀  ███   ███ 
 *     ▄█████▀    ███   ███ ███▌ ███       ███         ▄███▄▄▄▄███▄▄ ███    ███ ███   ███   ███    ███ ███    ███     ███   ▀  ▄███▄▄▄     ███   ███ 
 *    ▀▀█████▄    ███   ███ ███▌ ███       ███        ▀▀███▀▀▀▀███▀  ███    ███ ███   ███ ▀███████████ ███    ███     ███     ▀▀███▀▀▀     ███   ███ 
 *      ███▐██▄   ███   ███ ███  ███       ███    █▄    ███    ███   ███    ███ ███   ███   ███    ███ ███    ███     ███       ███    █▄  ███   ███ 
 *      ███ ▀███▄ ███   ███ ███  ███▌    ▄ ███    ███   ███    ███   ███    ███ ███   ███   ███    ███ ███    ███     ███       ███    ███ ███   ███ 
 *      ███   ▀█▀  ▀█   █▀  █▀   █████▄▄██ ████████▀    ███    █▀     ▀██████▀   ▀█   █▀    ███    █▀  ████████▀     ▄████▀     ██████████  ▀█   █▀  
 *      ▀                        ▀                                                                                                                   
 */

using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using TMPro;

public class SplashController : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public TMP_Text skipText;
    public string nextSceneName;

    private bool canSkip;
    private bool hasSeenVideo;

    void Start()
    {
        hasSeenVideo = PlayerPrefs.GetInt("HasSeenIntro", 0) == 1;

        canSkip = hasSeenVideo;

        if (skipText != null)
            skipText.gameObject.SetActive(canSkip == true);

        videoPlayer.loopPointReached += OnVideoFinished;

        videoPlayer.Play();
    }

    void Update()
    {
        if (canSkip && videoPlayer.isPlaying)
        {
            if (Input.GetMouseButtonDown(0))
            {
                videoPlayer.Stop();
                OnVideoFinished(videoPlayer);
            }
        }
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        PlayerPrefs.SetInt("HasSeenIntro", 1);
        PlayerPrefs.Save();

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("Keine Zielszene für den Szenenwechsel angegeben.");
        }
    }
}
