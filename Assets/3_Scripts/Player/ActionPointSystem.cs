using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ActionPointSystem : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int maxActionPoints = 4;

    [Header("Assigned UI Images")]
    [SerializeField] private List<Image> actionPointIcons = new List<Image>();

    private int currentActionPoints;
    private bool[] pointsUnlocked; 
    
    public static ActionPointSystem Instance { get; private set; }


    private void Start()
    {
        if (actionPointIcons.Count != maxActionPoints)
        {
            Debug.LogError($"Es werden genau {maxActionPoints} ActionPoint-Icons ben�tigt!");
            return;
        }

        pointsUnlocked = new bool[maxActionPoints];
        InitializeActionPoints(2); 
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.LogWarning("Another instance of ActionPointSystem found, destroying this one.");
            Destroy(gameObject);
        }
    }
    
    public void InitializeActionPoints(int startingPoints)
    {
        currentActionPoints = Mathf.Clamp(startingPoints, 0, maxActionPoints);
        for (int i = 0; i < startingPoints; i++)
        {
            pointsUnlocked[i] = true;
        }

        UpdateActionPointDisplay();
    }

    public void AddActionPoints(int amount)
    {
        int newPoints = Mathf.Min(currentActionPoints + amount, maxActionPoints);
        for (int i = currentActionPoints; i < newPoints; i++)
        {
            pointsUnlocked[i] = true;
        }

        currentActionPoints = newPoints;
        UpdateActionPointDisplay();
    }

    public void UseActionPoints(int amount)
    {
        currentActionPoints = Mathf.Max(currentActionPoints - amount, 0);
        UpdateActionPointDisplay();
    }

    public bool CanUseActionPoints(int amount)
    {
        return currentActionPoints >= amount;
    }

    private void UpdateActionPointDisplay()
    {
        for (int i = 0; i < actionPointIcons.Count; i++)
        {
            bool shouldShow = pointsUnlocked[i] && i < currentActionPoints;
            actionPointIcons[i].gameObject.SetActive(shouldShow);
        }
    }

    public int GetCurrentActionPoints()
    {
        return currentActionPoints;
    }
}
