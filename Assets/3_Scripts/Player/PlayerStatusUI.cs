using UnityEngine;
using TMPro;

public class PlayerStatusUI : MonoBehaviour
{
    public static PlayerStatusUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI movementPointsText;
    [SerializeField] private TextMeshProUGUI attackInfoText;
    [SerializeField] private TextMeshProUGUI exhaustLevelText;


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
            movementPointsText.text = $"{points}";
        }
    }
    public void UpdateExhaustLevel(int level)
    {
        if (movementPointsText != null)
        {
            movementPointsText.text = $"{level}";
        }
    }
    
    public void UpdateAttackInfo(int damage, int range)
    {
        if (attackInfoText != null)
        {
            attackInfoText.text = $"{damage}";
        }
    }
    
    public void ClearAttackInfo()
    {
        if (attackInfoText != null)
        {
            attackInfoText.text = " ";
        }
    }
    
}