using UnityEngine;
using System.Collections; 

[RequireComponent(typeof(CardDisplay))]
public class CardState : MonoBehaviour
{
    public DropType currentZone = DropType.Hand;
    private CardDisplay cardDisplay;
    private Coroutine _graveyardCoroutine = null;

    void Start()
    {
        cardDisplay = GetComponent<CardDisplay>();
        DropZone startingZone = GetComponentInParent<DropZone>();
        if (startingZone != null)
        {
            currentZone = startingZone.zoneType;
        }
    }

    public void StartDelayedMoveToGraveyard(float delay)
    {
        StopGraveyardTimer();
        _graveyardCoroutine = StartCoroutine(MoveToGraveyardAfterDelay(delay));
         Debug.Log($"'{cardDisplay?.card?.cardName ?? name}' startet {delay}s Timer zum Friedhof.");
    }

    private IEnumerator MoveToGraveyardAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (currentZone == DropType.Player1Ablage || currentZone == DropType.Player2Ablage)
        {
             SendToGraveyard();
        } else {
             Debug.Log($"'{cardDisplay?.card?.cardName ?? name}' ist nicht mehr in AblageZone, Friedhof-Move abgebrochen.");
        }
        _graveyardCoroutine = null; 
    }

    public void StopGraveyardTimer()
    {
        if (_graveyardCoroutine != null)
        {
            StopCoroutine(_graveyardCoroutine);
            _graveyardCoroutine = null;
            Debug.Log($"'{cardDisplay?.card?.cardName ?? name}' Friedhof-Timer gestoppt.");
        }
    }


    public void SendToGraveyard()
    {
        StopGraveyardTimer();

        if (currentZone != DropType.Graveyard)
        {
            Debug.Log($"Karte '{cardDisplay?.card?.cardName ?? name}' von Zone '{currentZone}' zum Friedhof gesendet.");
            UnitManager.Instance.ActivateMovement();
            GameObject graveyardObject = GameObject.Find("GraveyardZone");

            if (graveyardObject != null)
            {
                Transform graveyard = graveyardObject.transform;
                transform.SetParent(graveyard);
                currentZone = DropType.Graveyard; 

                RectTransform rect = GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchoredPosition = Vector2.zero;
                    rect.localRotation = Quaternion.identity;
                    rect.localScale = Vector3.one;
                }
            }
            else
            {
                Debug.LogError("Konnte 'GraveyardZone' nicht finden (per Tag oder Name)!");
            }
        }
        else
        {
            Debug.Log($"Karte '{cardDisplay?.card?.cardName ?? name}' ist bereits im Friedhof.");
        }
    }
}