using UnityEngine;
using System.Collections.Generic;

public class PlayedCardEffectCache : MonoBehaviour
{
    public static PlayedCardEffectCache Instance;

    public int PendingDamage { get; private set; }
    public int PendingRange { get; private set; }
    public int PendingMovement { get; private set; }
    public int PendingHealing { get; private set; }
    public int PendingBlock { get; private set; }
    
    public bool HasPendingAttack => PendingDamage > 0;
    public bool HasPendingMovement => PendingMovement > 0;

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

    public void CacheCardEffect(CardData cardData, bool isLeft)
    {
        if (cardData == null) return;
        
        List<CardEffect> effectsToUse = isLeft ? cardData.leftEffects : cardData.rightEffects;
        Debug.Log($"Caching effects for card '{cardData.cardName}'...");

        foreach (var effect in effectsToUse)
        {
            if (effect == null) continue;

            switch (effect.effectType)
            {
                case CardEffect.EffectType.Attack:
                    PendingDamage = effect.value;
                    PendingRange = effect.range;
                    Debug.Log($"Attack armed: {PendingDamage} damage at range {PendingRange}");
                    break;
                case CardEffect.EffectType.Move:
                    PendingMovement += effect.value;
                    break;
                case CardEffect.EffectType.Heal:
                    PendingHealing += effect.value;
                    break;
                case CardEffect.EffectType.Block:
                    PendingBlock += effect.value;
                    break;
            }
        }
        PrintCachedEffects();
    }
    
    public void UseMovement(int amount)
    {
        PendingMovement = Mathf.Max(0, PendingMovement - amount);
    }

    public void ConsumeBlock()
    {
        PendingBlock = 0;
    }

    public void ConsumeHealing()
    {
        PendingHealing = 0;
    }

    public void ConsumeAttack()
    {
        PendingDamage = 0;
        PendingRange = 0;
    }

    public void ClearCacheForNewTurn()
    {
        PendingDamage = 0;
        PendingRange = 0;
        PendingMovement = 0;
        PendingHealing = 0;
        PendingBlock = 0;
        Debug.Log("Player action cache cleared for new turn.");
    }

    public void PrintCachedEffects()
    {
        Debug.Log($"CACHE STATE: [Dmg:{PendingDamage}, Rng:{PendingRange}, Move:{PendingMovement}, Heal:{PendingHealing}, Block:{PendingBlock}]");
    }
}
