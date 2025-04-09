using UnityEngine;

public class CardState : MonoBehaviour
{
    public DropType currentZone;
    private CardDisplay cardDisplay;

    void Start()
    {
        cardDisplay = GetComponent<CardDisplay>();
    }

    public void UseCard()
    {
        if (currentZone == DropType.Player1Ablage || currentZone == DropType.Player2Ablage)
        {
            Debug.Log("Karte gespielt ? Friedhof");

            Transform graveyard = GameObject.Find("GraveyardZone").transform;
            transform.SetParent(graveyard);
            currentZone = DropType.Graveyard;

        }
    }
}

