using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyUnit : MonoBehaviour
{
    [Header("Health Settings")]
    public int damage = 3;
    public int maxHealth = 3;
    public int currentHealth { get; private set; }

    [Header("Visuals")]
    public Material normalMaterial;
    public Material highlightMaterial;
    private Renderer enemyRenderer;
    private bool isHighlighted = false;
    public Hex currentHex { get; private set; }

    [Header("Enemy Deck & Hand")]
    public List<CardData> deck = new List<CardData>();
    public List<CardData> hand = new List<CardData>();
    public int handSize = 3;
    public int playerDetectRange = 1; 

    private void Awake()
    {
        enemyRenderer = GetComponentInChildren<Renderer>();
        if (enemyRenderer == null)
        {
            Debug.LogError("Renderer not found on EnemyUnit!", this);
        }
        if (UnitManager.Instance != null)
        {
            UnitManager.Instance.RegisterEnemy(this);
            Debug.Log($"{name} bei UnitManager registriert!");
        }
        else
        {
            Debug.LogWarning("UnitManager.Instance ist noch nicht gesetzt beim EnemyUnit Awake.");
        }
        EnemyActivator.Instance?.RegisterEnemy(this);
    }

    private void Start()
    {
        currentHealth = maxHealth;
        ResetMaterial();
        StartCoroutine(VerifyHexPosition());
    }

    public void ToggleHighlight(bool highlight)
    {
        if (enemyRenderer == null) return;

        isHighlighted = highlight;
        enemyRenderer.material = highlight ? highlightMaterial : normalMaterial;
    }

    private void ResetMaterial()
    {
        if (enemyRenderer != null)
        {
            enemyRenderer.material = normalMaterial;
        }
    }

    private void OnMouseDown()
    {
        Debug.Log($"=== ENEMY CLICK DEBUG ===");
        Debug.Log($"Enemy clicked: {name}");
        Debug.Log($"Is highlighted: {isHighlighted}");
        Debug.Log($"UnitManager exists: {UnitManager.Instance != null}");
        Debug.Log($"PlayersTurn: {UnitManager.Instance?.PlayersTurn}");
        Debug.Log($"AttackManager exists: {AttackManager.Instance != null}");
        Debug.Log($"========================");
    
        if (UnitManager.Instance.PlayersTurn && isHighlighted)
        {
            Debug.Log("🎯 Calling AttackManager.HandleEnemyClick!");
            AttackManager.Instance?.HandleEnemyClick(this);
        }
        else
        {
            Debug.LogWarning("Click ignored - wrong turn or not highlighted");
        }
    }
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log($"Enemy hit! Remaining HP: {currentHealth}");

        if (currentHealth <= 0)
        {
           Destroy(gameObject);
        }
    }

    private IEnumerator VerifyHexPosition()
    {
        yield return new WaitForSeconds(1);
        Vector3Int hexCoords = HexGrid.Instance.GetClosestHex(transform.position);
        currentHex = HexGrid.Instance.GetTileAt(hexCoords);

        if (currentHex == null)
            Debug.LogError($"Enemy '{name}' not on grid! ");
        else
            Debug.Log($"Enemy registered at {hexCoords}");
            currentHex.SetEnemyUnit(this);
    }

    private void AttackPlayer(int damage)
    {
        int shields = ShieldSystem.Instance.GetCurrentShields();
        int restDamage = damage;

        if (shields > 0)
        {
            int shieldDamage = Mathf.Min(restDamage, shields);
            ShieldSystem.Instance.LoseShields(shieldDamage);
            restDamage -= shieldDamage;
            Debug.Log($"{name} zerstört {shieldDamage} Schilde des Spielers.");
        }

        if (restDamage > 0)
        {
            HealthSystem.Instance.LoseHealth(restDamage);
            Debug.Log($"{name} macht {restDamage} Schaden an den Herzen des Spielers.");
        }
    }

    public void DrawCards(int count)
         {
             for (int i = 0; i < count; i++)
             {
                 if (deck.Count == 0) break;
                 var card = deck[0];
                 deck.RemoveAt(0);
                 hand.Add(card);
             }
         }

    public void EnemyTurn()
    {
        DrawCards(handSize - hand.Count);

        int cardsPlayed = 0;
        int maxCardsPerTurn = 1;

        while (cardsPlayed < maxCardsPerTurn && hand.Count > 0)
        {
            bool playerInRange = IsPlayerInRange(2);

            CardData cardToPlay = null;
            bool playLeft = false;

            if (playerInRange)
            {
                cardToPlay = hand.FirstOrDefault(c =>
                    c.rightEffects.Any(e => e.effectType == CardEffect.EffectType.Attack) ||
                    c.leftEffects.Any(e => e.effectType == CardEffect.EffectType.Attack));
                if (cardToPlay != null)
                    playLeft = cardToPlay.leftEffects.Any(e => e.effectType == CardEffect.EffectType.Attack);
            }
            else
            {
                cardToPlay = hand.FirstOrDefault(c =>
                    c.rightEffects.Any(e => e.effectType == CardEffect.EffectType.Move) ||
                    c.leftEffects.Any(e => e.effectType == CardEffect.EffectType.Move));
                if (cardToPlay != null)
                    playLeft = cardToPlay.leftEffects.Any(e => e.effectType == CardEffect.EffectType.Move);
            }

            if (cardToPlay == null)
                cardToPlay = hand.FirstOrDefault(c =>
                    c.rightEffects.Any(e => e.effectType == CardEffect.EffectType.Block) ||
                    c.leftEffects.Any(e => e.effectType == CardEffect.EffectType.Block));

            if (cardToPlay != null)
            {
                PlayCard(cardToPlay, playLeft);
                cardsPlayed++;
            }
            else
            {
                break;
            }
        }
    }

    public IEnumerator EnemyTurnRoutine()
    {
        if (hand.Count == 0)
            DrawCards(2);

        int played = 0;
        var handCopy = hand.ToList();

        foreach (var card in handCopy)
        {
            if (played >= 1) break;

            bool playerInRange = IsPlayerInRange(2);
            bool playLeft = false;

            if (playerInRange)
            {
                if (card.rightEffects.Any(e => e.effectType == CardEffect.EffectType.Attack))
                    playLeft = false;
                else if (card.leftEffects.Any(e => e.effectType == CardEffect.EffectType.Attack))
                    playLeft = true;
                else
                    continue; 
            }
            else
            {
                if (card.rightEffects.Any(e => e.effectType == CardEffect.EffectType.Move))
                    playLeft = false;
                else if (card.leftEffects.Any(e => e.effectType == CardEffect.EffectType.Move))
                    playLeft = true;
                else
                    continue; 
            }

            yield return new WaitForSeconds(0.5f);
            PlayCard(card, playLeft);
            played++;
        }

        yield return new WaitForSeconds(0.5f);
    }

    private bool IsPlayerInRange(int range)
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return false;
        Vector3Int playerHex = HexGrid.Instance.GetClosestHex(player.transform.position);
        Vector3Int myHex = HexGrid.Instance.GetClosestHex(transform.position);
        int dist = HexDistance(playerHex, myHex);
        return dist <= range;
    }

    private int HexDistance(Vector3Int a, Vector3Int b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dz = Mathf.Abs(a.z - b.z);
    
        if ((a.z % 2 == 1 && b.z % 2 == 0) || (a.z % 2 == 0 && b.z % 2 == 1))
        {
            if (a.x < b.x)
                dx = Mathf.Max(0, dx - 1);
        }
    
        return dx + Mathf.Max(0, dz - (dx + 1) / 2);
    }


    private void PlayCard(CardData card, bool isLeft)
    {
        var effects = new List<CardEffect>();
        if (card.leftEffects != null) effects.AddRange(card.leftEffects);
        // if (card.bottomEffects != null) effects.AddRange(card.bottomEffects);
        if (card.rightEffects != null) effects.AddRange(card.rightEffects);

        foreach (var effect in effects)
        {
            switch (effect.effectType)
            {
                case CardEffect.EffectType.Move:
                    bool moved = MoveTowardsPlayer(effect.value);
                    if (moved)
                        Debug.Log($"{name} move by {effect.value} to player.");
                    else
                        Debug.Log($"{name} cant move.");
                    break;
                case CardEffect.EffectType.Attack:
                    if (IsPlayerInRange(2))
                    {
                        AttackPlayer(effect.value);
                        Debug.Log($"{name} attack player by {effect.value} damage.");
                    }
                    else
                    {
                        Debug.Log($"{name} cant attack because of range.");
                    }
                    break;
                case CardEffect.EffectType.Block:
                    break;
            }
        }
        Debug.Log($"{name} behält die Karte '{card.cardName}' in der Hand");
    }
    
    private bool MoveTowardsPlayer(int steps)
{
    var playerGO = GameObject.FindGameObjectWithTag("Player");
    if (playerGO == null) {
        Debug.LogError("AI Error: Player object with tag 'Player' not found.");
        return false;
    }
    var playerUnit = playerGO.GetComponent<Unit>();
    if (playerUnit == null) {
        Debug.LogError("AI Error: Player object does not have a Unit component.");
        return false;
    }

    Hex playerHexObject = playerUnit.GetCurrentHex();
    if (playerHexObject == null)
    {
        Debug.LogWarning("Player is not on a valid hex (playerUnit.GetCurrentHex() is null). Cannot move towards them.");
        return false;
    }

    if (this.currentHex == null)
    {
        Debug.LogWarning($"{name} is not on a valid hex (this.currentHex is null). Cannot calculate movement path.");
        return false;
    }
    
    Vector3Int myHexCoords = this.currentHex.hexCoords;

    var bfsResult = GraphSearch.BFSGetRange(HexGrid.Instance, myHexCoords, steps);
    var availablePositions = bfsResult.GetRangePositions().ToList();

    if (availablePositions.Count <= 1)
    {
        Debug.Log($"{name} has no available hexes to move to.");
        return false;
    }

    Vector3Int bestPosition = myHexCoords;
    int shortestDistance = HexGrid.Instance.GetDistance(this.currentHex, playerHexObject);

    foreach (var pos in availablePositions)
    {
        if (pos == myHexCoords) continue;

        var hex = HexGrid.Instance.GetTileAt(pos);
        if (hex != null && !hex.IsObstacle() && (!hex.HasEnemyUnit() || hex.EnemyUnitOnHex == this))
        {
            int distance = HexGrid.Instance.GetDistance(hex, playerHexObject);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                bestPosition = pos;
            }
        }
    }

    if (bestPosition == myHexCoords)
    {
        Debug.Log($"{name} is already at the best possible position or cannot get closer.");
        return false;
    }

    var targetHex = HexGrid.Instance.GetTileAt(bestPosition);
    
    currentHex?.ClearEnemyUnit();
    StartCoroutine(MoveToHexSmooth(targetHex));
    currentHex = targetHex;
    currentHex.SetEnemyUnit(this);

    Debug.Log($"{name} moves towards player to position {bestPosition}");
    return true;
}

    private IEnumerator MoveToHexSmooth(Hex targetHex, float duration = 0.3f)
    {
        var propsTransform = targetHex.transform.Find("Props");
        if (propsTransform == null)
        {
            Debug.LogError($"Props child not found on hex at position {targetHex.hexCoords}");
            yield break;
        }

        Vector3 start = transform.position;
        Vector3 end = propsTransform.TransformPoint(new Vector3(0.03f, 0.4f, 0.54f));
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(start, end, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = end;
        transform.SetParent(propsTransform);
        transform.localPosition = new Vector3(0.03f, 0.4f, 0.54f);
    }

    private List<Vector3Int> GetPathToTarget(BFSResult bfsResult, Vector3Int start, Vector3Int target)
    {
        var path = new List<Vector3Int>();
        var current = target;
        while (current != start && bfsResult.visitedNodesDict.ContainsKey(current))
        {
            path.Insert(0, current);
            current = bfsResult.visitedNodesDict[current] ?? start;
        }
        path.Insert(0, start);
        return path;
    }
}