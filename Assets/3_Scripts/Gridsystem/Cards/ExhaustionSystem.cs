using System.Collections.Generic;
using UnityEngine;

public class ExhaustionSystem : MonoBehaviour
{
    private int exhaustionStacks = 0;
    
    public static ExhaustionSystem Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void AddExhaustionStack()
    {
        exhaustionStacks++;
        Debug.Log($"Added exhaustion stack. Total stacks: {exhaustionStacks}");
    }

    public void ExhaustCards(List<CardData> deck)
    {
        if (exhaustionStacks <= 0) return;

        int cardsToRemove = Mathf.Min(exhaustionStacks, deck.Count);
        for (int i = 0; i < cardsToRemove; i++)
        {
            int randomIndex = Random.Range(0, deck.Count);
            deck.RemoveAt(randomIndex);
        }
        
        Debug.Log($"Removed {cardsToRemove} random cards from deck due to exhaustion");
    }

}