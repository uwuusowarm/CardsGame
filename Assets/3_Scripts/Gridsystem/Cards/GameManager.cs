using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    [SerializeField] private float enemyTurnDelay = 1f;
    
    [Header("Game Over")]
    [SerializeField] private CanvasGroup gameOverCanvasGroup;
    [SerializeField] private float gameOverFadeDuration = 1.5f;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool isFirstTurn = true;
    public bool IsPlayerTurn { get; private set; } = false;
    private bool isWaitingForPlayerActionResolution = false;
    private bool isGameOver = false;
    
    private bool attackAvailable = false;
    private int pendingAttackDamage = 0;
    private int pendingAttackRange = 0;

    private Unit playerUnit;

    public Unit PlayerUnit => playerUnit;
    
    int carryOverActionPoints = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(this);
        }

        if (gameOverCanvasGroup != null)
        {
            gameOverCanvasGroup.alpha = 0;
            gameOverCanvasGroup.interactable = false;
            gameOverCanvasGroup.blocksRaycasts = false;
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
        Debug.Log("Starting manager initialization check...");
    
        while (true)
        {
            bool allInitialized = true;
        
            if (CardManager.Instance == null) {
                Debug.Log("Waiting for CardManager...");
                allInitialized = false;
            }
        
            if (UnitManager.Instance == null) {
                Debug.Log("Waiting for UnitManager...");
                allInitialized = false;
            }
        
            if (HexGrid.Instance == null) {
                Debug.Log("Waiting for HexGrid...");
                allInitialized = false;
            }
        
            if (AttackManager.Instance == null) {
                Debug.Log("Waiting for AttackManager...");
                allInitialized = false;
            }
        
            if (PlayedCardEffectCache.Instance == null) {
                Debug.Log("Waiting for PlayedCardEffectCache...");
                allInitialized = false;
            }
        
            if (ExhaustionSystem.Instance == null) {
                Debug.Log("Waiting for ExhaustionSystem...");
                allInitialized = false;
            }
        
            if (ActionPointSystem.Instance == null) {
                Debug.Log("Waiting for ActionPointSystem...");
                allInitialized = false;
            }
        
            if (EquipmentManager.Instance == null) {
                Debug.Log("Waiting for EquipmentManager...");
                allInitialized = false;
            }
        
            if (PlayerDataManager.Instance == null || !PlayerDataManager.Instance.IsDataLoaded) {
                Debug.Log("Waiting for PlayerDataManager or data to load...");
                allInitialized = false;
            }

            if (allInitialized) {
                Debug.Log("All managers initialized successfully!");
                break;
            }

            yield return new WaitForSeconds(0.1f);
        }
    
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
        if (isGameOver) return;
        Debug.Log("Starting Player Turn.");
        IsPlayerTurn = true;
        isWaitingForPlayerActionResolution = false;
        playerUnit.shieldPoints = 0;
        playerUnit.ResetMovementPoints();
        
        attackAvailable = false;
        pendingAttackDamage = 0;
        pendingAttackRange = 0;

        if (PlayerStatusUI.Instance != null)
        {
            PlayerStatusUI.Instance.UpdateMovementPoints(playerUnit.MovementPoints);
            PlayerStatusUI.Instance.ClearAttackInfo();
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
        if (isGameOver) return;
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

        if (PlayedCardEffectCache.Instance.PendingDamage > 0)
        {
            attackAvailable = true;
            pendingAttackDamage = PlayedCardEffectCache.Instance.PendingDamage;
            pendingAttackRange = PlayedCardEffectCache.Instance.PendingRange;
            
            if(AttackManager.Instance != null)
            {
                AttackManager.Instance.PrepareAttack(pendingAttackDamage, pendingAttackRange);
            }
        }

        if(PlayedCardEffectCache.Instance != null)
        {
            PlayedCardEffectCache.Instance.ClearCache();
        }
        
        ActionPointSystem.Instance.UseActionPoints(1);
    }

    public bool IsAttackAvailable()
    {
        return attackAvailable;
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
            if (PlayerStatusUI.Instance != null)
            {
                PlayerStatusUI.Instance.UpdateMovementPoints(targetForSelfEffects.MovementPoints);
            }
            UnitManager.Instance.HandleUnitSelected(targetForSelfEffects.gameObject);
        }

        if (PlayedCardEffectCache.Instance.PendingDamage > 0)
        {
            Unit attacker = UnitManager.Instance.SelectedUnit;
            if (attacker == null) attacker = playerUnit;

            if (AttackManager.Instance != null && attacker != null)
            {
                int damageAmount = PlayedCardEffectCache.Instance.PendingDamage;
                int damageBonus = EquipmentManager.Instance.GetTotalDamageBonus();
                int totalDamage = damageAmount + damageBonus;
                
                int baseRange = PlayedCardEffectCache.Instance.PendingRange;
                int weaponRange = EquipmentManager.Instance.GetWeaponRange() - 1; // -1 because weapon range is absolute
                int totalRange = baseRange + weaponRange;

                Debug.Log($"Preparing attack from {attacker.name} with {totalDamage} damage (Base: {damageAmount}, Equipment: {damageBonus}) " +
                         $"and range {totalRange} (Base: {baseRange}, Weapon: {weaponRange + 1})");
                
                AttackManager.Instance.PrepareAttack(totalDamage, totalRange);
                if (PlayerStatusUI.Instance != null)
                {
                    PlayerStatusUI.Instance.UpdateAttackInfo(totalDamage, totalRange);
                }
            }
            else Debug.LogError("AttackManager or Attacker (SelectedUnit/PlayerUnit) is null. Cannot prepare attack.");
        }
    }

    private void ApplyImmediateEffect(CardEffect effect, Unit defaultTarget)
    {
        if (effect == null) return;

        switch (effect.effectType)
        {
            case CardEffect.EffectType.Draw:
                if (CardManager.Instance != null)
                {
                    Debug.Log($"Drawing {effect.value} card(s) from always effect.");
                    CardManager.Instance.DrawExtraCards(effect.value);
                }
                else
                {
                    Debug.LogError("CardManager.Instance is null. Cannot draw cards.");
                }
                return; 

            case CardEffect.EffectType.ActionPlus:
                if (ActionPointSystem.Instance != null)
                {
                    Debug.Log($"Gaining {effect.value} Action Point(s) from always effect.");
                    ActionPointSystem.Instance.AddActionPoints(effect.value);
                }
                else
                {
                    Debug.LogError("ActionPointSystem.Instance is null. Cannot add action points.");
                }
                return; 
            
            case CardEffect.EffectType.Block:
                if (ShieldSystem.Instance != null)
                {
                    Debug.Log($"Gaining {effect.value} Action Point(s) from always effect.");
                    ShieldSystem.Instance.AddShields(effect.value);
                }
                else
                {
                    Debug.LogError("ActionPointSystem.Instance is null. Cannot add action points.");
                }
                return; 
        }

        Unit effectiveTarget = defaultTarget;

        if (effectiveTarget == null)
        {
            Debug.LogWarning($"No effective target found for targeted immediate effect {effect.effectType}. Tried default: {defaultTarget?.name}");
            return;
        }

        switch (effect.effectType)
        {
            case CardEffect.EffectType.Attack:
                int totalDamage = effect.value + EquipmentManager.Instance.GetTotalDamageBonus();
                Debug.Log($"Applying direct Attack {totalDamage} (Base: {effect.value}, Equipment: {EquipmentManager.Instance.GetTotalDamageBonus()}) from {effectiveTarget.name}.");
                break;
                
            case CardEffect.EffectType.Move:
                int totalMove = effect.value + EquipmentManager.Instance.GetTotalMovementSpeedBonus();
                effectiveTarget.AddMovementPoints(totalMove);
                Debug.Log($"Added {totalMove} movement points (Base: {effect.value}, Equipment: {EquipmentManager.Instance.GetTotalMovementSpeedBonus()}) to {effectiveTarget.name}.");
                break;
                
            case CardEffect.EffectType.Heal:
                int totalHeal = effect.value + EquipmentManager.Instance.GetTotalHealBonus();
                effectiveTarget.Heal(totalHeal);
                Debug.Log($"Healed {effectiveTarget.name} for {totalHeal} (Base: {effect.value}, Equipment: {EquipmentManager.Instance.GetTotalHealBonus()}).");
                break;
                
            case CardEffect.EffectType.Block:
                int totalBlock = effect.value + EquipmentManager.Instance.GetTotalDefenseBonus();
                effectiveTarget.AddBlock(totalBlock);
                Debug.Log($"{effectiveTarget.name} gained {totalBlock} Block (Base: {effect.value}, Equipment: {EquipmentManager.Instance.GetTotalDefenseBonus()}).");
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
        if(PlayerStatusUI.Instance != null) PlayerStatusUI.Instance.ClearAttackInfo();
        
        attackAvailable = false;
        pendingAttackDamage = 0;
        pendingAttackRange = 0;
        
        if(AttackManager.Instance != null)
        {
            AttackManager.Instance.ClearHighlights();
        }
    }


    public void PlayerEndsTurn()
    {
        if (isGameOver) return;

        if (!IsPlayerTurn)
        {
            Debug.LogWarning("PlayerEndsTurn called, but it's not the player's turn.");
            return;
        }
        
        PlayerDataManager.Instance.SavePlayerState();
        if (attackAvailable)
        {
            attackAvailable = false;
            pendingAttackDamage = 0;
            pendingAttackRange = 0;
            if(AttackManager.Instance != null)
            {
                AttackManager.Instance.ClearHighlights();
            }
        }

        if (isWaitingForPlayerActionResolution)
        {
            Debug.LogWarning("Cannot end turn while waiting for action resolution.");
            if (PlayerStatusUI.Instance != null) PlayerStatusUI.Instance.ClearAttackInfo();
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
        if (isGameOver) yield break;
        Debug.Log("Starting Enemy Turn Routine.");

        foreach (var enemy in UnitManager.Instance.GetEnemyUnits())
        {
            if (isGameOver) yield break;
            Debug.Log($"Enemy {enemy.name} is taking its turn.");
            if (enemy != null)
            {
                Debug.Log($"Starting turn for enemy: {enemy.name}");
                yield return StartCoroutine(enemy.EnemyTurnRoutine());
                yield return new WaitForSeconds(enemyTurnDelay);
            }
        }
        if (!isGameOver)
        {
            ShieldSystem.Instance.LoseShields(100);
            StartPlayerTurn();
        }
    }

    public void HandlePlayerDeath()
    {
        if (isGameOver) return;

        isGameOver = true;
        IsPlayerTurn = false;
        Debug.Log("Game Over sequence initiated.");

        if (UnitManager.Instance?.SelectedUnit != null)
        {
            UnitManager.Instance.ClearOldSelection();
        }

        StopAllCoroutines();

        if (gameOverCanvasGroup != null)
        {
            StartCoroutine(FadeInGameOverScreen());
        }
        else
        {
            Debug.LogError("GameOverCanvasGroup is not assigned in the GameManager inspector!");
        }
    }

    private IEnumerator FadeInGameOverScreen()
    {
        float time = 0;
        gameOverCanvasGroup.gameObject.SetActive(true);
        while (time < gameOverFadeDuration)
        {
            gameOverCanvasGroup.alpha = Mathf.Lerp(0, 1, time / gameOverFadeDuration);
            time += Time.deltaTime;
            yield return null;
        }
        gameOverCanvasGroup.alpha = 1;
        gameOverCanvasGroup.interactable = true;
        gameOverCanvasGroup.blocksRaycasts = true;
    }

    public void ReturnToMainMenu()
    {
        Debug.Log($"Returning to Main Menu scene: {mainMenuSceneName}");
        SceneManager.LoadScene(mainMenuSceneName);
    }
}