using UnityEngine;
using UnityEngine.Audio;

public enum SoundType { Music, SFX }

[System.Serializable]
public class SoundMusic
{
    public string Name;
    public AudioClip clip;

    public SoundType soundType;

    [Range(0f, 1f)] 
    public float volume = 1f;

    [Range(0.1f, 3f)]
    public float pitch = 1f;

    public bool loop;

    [HideInInspector]
    public AudioSource source;
}