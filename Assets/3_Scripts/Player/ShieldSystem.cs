using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ShieldSystem : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int maxShields = 11;

    [Header("Assigned UI Images")]
    [SerializeField] private List<Image> shieldIcons = new List<Image>();

    private int currentShields;
    private bool[] shieldsUnlocked;

    private void Start()
    {
        shieldsUnlocked = new bool[maxShields];
        InitializeShields(0); 
    }

    public void InitializeShields(int startingShields)
    {
        currentShields = startingShields;

        for (int i = 0; i < startingShields; i++)
        {
            if (i < shieldsUnlocked.Length)
            {
                shieldsUnlocked[i] = true;
            }
        }

        UpdateShieldDisplay();
    }

    public void AddShields(int amount)
    {
        int newShields = Mathf.Min(currentShields + amount, maxShields);
        for (int i = currentShields; i < newShields; i++)
        {
            if (i < shieldsUnlocked.Length)
            {
                shieldsUnlocked[i] = true;
            }
        }

        currentShields = newShields;
        UpdateShieldDisplay();
    }

    public void LoseShields(int amount)
    {
        currentShields = Mathf.Max(currentShields - amount, 0);
        UpdateShieldDisplay();
    }

    private void UpdateShieldDisplay()
    {
        for (int i = 0; i < shieldIcons.Count; i++)
        {
            bool shouldShow = i < shieldsUnlocked.Length && shieldsUnlocked[i];
            shieldIcons[i].gameObject.SetActive(shouldShow && i < currentShields);
        }
    }

    public int GetCurrentShields()
    {
        return currentShields;
    }
}
