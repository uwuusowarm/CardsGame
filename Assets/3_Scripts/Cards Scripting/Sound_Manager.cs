using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Audio;


public class Sound_Manager : MonoBehaviour
{
    public SoundMusic[] sounds; 
    public static Sound_Manager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        foreach (SoundMusic s in sounds) 
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
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
}