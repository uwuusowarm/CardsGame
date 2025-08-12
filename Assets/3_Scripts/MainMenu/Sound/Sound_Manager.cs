using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class Sound_Manager : MonoBehaviour
{
    public static Sound_Manager instance;

    [Header("Mixer Groups")]
    public AudioMixerGroup musicMixerGroup;
    public AudioMixerGroup sfxMixerGroup;

    [Header("Sound Lists")]
    public SoundMusic[] sounds;
    public SoundPicker[] soundPicker;

    #region bin mir fast sicher 100% GPT code von Adrian oder Jan

    private void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(gameObject); return; }
        DontDestroyOnLoad(gameObject);

        foreach (SoundMusic s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;

            switch (s.soundType)
            {
                case SoundType.Music:
                    s.source.outputAudioMixerGroup = musicMixerGroup;
                    break;
                case SoundType.SFX:
                    s.source.outputAudioMixerGroup = sfxMixerGroup;
                    break;
            }
        }
    }

    public void PlayRandomFromGroup(string groupName)
    {
        SoundPicker picker = Array.Find(soundPicker, p => p.soundgroup == groupName);
        if (picker == null || picker.clips.Length == 0)
        {
            Debug.LogError($"Sound group '{groupName}' not found or empty!");
            return;
        }

        AudioClip clip = picker.clips[UnityEngine.Random.Range(0, picker.clips.Length)];
        AudioSource tempSource = gameObject.AddComponent<AudioSource>();
        tempSource.clip = clip;

        tempSource.outputAudioMixerGroup = sfxMixerGroup;

        tempSource.Play();
        Destroy(tempSource, clip.length);
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            Play("Main_Menu_Music");
        }

        // Beim **nächsten** Szenenwechsel: Musik einmalig stoppen
        UnityAction<Scene, Scene> stopOnNextScene = null;
        stopOnNextScene = (oldScene, newScene) =>
        {
            if (oldScene.name == "MainMenu")
            {
                foreach (var s in sounds)
                {
                    if (s.soundType == SoundType.Music && s.source != null && s.source.isPlaying)
                    s.source.Stop();
                }

            }
            // Stoppe die Musik, wenn die Szene "MainMenu" geladen wird
            if (newScene.name == "")
            {
                foreach (var s in sounds)
                {
                    if (s.soundType == SoundType.Music && s.source != null && s.source.isPlaying)
                        s.source.Stop();
                }
            }
            // Nur einmal ausführen
            SceneManager.activeSceneChanged -= stopOnNextScene;
        };
        SceneManager.activeSceneChanged += stopOnNextScene;

        
        SceneManager.sceneLoaded += (scene, mode) =>
        {
            if (scene.name == "MainMenu")
            {
                Play("Main_Menu_Music");
            }
            else
            {
                Play("Level_Music");
            }
        };
        

    }

    public void Play(string name)
    {
        SoundMusic s = Array.Find(sounds, sound => sound.Name == name);
        if (s == null)
        {
            Debug.LogError($"Sound '{name}' not found in Sound Manager!");
            return;
        }

        // Wenn Musik: erst alle anderen Musikquellen stoppen, dann spielen
        if (s.soundType == SoundType.Music)
        {
            foreach (var m in sounds)
            {
                if (m.soundType == SoundType.Music && m.source != null && m.source.isPlaying && m != s)
                    m.source.Stop();
            }
            s.source.loop = true; // BG-Musik sollte loopen
        }

        s.source.Play();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Add logic here to handle what happens when a scene is loaded
        if (scene.name == "MainMenu")
        {
            Play("Main_Menu_Music");
        }
        else if (scene.name == "17")
        {
            Play("Level_Music");
        }
    }

    #endregion

    public void SoundPlay()
    {
        if (SceneManager.GetActiveScene().name == "17")
        {
            Play("Level_Music");
        }
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            Play("Main_Menu_Music");
        }
    }
}
