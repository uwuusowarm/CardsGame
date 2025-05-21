using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


public class HealthSystem : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int maxBaseHealth = 5;
    [SerializeField] private int maxExtraHealth = 10; 

    [Header("Assigned UI Images")]
    [SerializeField] private List<Image> healthIcons = new List<Image>();

    private int currentHealth;
    private bool[] extraHealthUnlocked; 
    
    public static HealthSystem Instance { get; private set; }


    private void Start()
    {
        extraHealthUnlocked = new bool[maxExtraHealth];
        InitializeHealth(maxBaseHealth);
    }

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
    }
    public void InitializeHealth(int startingHealth)
    {
        currentHealth = startingHealth;
        UpdateHealthDisplay();
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
        currentHealth = Mathf.Max(currentHealth - amount, 0);
        UpdateHealthDisplay();
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

    public int GetCurrentHealth()
    {
        return currentHealth;
    }
}
