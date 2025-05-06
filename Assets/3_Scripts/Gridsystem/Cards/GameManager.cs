using UnityEngine;

public class GameManager : MonoBehaviour
{
    private void Start()
    {
        StartGame();
    }

    public void StartGame()
    {

        CardManager.Instance.DrawInitialCards();
        UnitManager.Instance.StartPlayerTurn();
    }
}