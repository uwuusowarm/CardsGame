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
        if (UnitManager.Instance.PlayersTurn && isHighlighted)
        {
            AttackManager.Instance?.HandleEnemyClick(this);
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
            Debug.LogError("Enemy not on grid!");
        else
            Debug.Log($"Enemy registered at {hexCoords}");
            currentHex.SetEnemyUnit(this);
    }

    public void AttackPlayer()
    {

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
                    bool moved = MoveTowardsPlayer(effect.value * 10);
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
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return false;

        Vector3Int playerHex = HexGrid.Instance.GetClosestHex(player.transform.position);
        Vector3Int myHex = HexGrid.Instance.GetClosestHex(transform.position);

        var bfsResult = GraphSearch.BFSGetRange(HexGrid.Instance, myHex, steps);
        var availablePositions = bfsResult.visitedNodesDict.Keys.ToList();
        
        Debug.Log($"{name} BFS-Range Felder: {string.Join(", ", availablePositions)}");

        if (availablePositions.Count == 0)
        {
            Debug.Log($"{name} kann sich nicht bewegen.");
            return false;
        }

        Vector3Int bestPosition = myHex;
        int shortestDistance = int.MaxValue;

        foreach (var pos in availablePositions)
        {
            var hex = HexGrid.Instance.GetTileAt(pos);
            if (hex != null && !hex.HasEnemyUnit() && !hex.IsObstacle())
            {
                int distance = Mathf.Abs(pos.x - playerHex.x) + Mathf.Abs(pos.z - playerHex.z);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    bestPosition = pos;
                }
            }
        }

        if (bestPosition == myHex)
        {
            Debug.Log($"{name} cant find a better position.");
            return false;
        }

        var targetHex = HexGrid.Instance.GetTileAt(bestPosition);
        var propsTransform = targetHex.transform.Find("Props");
        
        if (propsTransform == null)
        {
            Debug.LogError($"Props child not found on hex at position {bestPosition}");
            return false;
        }

        transform.SetParent(propsTransform); 
        transform.localPosition = new Vector3(0.03f, 0.4f, 0.54f); 

        currentHex?.SetEnemyUnit(null);
        currentHex = targetHex;
        currentHex.SetEnemyUnit(this);
        
        Debug.Log($"{name} moves to position {bestPosition}");
        return true;
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