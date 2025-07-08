using UnityEngine;
using TMPro;

public class PlayerStatsUI : MonoBehaviour
{
    public static PlayerStatsUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI movementPointsText;
    [SerializeField] private TextMeshProUGUI attackInfoText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateMovementPoints(0);
        ClearAttackInfo();
    }
    
    public void UpdateMovementPoints(int points)
    {
        if (movementPointsText != null)
        {
            movementPointsText.text = $"Movement: {points}";
        }
    }
    
    public void UpdateAttackInfo(int damage, int range)
    {
        if (attackInfoText != null)
        {
            attackInfoText.text = $"Attack: {damage}";
        }
    }
    
    public void ClearAttackInfo()
    {
        if (attackInfoText != null)
        {
            attackInfoText.text = "Attack: -";
        }
    }
    
}