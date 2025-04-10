using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardMovementController : MonoBehaviour
{
    public void OnCardDiscarded(Card card)
    {
        UnitManager.Instance.ActivateMovement();
    }
}
