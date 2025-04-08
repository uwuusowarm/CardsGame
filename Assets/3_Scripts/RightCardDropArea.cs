using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RightCardDropArea : MonoBehaviour, ICardDropArea
{
    [SerializeField] private GameObject objectToSpawn;

    public void OnCardDrop(Card card)
    {
        Destroy(card.gameObject);     
        Instantiate(objectToSpawn, transform.position, transform.rotation);
    }
}
