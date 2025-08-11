using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class LevelRewardSystem : MonoBehaviour
{
    public static LevelRewardSystem Instance { get; private set; }

    [Header("Reward Settings")]
    [SerializeField] private CardDatabaseSO cardDatabase;
    [SerializeField] private int rewardCardsCount = 3;
    
    [Header("Rarity Chances (Base)")]
    [SerializeField] private float commonChance = 70f;
    [SerializeField] private float rareChance = 25f;
    [SerializeField] private float legendaryChance = 5f;
    [SerializeField] private float monsterChance = 0f;
    
    [Header("Enemy Kill Bonuses")]
    [SerializeField] private float rareChancePerKill = 2f;
    [SerializeField] private float legendaryChancePerKill = 1f;
    [SerializeField] private int minKillsForRareBonus = 3;
    [SerializeField] private int minKillsForLegendaryBonus = 8;
    
    private int enemiesKilled = 0;
    
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
    
    public void AddEnemyKill()
    {
        enemiesKilled++;
        Debug.Log($"Enemy killed! Total kills: {enemiesKilled}");
    }
    
    public void ResetKillCount()
    {
        enemiesKilled = 0;
    }
    
    public List<CardData> GenerateRewards()
    {
        List<CardData> rewards = new List<CardData>();
        List<CardData> usedCards = new List<CardData>(); 
    
        if (cardDatabase == null || cardDatabase._allCards == null)
        {
            Debug.LogError("Card database is not assigned or empty!");
            return rewards;
        }
    
        float modifiedCommonChance = commonChance;
        float modifiedRareChance = rareChance;
        float modifiedLegendaryChance = legendaryChance;
    
        if (enemiesKilled >= minKillsForRareBonus)
        {
            float killBonus = (enemiesKilled - minKillsForRareBonus + 1) * rareChancePerKill;
            modifiedRareChance += killBonus;
            modifiedCommonChance -= killBonus * 0.7f; 
        }
    
        if (enemiesKilled >= minKillsForLegendaryBonus)
        {
            float killBonus = (enemiesKilled - minKillsForLegendaryBonus + 1) * legendaryChancePerKill;
            modifiedLegendaryChance += killBonus;
            modifiedCommonChance -= killBonus * 0.5f; 
        }
    
        modifiedCommonChance = Mathf.Max(modifiedCommonChance, 10f);
        modifiedRareChance = Mathf.Clamp(modifiedRareChance, 0f, 80f);
        modifiedLegendaryChance = Mathf.Clamp(modifiedLegendaryChance, 0f, 40f);
    
        Debug.Log($"Reward chances - Common: {modifiedCommonChance}%, Rare: {modifiedRareChance}%, Legendary: {modifiedLegendaryChance}%");
    
        for (int i = 0; i < rewardCardsCount; i++)
        {
            CardRarity targetRarity = DetermineRarity(modifiedCommonChance, modifiedRareChance, modifiedLegendaryChance);
            CardData rewardCard = GetRandomUniqueCardOfRarity(targetRarity, usedCards);
        
            if (rewardCard != null)
            {
                rewards.Add(rewardCard);
                usedCards.Add(rewardCard);
                Debug.Log($"Generated reward {i + 1}: {rewardCard.cardName} ({rewardCard.rarity})");
            }
            else
            {
                Debug.LogWarning($"Could not find unique card for reward {i + 1}");
            }
        }
    
        return rewards;
    }

    private CardData GetRandomUniqueCardOfRarity(CardRarity targetRarity, List<CardData> usedCards)
    {
        List<CardData> availableCards = cardDatabase._allCards
            .Where(card => card.rarity == targetRarity && !usedCards.Contains(card))
            .ToList();
    
        if (availableCards.Count == 0)
        {
            Debug.LogWarning($"No unique cards found for rarity {targetRarity}, falling back to Common");
            availableCards = cardDatabase._allCards
                .Where(card => card.rarity == CardRarity.Common && !usedCards.Contains(card))
                .ToList();
        }
    
        if (availableCards.Count == 0)
        {
            Debug.LogWarning("No unique common cards found, returning any unique card");
            availableCards = cardDatabase._allCards
                .Where(card => !usedCards.Contains(card))
                .ToList();
        }
    
        if (availableCards.Count > 0)
        {
            return availableCards[Random.Range(0, availableCards.Count)];
        }
    
        Debug.LogError("No unique cards available in database!");
        return null;
    }
    
    private CardRarity DetermineRarity(float commonChance, float rareChance, float legendaryChance)
    {
        float randomValue = Random.Range(0f, 100f);
        
        if (randomValue <= legendaryChance)
            return CardRarity.Legendary;
        else if (randomValue <= legendaryChance + rareChance)
            return CardRarity.Rare;
        else
            return CardRarity.Common;
    }
    
    public int GetEnemiesKilled()
    {
        return enemiesKilled;
    }
}