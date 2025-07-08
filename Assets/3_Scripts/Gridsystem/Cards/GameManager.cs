using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    [SerializeField] private float enemyTurnDelay = 1f;
    private PlayerInput playerInput;

    private bool isFirstTurn = true;
    public bool IsPlayerTurn { get; private set; } = false;
    private bool isWaitingForPlayerActionResolution = false;

    private Unit playerUnit;

    public Unit PlayerUnit => playerUnit;
    
    int carryOverActionPoints = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(this);
    }

    private void Start()
    {
        playerInput = FindObjectOfType<PlayerInput>();
        if (playerInput != null)
        {
            playerInput.PointerRightClick.AddListener(HandleCancelAction);
        }
        else
        {
            Debug.LogError("PlayerInput script not found in the scene!");
        }

        GameObject playerGameObject = GameObject.FindGameObjectWithTag("Player");
        if (playerGameObject != null)
        {
            playerUnit = playerGameObject.GetComponent<Unit>();
            if (playerUnit != null)
            {
                Debug.Log("Player Unit found and assigned: " + playerUnit.name);
                playerUnit.MovementFinished += OnPlayerMovementFinished;
            }
            else
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

    private void OnDestroy()
    {
        if (playerInput != null)
        {
            playerInput.PointerRightClick.RemoveListener(HandleCancelAction);
        }
        if (playerUnit != null)
        {
            playerUnit.MovementFinished -= OnPlayerMovementFinished;
        }
    }
    
    private void OnPlayerMovementFinished(Unit unit)
    {
        Debug.Log("Player has finished moving. Re-evaluating actions.");
        UpdatePlayerStateFromCache();
    }

    private IEnumerator WaitForManagersAndStartGame()
    {
        yield return new WaitUntil(() => CardManager.Instance != null &&
                                         UnitManager.Instance != null &&
                                         HexGrid.Instance != null &&
                                         AttackManager.Instance != null &&
                                         PlayedCardEffectCache.Instance != null &&
                                         ActionPointSystem.Instance != null &&
                                         playerUnit != null &&
                                         PlayerStatsUI.Instance != null);
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
        
        playerUnit.ResetMovementPoints(); 
        
        PlayerStatsUI.Instance.UpdateMovementPoints(0); 
        PlayerStatsUI.Instance.ClearAttackInfo();

        if (ActionPointSystem.Instance != null)
        {
            if (isFirstTurn)
            {
                ActionPointSystem.Instance.InitializeActionPoints(2);
            }
            else
            {
                ActionPointSystem.Instance.ResetActionPoints();
                ActionPointSystem.Instance.AddActionPoints(2);
            }

            if (carryOverActionPoints > 0)
            {
                ActionPointSystem.Instance.AddActionPoints(1);
                carryOverActionPoints = 0;
            }
        }
    
        if (isFirstTurn)
        {
            Debug.Log("First turn of the game.");
            isFirstTurn = false;
            CardManager.Instance.DrawInitialCards();
        }
<<<<<<< HEAD
=======

    }

    private void DrawPlayerCards()
    {
        
        if (CardManager.Instance == null)
        {
            Debug.LogError("CardManager.Instance is null for card draw.");
            return;
        }
        Debug.Log("Drawing new cards based on exhaustion level.");
        CardManager.Instance.DrawCards(CardManager.Instance.drawCount);
        Sound_Manager.instance.Play("Draw_Card_V2");
>>>>>>> 857421fa43217ce6648a67edc30cb1765b78c3e3
    }

    public void ProcessPlayedCard(CardData cardData, bool isLeftEffectChosen)
    {
        if (ActionPointSystem.Instance.GetCurrentActionPoints() <= 0) return;
        if (!IsPlayerTurn || isWaitingForPlayerActionResolution) return;

        UnitManager.Instance.HandleUnitSelected(playerUnit.gameObject);

        PlayedCardEffectCache.Instance.CacheCardEffect(cardData, isLeftEffectChosen);

        ApplyAndConsumeNonTargetedEffects();

        UpdatePlayerStateFromCache();

        CardManager.Instance.MoveToZone(cardData, DropType.Discard);
        ActionPointSystem.Instance.UseActionPoints(1);
        UnitManager.Instance.ReduceActionPoints(playerUnit, 1);
    }
    
    private void ApplyAndConsumeNonTargetedEffects()
    {
        var cache = PlayedCardEffectCache.Instance;
        Unit target = playerUnit;

        if (cache.PendingBlock > 0)
        {
            target.AddBlock(cache.PendingBlock);
            Debug.Log($"Player gained {cache.PendingBlock} Block");
            cache.ConsumeBlock();
        }
        if (cache.PendingHealing > 0)
        {
            target.Heal(cache.PendingHealing);
            Debug.Log($"Player healed for {cache.PendingHealing}");
            cache.ConsumeHealing();
        }
    }
    
    public void UpdatePlayerStateFromCache()
    {
        var cache = PlayedCardEffectCache.Instance;

        if (cache.HasPendingMovement)
        {
            Debug.Log("Updating movement range from cache.");
            UnitManager.Instance.PrepareUnitForMovement(playerUnit);
        }
        else
        {
            FindObjectOfType<MovementSystem>().HideRange();
        }
        PlayerStatsUI.Instance.UpdateMovementPoints(cache.PendingMovement);

        if (cache.HasPendingAttack)
        {
            Debug.Log("Updating attack range from cache.");
            isWaitingForPlayerActionResolution = true;
            int totalDamage = cache.PendingDamage;
            int totalRange = cache.PendingRange;
            AttackManager.Instance.PrepareAttack(totalDamage, totalRange);
            PlayerStatsUI.Instance.UpdateAttackInfo(totalDamage, totalRange);
        }
        else
        {
            AttackManager.Instance.ClearHighlights();
            PlayerStatsUI.Instance.ClearAttackInfo();
            isWaitingForPlayerActionResolution = false;
        }
    }

    public void PlayerActionResolved(bool actionWasCompleted)
    {
        Debug.Log($"Player attack action resolved. Completed: {actionWasCompleted}");
    
        if (actionWasCompleted)
        {
            PlayedCardEffectCache.Instance.ConsumeAttack();
        }
        
        UpdatePlayerStateFromCache();
    }
    
    public void HandleCancelAction()
    {
        if (!IsPlayerTurn || !isWaitingForPlayerActionResolution) return;

        Debug.Log("Player cancelled target selection. Hiding highlights, attack remains armed.");
        AttackManager.Instance.ClearHighlights();
    }

    public void PlayerEndsTurn()
    {
        if (!IsPlayerTurn) return;
        
        PlayedCardEffectCache.Instance.ClearCacheForNewTurn();
        FindObjectOfType<MovementSystem>().HideRange();
        AttackManager.Instance.ClearHighlights();
        
        int currentAP = ActionPointSystem.Instance.GetCurrentActionPoints();
        carryOverActionPoints = Mathf.Min(currentAP, 1);

        Debug.Log("Player initiated end of turn.");
        IsPlayerTurn = false;
        StartCoroutine(EnemyTurnRoutine());
    }

    private IEnumerator EnemyTurnRoutine()
    {
        Debug.Log("Starting Enemy Turn Routine.");

        foreach (var enemy in UnitManager.Instance.GetEnemyUnits())
        {
            if (enemy != null)
            {
                yield return StartCoroutine(enemy.EnemyTurnRoutine());
                yield return new WaitForSeconds(enemyTurnDelay);
            }
        }
        ShieldSystem.Instance.LoseShields(100);
        StartPlayerTurn();
    }
}
