using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class Sound_Manager : MonoBehaviour
{
    public static Sound_Manager instance;

    [Header("Mixer Groups")]
    public AudioMixerGroup musicMixerGroup; 
    public AudioMixerGroup sfxMixerGroup;  

    [Header("Sound Lists")]
    public SoundMusic[] sounds;
    public SoundPicker[] soundPicker;

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

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string sceneName = scene.name;
        foreach (SoundMusic s in sounds)
        {
            if (s.soundType == SoundType.Music && s.source.isPlaying)
            {
                s.source.Stop();
            }
        }
        switch (sceneName)
        {
            case "MainMenu":
                Play("Main_Menu_Music");
                break;
            default:
                Play("Level_Music");
                break;
        }
    }

    public void Play(string name)
    {
        SoundMusic s = Array.Find(sounds, sound => sound.Name == name);
        if (s == null)
        {
            Debug.LogError($"Sound '{name}' not found in Sound Manager!");
            return;
        }
        s.source.Play();
    }

    public void SoundPlay()
    {
        if (SceneManager.GetActiveScene().name == "Level3")
        {
            Play("Level_Music");
        }
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            Play("Main_Menu_Music");
        }
    }


}