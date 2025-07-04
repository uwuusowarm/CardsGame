using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverImageHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject hoverImage;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverImage != null)
            hoverImage.SetActive(true);
      
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hoverImage != null)
            hoverImage.SetActive(false);
    }
}
