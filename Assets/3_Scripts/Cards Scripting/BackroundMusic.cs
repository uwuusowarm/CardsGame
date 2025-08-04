using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackroundMusic : MonoBehaviour
{
    public AudioClip clip;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.Play();
    }
}
