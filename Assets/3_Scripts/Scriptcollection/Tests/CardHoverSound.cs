using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(AudioSource))]
public class CardHoverSound : MonoBehaviour, IPointerEnterHandler
{
    public AudioClip hoverSound;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverSound != null)
        {
            Sound_Manager.instance.Play("Hover_V1");
        }
        else
        {
            Debug.LogWarning("kein sound", this.gameObject);
        }
    }
}