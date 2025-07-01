using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class SoundMusic
{
    public string Name;
    public AudioClip clip;

    [Range(0f, 5f)]
    public float volume;

    [Range(0f, 3f)]
    public float pitch;

    public bool loop;

    [HideInInspector]
    public AudioSource source;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
