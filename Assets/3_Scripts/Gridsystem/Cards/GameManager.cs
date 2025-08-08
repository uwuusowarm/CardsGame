using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")] [SerializeField]
    private float enemyTurnDelay = 1f;


    [Header("Game Over")] [SerializeField] private CanvasGroup gameOverCanvasGroup;
    [SerializeField] private float gameOverFadeDuration = 1.5f;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool isFirstTurn = true;
    public bool IsPlayerTurn { get; private set; } = false;
    private bool isWaitingForPlayerActionResolution = false;
    private bool isGameOver = false;

    private bool attackAvailable = false;
    private int pendingAttackDamage = 0;
    private int pendingAttackRange = 0;

    private bool poisonAttackActive = false;
    private int pendingPoisonDuration = 0;

    public Animator animDead;
    
    private Unit playerUnit;
    public Unit PlayerUnit => playerUnit;

    int carryOverActionPoints = 0;

    private void Awake()
    {
        Debug.Log($"GameManager Awake() called on object '{gameObject.name}' in scene '{gameObject.scene.name}'");

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            Debug.Log($"GameManager Instance SET to '{gameObject.name}'. It is now persistent.");
        }
        else if (Instance != this)
        {
            Debug.LogWarning(
                $"Duplicate GameManager found on '{gameObject.name}'. The original is '{Instance.gameObject.name}'. Destroying the duplicate.");
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        Debug.LogWarning($"GameManager OnDestroy() called for object '{gameObject.name}'. Was this intentional?");
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }


    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != mainMenuSceneName)
        {
            StartCoroutine(InitializeLevel());
        }
        else
        {
            isGameOver = false;
            gameOverCanvasGroup = null;
        }
    }

    private IEnumerator InitializeLevel()
    {
        yield return null;

        isGameOver = false;
        playerUnit = null;
        gameOverCanvasGroup = null;

        GameObject gameOverUIObject = GameObject.FindGameObjectWithTag("GameOverCanvas");
        if (gameOverUIObject != null)
        {
            gameOverCanvasGroup = gameOverUIObject.GetComponent<CanvasGroup>();
            if (gameOverCanvasGroup != null)
            {
                gameOverCanvasGroup.alpha = 0;
                gameOverCanvasGroup.interactable = false;
                gameOverCanvasGroup.blocksRaycasts = false;
                Debug.Log("GameManager: Found and initialized GameOverCanvasGroup.");
            }
            else
            {
                Debug.LogError(
                    "GameManager: GameObject with tag 'GameOverCanvas' found, but it's missing a CanvasGroup component!");
            }
        }
        else
        {
            Debug.LogWarning(
                "GameManager: Could not find GameObject with tag 'GameOverCanvas' in this scene. The game over screen may not function.");
        }

        GameObject playerGameObject = null;
        float searchTimeout = 5f;
        float searchTimer = 0f;
        while (playerGameObject == null && searchTimer < searchTimeout)
        {
            playerGameObject = GameObject.FindGameObjectWithTag("Player");
            if (playerGameObject == null)
            {
                yield return new WaitForSeconds(0.1f);
                searchTimer += 0.1f;
            }
        }

        if (playerGameObject == null)
        {
            Debug.LogError(
                "FATAL: Player object with tag 'Player' not found in scene after timeout. Game cannot start.");
            yield break;
        }

        playerUnit = playerGameObject.GetComponent<Unit>();
        if (playerUnit == null)
        {
            Debug.LogError("FATAL: GameObject with tag 'Player' does not have a Unit component! Game cannot start.");
            yield break;
        }

        Debug.Log("GameManager: Player Unit found and assigned: " + playerUnit.name);

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

        if (AttackManager.Instance != null)
        {
            AttackManager.Instance.ResetAttackValues();
        }

        if (PlayedCardEffectCache.Instance != null)
        {
            PlayedCardEffectCache.Instance.ClearCache();
        }

        if (PlayerStatusUI.Instance != null)
        {
            PlayerStatusUI.Instance.UpdateMovementPoints(playerUnit.MovementPoints);
            PlayerStatusUI.Instance.ClearAttackInfo();
        }

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

        ActionPointSystem.Instance.UseActionPoints(1);
    }

    public bool IsAttackAvailable()
    {
        return attackAvailable;
    }

    public void CheckForEnemiesInRange()
    {
        if (IsAttackAvailable() && AttackManager.Instance != null)
        {
            AttackManager.Instance.TryPrepareAttack();
        }
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
            
            CheckForEnemiesInRange();
        }

        if (PlayedCardEffectCache.Instance.PendingDamage > 0)
        {
            Unit attacker = UnitManager.Instance.SelectedUnit;
            if (attacker == null) attacker = playerUnit;

            if (AttackManager.Instance != null && attacker != null)
            {
                attackAvailable = true;

                int damageAmount = PlayedCardEffectCache.Instance.PendingDamage;
                int damageBonus = EquipmentManager.Instance.GetTotalDamageBonus();
                int totalDamage = damageAmount + damageBonus;

                int baseRange = PlayedCardEffectCache.Instance.PendingRange;
                int weaponRange = EquipmentManager.Instance.GetWeaponRange() - 1; // -1 because weapon range is absolute
                int totalRange = baseRange + weaponRange;

                Debug.Log(
                    $"Preparing attack from {attacker.name} with {totalDamage} damage (Base: {damageAmount}, Equipment: {damageBonus}) " +
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

            case CardEffect.EffectType.Burn:
                ApplyBurnEffect(effect.value, effect.range);
                return;

            case CardEffect.EffectType.Stun:
                ApplyStunEffect(effect.value);
                return;

            case CardEffect.EffectType.Poison:
                poisonAttackActive = true;
                pendingPoisonDuration = effect.value;
                Debug.Log($"Next attack will poison the target for {effect.value} turns");
                return;
        }

        Unit effectiveTarget = defaultTarget;

        if (effectiveTarget == null)
        {
            Debug.LogWarning(
                $"No effective target found for targeted immediate effect {effect.effectType}. Tried default: {defaultTarget?.name}");
            return;
        }

        switch (effect.effectType)
        {
            case CardEffect.EffectType.Attack:
                int totalDamage = effect.value + EquipmentManager.Instance.GetTotalDamageBonus();
                Debug.Log(
                    $"Applying direct Attack {totalDamage} (Base: {effect.value}, Equipment: {EquipmentManager.Instance.GetTotalDamageBonus()}) from {effectiveTarget.name}.");
                break;

            case CardEffect.EffectType.Move:
                int totalMove = effect.value + EquipmentManager.Instance.GetTotalMovementSpeedBonus();
                effectiveTarget.AddMovementPoints(totalMove);
                Debug.Log(
                    $"Added {totalMove} movement points (Base: {effect.value}, Equipment: {EquipmentManager.Instance.GetTotalMovementSpeedBonus()}) to {effectiveTarget.name}.");
                break;

            case CardEffect.EffectType.Heal:
                int totalHeal = effect.value + EquipmentManager.Instance.GetTotalHealBonus();
                effectiveTarget.Heal(totalHeal);
                Debug.Log(
                    $"Healed {effectiveTarget.name} for {totalHeal} (Base: {effect.value}, Equipment: {EquipmentManager.Instance.GetTotalHealBonus()}).");
                break;

            case CardEffect.EffectType.Block:
                int totalBlock = effect.value + EquipmentManager.Instance.GetTotalDefenseBonus();
                effectiveTarget.AddBlock(totalBlock);
                Debug.Log(
                    $"{effectiveTarget.name} gained {totalBlock} Block (Base: {effect.value}, Equipment: {EquipmentManager.Instance.GetTotalDefenseBonus()}).");
                break;
        }
    }

    public bool IsPoisonAttackActive()
    {
        return poisonAttackActive;
    }

    public int GetPendingPoisonDuration()
    {
        return pendingPoisonDuration;
    }

    public void ClearPoisonAttack()
    {
        poisonAttackActive = false;
        pendingPoisonDuration = 0;
        Debug.Log("Poison attack effect cleared");
    }

    private void ApplyStunEffect(int cardValue)
    {
        if (UnitManager.Instance == null || UnitManager.Instance.SelectedUnit == null)
        {
            Debug.LogError("Cannot apply stun effect: UnitManager or SelectedUnit is null");
            return;
        }

        if (AttackManager.Instance == null)
        {
            Debug.LogError("Cannot apply stun effect: AttackManager.Instance is null");
            return;
        }

        int stunRange = 1;

        Vector3Int playerHexCoords;
        if (UnitManager.Instance.SelectedUnit.currentHex != null)
        {
            playerHexCoords = UnitManager.Instance.SelectedUnit.currentHex.hexCoords;
            Debug.Log($"Using Unit.currentHex: {playerHexCoords}");
        }
        else
        {
            playerHexCoords = HexGrid.Instance.GetClosestHex(
                UnitManager.Instance.SelectedUnit.transform.position
            );
            Debug.Log($"Using calculated hex: {playerHexCoords}");
        }

        Debug.Log($"=== STUN EFFECT DEBUG ===");
        Debug.Log($"Player at hex: {playerHexCoords}");
        Debug.Log($"Stun range: {stunRange} (card value: {cardValue})");
        Debug.Log($"==========================");

        List<EnemyUnit> enemiesInRange = AttackManager.Instance.GetEnemiesInRange(playerHexCoords, stunRange);

        int enemiesAffected = 0;
        foreach (EnemyUnit enemy in enemiesInRange)
        {
            enemy.ApplyStun(cardValue);
            enemiesAffected++;
            Debug.Log($"Stun effect applied to {enemy.name} - will skip next turn");
        }

        Debug.Log($"Stun effect completed: {enemiesAffected} enemies stunned in range {stunRange}");
    }


    private void ApplyBurnEffect(int cardValue, int range)
    {
        if (UnitManager.Instance == null || UnitManager.Instance.SelectedUnit == null)
        {
            Debug.LogError("Cannot apply burn effect: UnitManager or SelectedUnit is null");
            return;
        }

        if (AttackManager.Instance == null)
        {
            Debug.LogError("Cannot apply burn effect: AttackManager.Instance is null");
            return;
        }

        const int burnDamage = 2;
        int burnRange = cardValue;

        Vector3Int playerHexCoords;
        if (UnitManager.Instance.SelectedUnit.currentHex != null)
        {
            playerHexCoords = UnitManager.Instance.SelectedUnit.currentHex.hexCoords;
            Debug.Log($"Using Unit.currentHex: {playerHexCoords}");
        }
        else
        {
            playerHexCoords = HexGrid.Instance.GetClosestHex(
                UnitManager.Instance.SelectedUnit.transform.position
            );
            Debug.Log($"Using calculated hex: {playerHexCoords}");
        }

        Debug.Log($"=== BURN EFFECT DEBUG ===");
        Debug.Log($"Player at hex: {playerHexCoords}");
        Debug.Log($"Burn damage: {burnDamage} (always 2)");
        Debug.Log($"Burn range: {burnRange} (card value: {cardValue})");

        HashSet<Vector3Int> hexesInRange = AttackManager.Instance.GetHexesInRange(playerHexCoords, burnRange);
        Debug.Log($"Found {hexesInRange.Count} hexes in burn range using AttackManager logic");

        List<EnemyUnit> enemiesInRange = AttackManager.Instance.GetEnemiesInRange(playerHexCoords, burnRange);
        Debug.Log($"Found {enemiesInRange.Count} enemies in burn range");

        List<Vector3> burnVFXPositions = new List<Vector3>();

        foreach (Vector3Int hexCoord in hexesInRange)
        {
            Hex hex = HexGrid.Instance.GetTileAt(hexCoord);
            if (hex != null)
            {
                Vector3 hexPosition = hex.transform.position;
                burnVFXPositions.Add(hexPosition);
                Debug.Log($"Adding burn VFX at hex {hexCoord} at position {hexPosition}");
            }
            else if (hex != null && hex.IsObstacle())
            {
                Debug.Log($"Skipping VFX at hex {hexCoord} - is obstacle");
            }
        }

        if (VFXManager.Instance != null && burnVFXPositions.Count > 0)
        {
            Debug.Log($"Playing burn VFX at {burnVFXPositions.Count} hex positions");
            foreach (Vector3 pos in burnVFXPositions)
            {
                VFXManager.Instance.PlayVFX(VFXManager.VFXType.Burn, pos);
                Debug.Log($"  -> VFX played at {pos}");
            }
        }
        else if (VFXManager.Instance == null)
        {
            Debug.LogError("VFXManager.Instance is null!");
        }
        else
        {
            Debug.Log("No valid hex positions for VFX");
        }

        int enemiesAffected = 0;
        foreach (EnemyUnit enemy in enemiesInRange)
        {
            if (enemy != null && enemy.gameObject.activeInHierarchy)
            {
                enemy.TakeDamage(burnDamage);
                enemiesAffected++;
                Debug.Log($"Burn effect dealt {burnDamage} damage to {enemy.name}");
            }
        }

        Debug.Log(
            $"Burn effect completed: VFX shown on {burnVFXPositions.Count} hexes, {enemiesAffected} enemies damaged with {burnDamage} damage each");
        Debug.Log($"==========================");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            TestBurnEffect();
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            TestStunEffect();
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            TestPoisonEffect();
        }
    }

    private void TestStunEffect()
    {
        Debug.Log("Testing stun effect with H key press...");

        if (UnitManager.Instance == null)
        {
            Debug.LogError("TEST: UnitManager.Instance is null!");
            return;
        }

        if (UnitManager.Instance.SelectedUnit == null)
        {
            Debug.LogWarning("TEST: UnitManager.Instance.SelectedUnit is null! Trying to find player unit...");

            Unit[] allUnits = FindObjectsOfType<Unit>();
            Unit playerUnit = null;

            foreach (Unit unit in allUnits)
            {
                if (!unit.GetComponent<EnemyUnit>())
                {
                    playerUnit = unit;
                    break;
                }
            }

            if (playerUnit != null)
            {
                Debug.Log($"TEST: Found player unit: {playerUnit.name}. Setting as SelectedUnit.");
                UnitManager.Instance.HandleUnitSelected(playerUnit.gameObject);
            }
            else
            {
                Debug.LogError("TEST: No player unit found in scene!");
                return;
            }
        }

        Debug.Log($"TEST: Using SelectedUnit: {UnitManager.Instance.SelectedUnit.name}");

        CardEffect testStunEffect = new CardEffect
        {
            effectType = CardEffect.EffectType.Stun,
            value = 5,
        };

        ApplyImmediateEffect(testStunEffect, UnitManager.Instance.SelectedUnit);
    }

    private void TestBurnEffect()
    {
        Debug.Log("Testing burn effect with G key press...");

        if (UnitManager.Instance == null)
        {
            Debug.LogError("TEST: UnitManager.Instance is null!");
            return;
        }

        if (UnitManager.Instance.SelectedUnit == null)
        {
            Debug.LogWarning("TEST: UnitManager.Instance.SelectedUnit is null! Trying to find player unit...");

            Unit[] allUnits = FindObjectsOfType<Unit>();
            Unit playerUnit = null;

            foreach (Unit unit in allUnits)
            {
                if (!unit.GetComponent<EnemyUnit>())
                {
                    playerUnit = unit;
                    break;
                }
            }

            if (playerUnit != null)
            {
                Debug.Log($"TEST: Found player unit: {playerUnit.name}. Setting as SelectedUnit.");
                UnitManager.Instance.HandleUnitSelected(playerUnit.gameObject);
            }
            else
            {
                Debug.LogError("TEST: No player unit found in scene!");
                return;
            }
        }

        Debug.Log($"TEST: Using SelectedUnit: {UnitManager.Instance.SelectedUnit.name}");

        CardEffect testBurnEffect = new CardEffect
        {
            effectType = CardEffect.EffectType.Burn,
            value = 2,
            range = 0
        };

        ApplyImmediateEffect(testBurnEffect, UnitManager.Instance.SelectedUnit);
    }

    private void TestPoisonEffect()
    {
        Debug.Log("Testing poison effect with J key press...");

        if (UnitManager.Instance == null)
        {
            Debug.LogError("TEST: UnitManager.Instance is null!");
            return;
        }

        if (UnitManager.Instance.SelectedUnit == null)
        {
            Debug.LogWarning("TEST: UnitManager.Instance.SelectedUnit is null! Trying to find player unit...");

            Unit[] allUnits = FindObjectsOfType<Unit>();
            Unit playerUnit = null;

            foreach (Unit unit in allUnits)
            {
                if (!unit.GetComponent<EnemyUnit>())
                {
                    playerUnit = unit;
                    break;
                }
            }

            if (playerUnit != null)
            {
                Debug.Log($"TEST: Found player unit: {playerUnit.name}. Setting as SelectedUnit.");
                UnitManager.Instance.HandleUnitSelected(playerUnit.gameObject);
            }
            else
            {
                Debug.LogError("TEST: No player unit found in scene!");
                return;
            }
        }

        Debug.Log($"TEST: Using SelectedUnit: {UnitManager.Instance.SelectedUnit.name}");

        CardEffect testPoisonEffect = new CardEffect
        {
            effectType = CardEffect.EffectType.Poison,
            value = 3,
            range = 0
        };

        ApplyImmediateEffect(testPoisonEffect, UnitManager.Instance.SelectedUnit);
        Debug.Log("Poison effect activated! Next attack will poison the target.");
    }

    public void PlayerActionResolved(bool actionWasCompleted)
    {
        isWaitingForPlayerActionResolution = false;

        attackAvailable = false;
        pendingAttackDamage = 0;
        pendingAttackRange = 0;

        if (PlayerStatusUI.Instance != null)
        {
            PlayerStatusUI.Instance.ClearAttackInfo();
        }

        Debug.Log($"Player action resolved. Attack available reset to: {attackAvailable}");
    
        if (AttackManager.Instance != null)
        {
            AttackManager.Instance.TryPrepareAttack();
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

        if (attackAvailable)
        {
            attackAvailable = false;
            pendingAttackDamage = 0;
            pendingAttackRange = 0;
            if (AttackManager.Instance != null)
            {
                AttackManager.Instance.ClearHighlights();
            }
        }

        ClearPoisonAttack();
        ResetAttackAvailability();

        if (isWaitingForPlayerActionResolution)
        {
            Debug.LogWarning("Cannot end turn while waiting for action resolution.");
            if (PlayerStatusUI.Instance != null) PlayerStatusUI.Instance.ClearAttackInfo();
            AttackManager.Instance?.ClearHighlights();
            isWaitingForPlayerActionResolution = false;
            if (PlayedCardEffectCache.Instance != null) PlayedCardEffectCache.Instance.ClearCache();
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

        if (animDead != null)
        {
            animDead.SetBool("Dead", true);
        }
        else
        {
            Debug.LogWarning("animDead is not assigned in the GameManager inspector!");
        }

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

    public void ResetAttackAvailability()
    {
        attackAvailable = false;
        pendingAttackDamage = 0;
        pendingAttackRange = 0;

        if (PlayerStatusUI.Instance != null)
        {
            PlayerStatusUI.Instance.ClearAttackInfo();
        }

        Debug.Log("Attack availability reset");
    }

    public void ReturnToMainMenu()
    {
        Debug.Log($"Returning to Main Menu scene: {mainMenuSceneName}");
        SceneManager.LoadScene(mainMenuSceneName);
    }
}