using System.Collections;
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

    private void Awake()
    {
        enemyRenderer = GetComponentInChildren<Renderer>();
        if (enemyRenderer == null)
        {
            Debug.LogError("Renderer not found on EnemyUnit!", this);
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
    }

    public void AttackPlayer()
    {

    }
}