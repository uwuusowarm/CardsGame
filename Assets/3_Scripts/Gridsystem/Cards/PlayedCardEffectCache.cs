using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayedCardEffectCache : MonoBehaviour
{
    public static PlayedCardEffectCache Instance;
    
    public string PlayedCardName { get; private set; }
    public int PlayedCardManaCost { get; private set; }
    
    public int PendingDamage { get; private set; }
    public int PendingRange { get; private set; }
    public int PendingMovement { get; private set; }
    public int PendingHealing { get; private set; }
    public int PendingBlock { get; private set; }
    public bool HasPendingEffects { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void CacheCardEffect(CardData cardData, bool isLeft)
    {
        if (cardData == null)
        {
            Debug.LogError("CardData is null!");
            return;
        }

        ResetEffectKeepAttack();
            
        PlayedCardName = cardData.cardName;
        PlayedCardManaCost = cardData.manaCost;
        
        List<CardEffect> effectsToUse = isLeft ? cardData.leftEffects : cardData.rightEffects;
        
        Debug.Log($"Werte für Karte '{PlayedCardName}' (Mana: {PlayedCardManaCost}, Seite: {(isLeft ? "Links" : "Rechts")})  gespeichert.");

        foreach (var effect in effectsToUse)
        {
            if (effect == null) continue;

            HasPendingEffects = true;
            
            switch (effect.effectType)
            {
                case CardEffect.EffectType.Attack:
                    PendingDamage += effect.value;
                    PendingRange = Mathf.Max(PendingRange, effect.range);
                    break;
                case CardEffect.EffectType.Move:
                    PendingMovement = effect.value;
                    break;
                case CardEffect.EffectType.Heal:
                    PendingHealing = effect.value;
                    break;
                case CardEffect.EffectType.Block:
                    PendingBlock = effect.value;
                    break;
            }
        }
        
        Debug.Log($"After caching {PlayedCardName}: " + 
                  $"Damage {PendingDamage}, " + 
                  $"Range {PendingRange}, " +  
                  $"Movement {PendingMovement}, " +
                  $"Healing {PendingHealing}, " + 
                  $"Block {PendingBlock}");
    }

    private void ResetEffectValues()
    {
        PendingDamage = 0;
        PendingRange = 0;
        PendingMovement = 0;
        PendingHealing = 0;
        PendingBlock = 0;
        HasPendingEffects = false;
    }
    
    private void ResetEffectKeepAttack()
    {
        PendingMovement = 0;
        PendingHealing = 0;
        PendingBlock = 0;
        
        HasPendingEffects = (PendingDamage > 0);
    }

    public void ClearCache()
    {
        PlayedCardName = null;
        PlayedCardManaCost = 0;
        ResetEffectValues();
        Debug.Log("Cache geleert.");
    }

    public void PrintCachedEffects()
    {
        if (PendingDamage > 0)
        {
            Debug.Log($"Saved Damage {PendingDamage},Saved Range {PendingRange}");
        }
        if (PendingMovement > 0)
        {
            Debug.Log($"Saved Movement {PendingMovement}");
        }
        if (PendingHealing > 0)
        {
            Debug.Log($"Saved healing {PendingHealing}");
        }
        if (PendingBlock > 0)
        {
            Debug.Log($"Saved Cached {PendingBlock}");
        }
    }
    

}
