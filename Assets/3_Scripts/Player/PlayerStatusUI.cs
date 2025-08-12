using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerStatusUI : MonoBehaviour
{
    public static PlayerStatusUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI movementPointsText;
    [SerializeField] private TextMeshProUGUI attackInfoText;
    [SerializeField] private TextMeshProUGUI exhaustLevelText;
    
    [Header("Attack Effect Icons (Auto-Found)")]
    private GameObject stunIconContainer;
    private GameObject poisonIconContainer;
    private Image stunIcon;
    private Image poisonIcon;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            FindAttackEffectIcons();
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
        UpdateExhaustLevel(0);
        HideAttackEffectIcons();
    }
    
    private void FindAttackEffectIcons()
    {
        GameObject stunIconObject = GameObject.Find("StunIcon");
        if (stunIconObject != null)
        {
            stunIconContainer = stunIconObject;
            stunIcon = stunIconObject.GetComponent<Image>();
            Debug.Log("Found StunIcon successfully");
        }
        else
        {
            Debug.LogWarning("StunIcon GameObject not found in scene");
        }
        
        GameObject poisonIconObject = GameObject.Find("PoisonIcon");
        if (poisonIconObject != null)
        {
            poisonIconContainer = poisonIconObject;
            poisonIcon = poisonIconObject.GetComponent<Image>();
            Debug.Log("Found PoisonIcon successfully");
        }
        else
        {
            Debug.LogWarning("PoisonIcon GameObject not found in scene");
        }
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
        if (exhaustLevelText != null)
        {
            exhaustLevelText.text = $"{level}";
        }
    }
    
    public void UpdateAttackInfo(int damage, int range)
    {
        if (attackInfoText != null)
        {
            attackInfoText.text = $"{damage}";
        }
        
        UpdateAttackEffectIcons();
    }
    
    public void ClearAttackInfo()
    {
        if (attackInfoText != null)
        {
            attackInfoText.text = " ";
        }
        
        HideAttackEffectIcons();
    }
    
    private void UpdateAttackEffectIcons()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsStunAttackActive())
        {
            ShowStunIcon();
        }
        else
        {
            HideStunIcon();
        }
        
        if (GameManager.Instance != null && GameManager.Instance.IsPoisonAttackActive())
        {
            ShowPoisonIcon();
        }
        else
        {
            HidePoisonIcon();
        }
    }
    
    private void ShowStunIcon()
    {
        if (stunIconContainer != null)
        {
            stunIconContainer.SetActive(true);
            Debug.Log("Showing stun icon");
        }
    }
    
    private void HideStunIcon()
    {
        if (stunIconContainer != null)
        {
            stunIconContainer.SetActive(false);
        }
    }
    
    private void ShowPoisonIcon()
    {
        if (poisonIconContainer != null)
        {
            poisonIconContainer.SetActive(true);
            Debug.Log("Showing poison icon");
        }
    }
    
    private void HidePoisonIcon()
    {
        if (poisonIconContainer != null)
        {
            poisonIconContainer.SetActive(false);
        }
    }
    
    private void HideAttackEffectIcons()
    {
        HideStunIcon();
        HidePoisonIcon();
    }
    
    public void RefreshAttackEffectIcons()
    {
        UpdateAttackEffectIcons();
    }
}