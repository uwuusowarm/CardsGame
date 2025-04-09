using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftCardDropArea : MonoBehaviour, ICardDropArea
{
   
    public void OnCardDrop(Card card)
    {
        card.transform.position = transform.position;
        Debug.Log("Karte wurde platziert!");
    }
}
