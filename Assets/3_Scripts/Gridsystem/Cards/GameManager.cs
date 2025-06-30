using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    [SerializeField] private float enemyTurnDelay = 1f;

    private bool isFirstTurn = true;
    public bool IsPlayerTurn { get; private set; } = false;
    private bool isWaitingForPlayerActionResolution = false;

    private Unit playerUnit; 
    
    int carryOverActionPoints = ActionPointSystem.Instance.GetCurrentActionPoints();

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        GameObject playerGameObject = GameObject.FindGameObjectWithTag("Player");
        if (playerGameObject != null)
        {
            playerUnit = playerGameObject.GetComponent<Unit>();
            if (playerUnit == null)
            {
                Debug.LogError("GameObject with tag 'Player' does not have a Unit component!");
            }
            else
            {
                Debug.Log("Player Unit found and assigned: " + playerUnit.name);
            }
        }
        else
        {
            Debug.LogError("No GameObject with tag 'Player' found in scene!");
        }

        StartCoroutine(WaitForManagersAndStartGame());
    }

    private IEnumerator WaitForManagersAndStartGame()
    {
        yield return new WaitUntil(() => CardManager.Instance != null &&
                                         UnitManager.Instance != null &&
                                         HexGrid.Instance != null &&
                                         AttackManager.Instance != null &&
                                         PlayedCardEffectCache.Instance != null &&
                                         ExhaustionSystem.Instance != null &&
                                         playerUnit != null);
        StartGame();
    }

    public void StartGame()
    {
        Debug.Log("Starting Game");
        isFirstTurn = true;
        StartPlayerTurn();
    }

    public void StartPlayerTurn()
    {
        Debug.Log("Starting Player Turn.");
        IsPlayerTurn = true;
        isWaitingForPlayerActionResolution = false;
        playerUnit.shieldPoints = 0;
        playerUnit.movementPoints = 0;

        ActionPointSystem.Instance.ResetActionPoints();
        ActionPointSystem.Instance.AddActionPoints(2); 
        
        if (carryOverActionPoints > 0)
        {
            ActionPointSystem.Instance.AddActionPoints(1); 
            carryOverActionPoints = 0;
            Debug.Log("Added 1 carried over action point");
        }
        
        if (isFirstTurn)
        {
            Debug.Log("First turn of the game.");
            isFirstTurn = false;
            CardManager.Instance.DrawInitialCards();
        }
    }

    private void DrawPlayerCards()
    {
        if (CardManager.Instance == null)
        {
            Debug.LogError("CardManager.Instance is null for card draw.");
            return;
        }
        Debug.Log("Drawing new cards based on exhaustion level.");
        CardManager.Instance.DrawCards(CardManager.Instance.DrawCount);
    }

    public void ProcessPlayedCard(CardData cardData, bool isLeftEffectChosen)
    {
        int AP = ActionPointSystem.Instance.GetCurrentActionPoints();
        
        if (AP <= 0)
        {
            Debug.LogWarning("Cannot process card. No action points left.");
            return;    
        }
        
        if (!IsPlayerTurn || isWaitingForPlayerActionResolution)
        {
            Debug.LogWarning("Cannot process card. Not player's turn or waiting for action resolution.");
            return;
        }
        if (cardData == null)
        {
            Debug.LogError("cardData is null in ProcessPlayedCard.");
            return;
        }

        if (PlayedCardEffectCache.Instance == null)
        {
            Debug.LogError("PlayedCardEffectCache.Instance is null.");
            return;
        }
        PlayedCardEffectCache.Instance.CacheCardEffect(cardData, isLeftEffectChosen);

        if (playerUnit != null && UnitManager.Instance != null)
        {
            UnitManager.Instance.HandleUnitSelected(playerUnit.gameObject);
        }
       
        if (cardData.alwaysEffects != null)
        {
            foreach (var effect in cardData.alwaysEffects)
            {
                ApplyImmediateEffect(effect, playerUnit); 
            }
        }

        ApplyCachedEffects();

        if (CardManager.Instance != null)
        {
            CardManager.Instance.MoveToZone(cardData, DropType.Discard);
            UnitManager.Instance.ReduceActionPoints(playerUnit, 1);
        }
        else
        {
            Debug.LogError("CardManager.Instance is null. Cannot move card to discard.");
        }

        if (!PlayedCardEffectCache.Instance.HasPendingEffects || !IsAttackPending())
        {
            if(PlayedCardEffectCache.Instance != null) PlayedCardEffectCache.Instance.ClearCache();
            PlayedCardEffectCache.Instance.PrintCachedEffects();
        }
        else
        {
            isWaitingForPlayerActionResolution = true;
            Debug.Log("Waiting for player to select a target or resolve action.");
        }
        
        ActionPointSystem.Instance.UseActionPoints(1);
    }

    private void ApplyCachedEffects()
    {
        if (PlayedCardEffectCache.Instance == null || !PlayedCardEffectCache.Instance.HasPendingEffects) return;

        Unit targetForSelfEffects = playerUnit; 

        if (PlayedCardEffectCache.Instance.PendingBlock > 0 && targetForSelfEffects != null)
        {
            targetForSelfEffects.AddBlock(PlayedCardEffectCache.Instance.PendingBlock);
            Debug.Log($"Player gained {PlayedCardEffectCache.Instance.PendingBlock} Block.");
        }
        if (PlayedCardEffectCache.Instance.PendingHealing > 0 && targetForSelfEffects != null)
        {
            targetForSelfEffects.Heal(PlayedCardEffectCache.Instance.PendingHealing);
            Debug.Log($"Player healed for {PlayedCardEffectCache.Instance.PendingHealing}.");
        }
        if (PlayedCardEffectCache.Instance.PendingMovement > 0 && targetForSelfEffects != null)
        {
            targetForSelfEffects.AddMovementPoints(PlayedCardEffectCache.Instance.PendingMovement);
            Debug.Log($"Player gained {PlayedCardEffectCache.Instance.PendingMovement} Movement Points.");
            UnitManager.Instance.HandleUnitSelected(targetForSelfEffects.gameObject);
        }

        if (PlayedCardEffectCache.Instance.PendingDamage > 0)
        {
            Unit attacker = UnitManager.Instance.SelectedUnit;
            if (attacker == null) attacker = playerUnit; 

            if (AttackManager.Instance != null && attacker != null)
            {
                Debug.Log($"Preparing attack from {attacker.name} with {PlayedCardEffectCache.Instance.PendingDamage} damage and range {PlayedCardEffectCache.Instance.PendingRange}.");
                AttackManager.Instance.PrepareAttack(
                    PlayedCardEffectCache.Instance.PendingDamage,
                    PlayedCardEffectCache.Instance.PendingRange
                );
                isWaitingForPlayerActionResolution = true;
            }
            else Debug.LogError("AttackManager or Attacker (SelectedUnit/PlayerUnit) is null. Cannot prepare attack.");
        }
    }
    
    private void ApplyImmediateEffect(CardEffect effect, Unit defaultTarget)
    {
        if (effect == null) return;

        Unit effectiveTarget = defaultTarget; 

        if (effectiveTarget == null) {
            Debug.LogWarning($"No effective target found for immediate effect {effect.effectType}. Tried default: {defaultTarget?.name}");
            return;
        }

        switch (effect.effectType)
        {
            case CardEffect.EffectType.Attack:
                Debug.Log($"Applying direct Attack {effect.value} from {effectiveTarget.name}.");
                break;
            case CardEffect.EffectType.Move:
                effectiveTarget.AddMovementPoints(effect.value);
                Debug.Log($"Added {effect.value} movement points to {effectiveTarget.name}.");
                break;
            case CardEffect.EffectType.Heal:
                effectiveTarget.Heal(effect.value);
                Debug.Log($"Healed {effectiveTarget.name} for {effect.value}.");
                break;
            case CardEffect.EffectType.Block:
                effectiveTarget.AddBlock(effect.value);
                Debug.Log($"{effectiveTarget.name} gained {effect.value} Block.");
                break;
        }
    }

    private bool IsAttackPending()
    {
        if(PlayedCardEffectCache.Instance == null) return false;
        return PlayedCardEffectCache.Instance.PendingDamage > 0 && isWaitingForPlayerActionResolution;
    }

    public void PlayerActionResolved(bool actionWasCompleted)
    {
        Debug.Log($"Player action resolved. Completed: {actionWasCompleted}");
        isWaitingForPlayerActionResolution = false;
        
        if(PlayedCardEffectCache.Instance != null) PlayedCardEffectCache.Instance.ClearCache();
    }

    public void PlayerEndsTurn()
    {
        if (!IsPlayerTurn)
        {
            Debug.LogWarning("PlayerEndsTurn called, but it's not the player's turn.");
            return;
        }
        if (isWaitingForPlayerActionResolution)
        {
            Debug.LogWarning("Cannot end turn while waiting for action resolution.");
            AttackManager.Instance?.ClearHighlights();
            isWaitingForPlayerActionResolution = false;
            if(PlayedCardEffectCache.Instance != null) PlayedCardEffectCache.Instance.ClearCache();
            return;
        }
        
        int currentAP = ActionPointSystem.Instance.GetCurrentActionPoints();
        carryOverActionPoints = Mathf.Min(currentAP, 1);

        Debug.Log("Player initiated end of turn.");
        IsPlayerTurn = false;
        StartCoroutine(EnemyTurnRoutine());
    }

    private IEnumerator EnemyTurnRoutine()
    {
        Debug.Log("Starting Enemy Turn Routine.");

        /*if (UnitManager.Instance == null)
        {
            Debug.LogError("UnitManager.Instance is null. Enemies cannot take their turn.");
            StartPlayerTurn();
            yield break;
        }*/

        foreach (var enemy in UnitManager.Instance.GetEnemyUnits())
        {
            Debug.Log($"Enemy {enemy.name} is taking its turn.");
            if (enemy != null)
            {
                Debug.Log($"Starting turn for enemy: {enemy.name}");
                yield return StartCoroutine(enemy.EnemyTurnRoutine());
                yield return new WaitForSeconds(enemyTurnDelay);
            }
        }
        ShieldSystem.Instance.LoseShields(100);
        StartPlayerTurn();
    }
}