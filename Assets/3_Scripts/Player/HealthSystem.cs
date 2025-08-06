using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HealthSystem : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int maxBaseHealth = 5;
    [SerializeField] private int maxExtraHealth = 10;
    [SerializeField] private GameObject healVFXPrefab;
    [SerializeField] private float vfxDuration;
    [SerializeField] private Vector3 healVFXOffset = new Vector3(0, -1, 0); // Offset for VFX position  

    [Header("Assigned UI Images")]
    [SerializeField] private List<Image> healthIcons = new List<Image>();

    private int currentHealth;
    private bool[] extraHealthUnlocked;
    public static HealthSystem Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
        
        extraHealthUnlocked = new bool[maxExtraHealth];
    }
    
    private void Start()
    {
        InitializeHealth(maxBaseHealth);
    }
    
    public void InitializeHealth(int startingHealth)
    {
        currentHealth = startingHealth;
        UpdateHealthDisplay();
    }

    public void UpdateMaxHealth()
    {
        int totalMaxHealth = maxBaseHealth;
    
        if (EquipmentManager.Instance != null)
        {
            totalMaxHealth += EquipmentManager.Instance.GetTotalMaxHPBonus();
        }
    
        if (extraHealthUnlocked != null)
        {
            for (int i = 0; i < extraHealthUnlocked.Length; i++)
            {
                if (extraHealthUnlocked[i])
                {
                    totalMaxHealth++;
                }
            }
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null && playerObj.TryGetComponent<Unit>(out var playerUnit))
        {
            playerUnit.maxHealth = totalMaxHealth;
        }

        if (currentHealth > totalMaxHealth)
        {
            currentHealth = totalMaxHealth;
        }

        UpdateHealthDisplay();
    }


    public int GetCurrentHealth()
    {
        return currentHealth;
    }
    
    public int GetMaxHealth()
    {
        int maxPossibleHealth = maxBaseHealth;
    
        if (EquipmentManager.Instance != null)
        {
            maxPossibleHealth += EquipmentManager.Instance.GetTotalMaxHPBonus();
        }
    
        if (extraHealthUnlocked != null)
        {
            for (int i = 0; i < maxExtraHealth; i++)
            {
                if (extraHealthUnlocked[i])
                    maxPossibleHealth++;
                else
                    break;
            }
        }
    
        return maxPossibleHealth;
    }

    
    public int GetUnlockedExtraHealthCount()
    {
        int count = 0;
        if (extraHealthUnlocked != null)
        {
            for (int i = 0; i < extraHealthUnlocked.Length; i++)
            {
                if (extraHealthUnlocked[i])
                {
                    count++;
                }
            }
        }
        return count;
    }

    public void Heal(int amount)
    {
        int maxPossibleHealth = GetMaxHealth();
        HealVFX(GameObject.FindGameObjectWithTag("Player").transform.position);
        if (currentHealth < maxPossibleHealth)
        {
            int healAmount = Mathf.Min(amount, maxPossibleHealth - currentHealth);
            currentHealth += healAmount;
        
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null && playerObj.TryGetComponent<Unit>(out var playerUnit))
            {
                playerUnit.currentHealth = currentHealth;
            }
        
            UpdateHealthDisplay();
            Debug.Log($"Heal {healAmount}. Now: {currentHealth}/{maxPossibleHealth}");
        }
        else
        {
            Debug.Log("Full!");
        }
        
    }

    public void HealVFX(Vector3 playerPos)
    {
        if (healVFXPrefab != null)
        {
            GameObject vfxInstance = Instantiate(healVFXPrefab, playerPos + healVFXOffset, Quaternion.identity);
            Destroy(vfxInstance, vfxDuration); // Destroy after a set duration
            
        }
    }


    public void UnlockExtraHealth(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            int nextSlot = -1;
            for (int j = 0; j < maxExtraHealth; j++)
            {
                if (!extraHealthUnlocked[j])
                {
                    nextSlot = j;
                    break;
                }
            }
            if (nextSlot != -1)
            {
                extraHealthUnlocked[nextSlot] = true;
                Debug.Log($"Extra! (Slot {nextSlot})");
            }
            else
            {
                Debug.Log("No Extra!");
                break;
            }
        }
        UpdateMaxHealth();
    }

    public void AddHealth(int amount)
    {
        int newHealth = Mathf.Min(currentHealth + amount, maxBaseHealth + maxExtraHealth);
        for (int i = currentHealth; i < newHealth; i++)
        {
            if (i >= maxBaseHealth)
            {
                int extraIndex = i - maxBaseHealth;
                extraHealthUnlocked[extraIndex] = true;
            }
        }

        currentHealth = newHealth;
        UpdateHealthDisplay();
    }

    public void LoseHealth(int amount)
    {
        if (currentHealth <= 0) return;

        currentHealth = Mathf.Max(currentHealth - amount, 0);
        UpdateHealthDisplay();

        if (currentHealth <= 0)
        {
            Debug.Log("Player health depleted. Signaling game over.");
            if (GameManager.Instance != null)
            {
                GameManager.Instance.HandlePlayerDeath();
            }
        }
    }

    private void UpdateHealthDisplay()
    {
        for (int i = 0; i < healthIcons.Count; i++)
        {
            if (i < maxBaseHealth)
            {
                healthIcons[i].gameObject.SetActive(true);
                healthIcons[i].color = i < currentHealth ? Color.white : Color.gray;
            }
            else if (i < maxBaseHealth + maxExtraHealth)
            {
                int extraIndex = i - maxBaseHealth;
                bool shouldShow = extraHealthUnlocked[extraIndex];
                healthIcons[i].gameObject.SetActive(shouldShow);

                if (shouldShow)
                {
                    healthIcons[i].color = i < currentHealth ? Color.white : Color.gray;
                }
            }
        }
    }
}