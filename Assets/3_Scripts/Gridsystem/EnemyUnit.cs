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
        int maxCardsPerTurn = 2;

        while (cardsPlayed < maxCardsPerTurn && hand.Count > 0)
        {
            bool playerInRange = IsPlayerInRange(playerDetectRange);

            CardData cardToPlay = null;

            if (playerInRange)
            {
                cardToPlay = hand.FirstOrDefault(c => c.rightEffects.Any(e => e.effectType == CardEffect.EffectType.Attack));
            }
            else
            {
                cardToPlay = hand.FirstOrDefault(c => c.rightEffects.Any(e => e.effectType == CardEffect.EffectType.Move));
            }
            if (cardToPlay == null)
                cardToPlay = hand.FirstOrDefault(c => c.rightEffects.Any(e => e.effectType == CardEffect.EffectType.Block));

            if (cardToPlay != null)
            {
                PlayCard(cardToPlay, false);
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
        DrawCards(2);

        var hand = this.hand.ToList(); 
        int played = 0;
        foreach (var card in hand)
        {
            if (played >= 2) break;
            yield return new WaitForSeconds(0.5f); 
            PlayCard(card, false);
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
                    //MoveTowardsPlayer(effect.value);
                    Debug.Log($"{name} bewegt sich {effect.value} Felder Richtung Spieler.");
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

    /*private void MoveTowardsPlayer(int steps)
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        Vector3Int playerHex = HexGrid.Instance.GetClosestHex(player.transform.position);
        Vector3Int myHex = HexGrid.Instance.GetClosestHex(transform.position);

        for (int i = 0; i < steps; i++)
        {
            Vector3Int nextHex = GetNextHexTowards(myHex, playerHex);
            Hex nextTile = HexGrid.Instance.GetTileAt(nextHex);
            if (nextTile != null && !nextTile.HasEnemyUnit())
            {
                transform.position = HexGrid.Instance.GetWorldPosition(nextHex);
                myHex = nextHex;
                currentHex?.SetEnemyUnit(null);
                currentHex = nextTile;
                currentHex.SetEnemyUnit(this);
            }
            else
            {
                break;
            }
        }
    }

    private Vector3Int GetNextHexTowards(Vector3Int from, Vector3Int to)
    {
        Vector3Int direction = new Vector3Int(
            to.x > from.x ? 1 : (to.x < from.x ? -1 : 0),
            to.y > from.y ? 1 : (to.y < from.y ? -1 : 0),
            to.z > from.z ? 1 : (to.z < from.z ? -1 : 0)
        );

        Vector3Int next = from + direction;
        return next;
    }*/
}