using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class EndTurnButton : MonoBehaviour
{
    private Button endTurnButton;

    void Awake()
    {
        endTurnButton = GetComponent<Button>();
    }

    void Start()
    {
        if (GameManager.Instance != null)
        {

            endTurnButton.onClick.AddListener(GameManager.Instance.PlayerEndsTurn);
        }
        else
        {
            Debug.LogError("EndTurnButton could not find the GameManager.Instance! Disabling button.");
            endTurnButton.interactable = false;
        }
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            endTurnButton.onClick.RemoveListener(GameManager.Instance.PlayerEndsTurn);
        }
    }
}