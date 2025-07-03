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
    private int carryOverActionPoints = 0;

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

        if (playerUnit != null)
        {
            playerUnit.shieldPoints = 0;
            playerUnit.movementPoints = 0;
        }

        if (ActionPointSystem.Instance != null)
        {
            if (isFirstTurn)
            {
                int startingAP = 2 + EquipmentManager.Instance.GetTotalAPBonus();
                ActionPointSystem.Instance.InitializeActionPoints(startingAP);
            }
            else
            {
                ActionPointSystem.Instance.ResetActionPoints();
                int turnAP = 2 + EquipmentManager.Instance.GetTotalAPBonus();
                ActionPointSystem.Instance.AddActionPoints(turnAP);
            }

            if (carryOverActionPoints > 0)
            {
                ActionPointSystem.Instance.AddActionPoints(1);
                carryOverActionPoints = 0;
                Debug.Log("Added 1 carried over action point");
            }
        }

        if (isFirstTurn)
        {
            Debug.Log("First turn of the game.");
            isFirstTurn = false;
            CardManager.Instance.DrawInitialCards();
        }
        else
        {
            DrawPlayerCards();
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
        if (ActionPointSystem.Instance.GetCurrentActionPoints() <= 0)
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
        }
        else
        {
            Debug.LogError("CardManager.Instance is null. Cannot move card to discard.");
        }

        if (!PlayedCardEffectCache.Instance.HasPendingEffects || !IsAttackPending())
        {
            if (PlayedCardEffectCache.Instance != null)
            {
                PlayedCardEffectCache.Instance.ClearCache();
                PlayedCardEffectCache.Instance.PrintCachedEffects();
            }
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
            int blockAmount = PlayedCardEffectCache.Instance.PendingBlock;
            int defenseBonus = EquipmentManager.Instance.GetTotalDefenseBonus();
            int totalBlock = blockAmount + defenseBonus;
            targetForSelfEffects.AddBlock(totalBlock);
            Debug.Log($"Player gained {totalBlock} Block (Base: {blockAmount}, Equipment: {defenseBonus})");
        }

        if (PlayedCardEffectCache.Instance.PendingHealing > 0 && targetForSelfEffects != null)
        {
            int healAmount = PlayedCardEffectCache.Instance.PendingHealing;
            int healBonus = EquipmentManager.Instance.GetTotalHealBonus();
            int totalHeal = healAmount + healBonus;
            targetForSelfEffects.Heal(totalHeal);
            Debug.Log($"Player healed for {totalHeal} (Base: {healAmount}, Equipment: {healBonus})");
        }

        if (PlayedCardEffectCache.Instance.PendingMovement > 0 && targetForSelfEffects != null)
        {
            int moveAmount = PlayedCardEffectCache.Instance.PendingMovement;
            int moveBonus = EquipmentManager.Instance.GetTotalMovementSpeedBonus();
            int totalMove = moveAmount + moveBonus;
            targetForSelfEffects.AddMovementPoints(totalMove);
            Debug.Log($"Player gained {totalMove} Movement Points (Base: {moveAmount}, Equipment: {moveBonus})");
            UnitManager.Instance.HandleUnitSelected(targetForSelfEffects.gameObject);
        }

        if (PlayedCardEffectCache.Instance.PendingDamage > 0)
        {
            Unit attacker = UnitManager.Instance.SelectedUnit ?? playerUnit;

            if (AttackManager.Instance != null)
            {
                int damageAmount = PlayedCardEffectCache.Instance.PendingDamage;
                int damageBonus = EquipmentManager.Instance.GetTotalDamageBonus();
                int totalDamage = damageAmount + damageBonus;

                int baseRange = PlayedCardEffectCache.Instance.PendingRange;
                int weaponRange = EquipmentManager.Instance.GetWeaponRange() - 1;
                int totalRange = baseRange + weaponRange;

                Debug.Log($"Preparing attack from {attacker.name} with {totalDamage} damage and range {totalRange}");

                AttackManager.Instance.PrepareAttack(totalDamage, totalRange);
                isWaitingForPlayerActionResolution = true;
            }
            else
            {
                Debug.LogError("AttackManager is null. Cannot prepare attack.");
            }
        }
    }

    private void ApplyImmediateEffect(CardEffect effect, Unit defaultTarget)
    {
        if (effect == null || defaultTarget == null) return;

        switch (effect.effectType)
        {
            case CardEffect.EffectType.Attack:
                int totalDamage = effect.value + EquipmentManager.Instance.GetTotalDamageBonus();
                Debug.Log($"Applying direct Attack {totalDamage} from {defaultTarget.name}.");
                break;

            case CardEffect.EffectType.Move:
                int totalMove = effect.value + EquipmentManager.Instance.GetTotalMovementSpeedBonus();
                defaultTarget.AddMovementPoints(totalMove);
                Debug.Log($"Added {totalMove} movement points to {defaultTarget.name}.");
                break;

            case CardEffect.EffectType.Heal:
                int totalHeal = effect.value + EquipmentManager.Instance.GetTotalHealBonus();
                defaultTarget.Heal(totalHeal);
                Debug.Log($"Healed {defaultTarget.name} for {totalHeal}.");
                break;

            case CardEffect.EffectType.Block:
                int totalBlock = effect.value + EquipmentManager.Instance.GetTotalDefenseBonus();
                defaultTarget.AddBlock(totalBlock);
                Debug.Log($"{defaultTarget.name} gained {totalBlock} Block.");
                break;
        }
    }

    private bool IsAttackPending()
    {
        if (PlayedCardEffectCache.Instance == null) return false;
        return PlayedCardEffectCache.Instance.PendingDamage > 0 && isWaitingForPlayerActionResolution;
    }

    public void PlayerActionResolved(bool actionWasCompleted)
    {
        Debug.Log($"Player action resolved. Completed: {actionWasCompleted}");
        isWaitingForPlayerActionResolution = false;

        if (PlayedCardEffectCache.Instance != null) PlayedCardEffectCache.Instance.ClearCache();
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
            if (PlayedCardEffectCache.Instance != null) PlayedCardEffectCache.Instance.ClearCache();
            return;
        }

        if (ActionPointSystem.Instance != null)
        {
            carryOverActionPoints = Mathf.Min(ActionPointSystem.Instance.GetCurrentActionPoints(), 1);
        }

        Debug.Log("Player initiated end of turn.");
        IsPlayerTurn = false;
        StartCoroutine(EnemyTurnRoutine());
    }

    private IEnumerator EnemyTurnRoutine()
    {
        Debug.Log("Starting Enemy Turn Routine.");

        if (UnitManager.Instance != null)
        {
            var enemies = UnitManager.Instance.GetEnemyUnits();
            foreach (var enemy in enemies)
            {
                if (enemy != null)
                {
                    Debug.Log($"Starting turn for enemy: {enemy.name}");
                    yield return StartCoroutine(enemy.EnemyTurnRoutine());
                    yield return new WaitForSeconds(enemyTurnDelay);
                }
            }
        }

        ShieldSystem.Instance?.LoseShields(100);
        StartPlayerTurn();
    }
}