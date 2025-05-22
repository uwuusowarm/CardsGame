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

    public void InitializeHealth(int startingHealth)
    {
        currentHealth = startingHealth;
        UpdateHealthDisplay();
    }
    public void Heal(int amount)
    {
        int maxPossibleHealth = maxBaseHealth;
        for (int i = 0; i < maxExtraHealth; i++)
        {
            if (extraHealthUnlocked[i])
                maxPossibleHealth++;
            else
                break; 
        }

        if (currentHealth < maxPossibleHealth)
        {
            int healAmount = Mathf.Min(amount, maxPossibleHealth - currentHealth);
            currentHealth += healAmount;
            UpdateHealthDisplay();
            Debug.Log($"Heal {healAmount}. Now: {currentHealth}/{maxPossibleHealth}");
        }
        else
        {
            Debug.Log("Full!");
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
