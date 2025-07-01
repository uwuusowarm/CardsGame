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
        return (Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) + Mathf.Abs(a.z - b.z)) / 2;
    }

    private void PlayCard(CardData card, bool isLeft)
    {
        foreach (var effect in isLeft ? card.leftEffects : card.rightEffects)
        {
            switch (effect.effectType)
            {
                case CardEffect.EffectType.Move:
                    bool moved = MoveTowardsPlayer(effect.value * 10);
                    if (moved)
                        Debug.Log($"{name} bewegt sich {effect.value} Felder Richtung Spieler.");
                    else
                        Debug.Log($"{name} konnte sich nicht bewegen.");
                    break;
                case CardEffect.EffectType.Attack:
                    AttackPlayer(effect.value);
                    Debug.Log($"{name} greift Spieler an für {effect.value} Schaden.");
                    break;
                case CardEffect.EffectType.Block:
                    break;
            }
        }
        hand.Remove(card);
    }

    private bool MoveTowardsPlayer(int steps)
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return false;

        Vector3Int playerHex = HexGrid.Instance.GetClosestHex(player.transform.position);
        Vector3Int myHex = HexGrid.Instance.GetClosestHex(transform.position);

        var bfsResult = GraphSearch.BFSGetRange(HexGrid.Instance, myHex, steps);
        List<Vector3Int> path = GetPathToTarget(bfsResult, myHex, playerHex);

        Debug.Log($"{name} Path length: {path.Count} | Start: {myHex} | Ziel: {playerHex}");
        Debug.Log($"{name} BFS-Range Felder: {string.Join(", ", bfsResult.GetRangePositions())}");

        if (path.Count <= 1)
        {
            Debug.Log($"{name} kann keinen Pfad zum Spieler finden oder steht schon am Ziel.");
            return false;
        }

        int moveSteps = Mathf.Min(steps, path.Count - 1); 
        for (int i = 1; i <= moveSteps; i++)
        {
            Vector3Int nextHex = path[i];
            Hex nextTile = HexGrid.Instance.GetTileAt(nextHex);
            if (nextTile != null && !nextTile.HasEnemyUnit())
            {
                transform.position = HexGrid.Instance.GetWorldPosition(nextHex);
                currentHex?.SetEnemyUnit(null);
                currentHex = nextTile;
                currentHex.SetEnemyUnit(this);
            }
            else
            {
                Debug.Log($"{name} Bewegung blockiert bei {nextHex}.");
                break; 
            }
        }
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