using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class Unit : MonoBehaviour
{
    [SerializeField] public int maxHealth = 5;
    [SerializeField] public int currentHealth;
    [SerializeField] public int movementPoints = 0;
    [SerializeField] public int actionPoints = 4;
    public int shieldPoints = 0;
    protected Hex currentHex;
    public bool IsEnemy = false;
    public int MovementPoints { get => movementPoints; }
    
    public static Unit Instance { get; private set; }

    [SerializeField] private float movementDuration = 1, rotationDuration = 0.3f;
    private GlowHighlight glowHighlight;
    private Queue<Vector3> pathPositions = new Queue<Vector3>();
    public event Action<Unit> MovementFinished;

    private void Awake()
    {
        glowHighlight = GetComponent<GlowHighlight>();
        if (glowHighlight == null)
        {
            Debug.LogError("GlowHighlight component missing on Unit!", gameObject);
        }
        
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
        if (glowHighlight == null)
        {
            glowHighlight = GetComponent<GlowHighlight>();
        }
        StartCoroutine(InitializeHexPosition());
    }

    public void AddBlock(int amount)
    {
        ShieldSystem.Instance.AddShields(amount);
    }

    public void Heal(int amount)
    {
        HealthSystem.Instance.Heal(amount);
    }
    
    public void PlayerTakeDamage(int damage)
    {
        int remainingDamage = damage;

        if (ShieldSystem.Instance.GetCurrentShields() > 0)
        {
            int shieldDamage = Mathf.Min(remainingDamage, ShieldSystem.Instance.GetCurrentShields());
            ShieldSystem.Instance.LoseShields(shieldDamage);
            remainingDamage -= shieldDamage;

            Debug.Log($"Shields took {shieldDamage} damage. Remaining shields: {ShieldSystem.Instance.GetCurrentShields()}");
        }

        if (remainingDamage > 0)
        {
            HealthSystem.Instance.LoseHealth(remainingDamage);
            Debug.Log($"Health took {remainingDamage} damage. Remaining health: {HealthSystem.Instance.GetCurrentHealth()}");
        }    }
    
    public void AddMovementPoints(int points)
    {
        movementPoints += (points*10);
    }
    
    public void ResetMovementPoints()
    {
        movementPoints = 0;
    }

    protected IEnumerator InitializeHexPosition()
    {
        yield return new WaitUntil(() => HexGrid.Instance != null);

        HexGrid hexGrid = HexGrid.Instance;
        Vector3Int hexCoords = hexGrid.GetClosestHex(transform.position);
        currentHex = hexGrid.GetTileAt(hexCoords);

        if (currentHex != null)
        {
            if (currentHex.IsOccupied())
            {
                Debug.LogError($"Hex at {hexCoords} is already occupied!");
            }
            else
            {
                currentHex.SetUnit(this);
                Debug.Log($"Unit initialized at {hexCoords}");
            }
        }
        else
        {
            Debug.LogError($"Failed to initialize unit at {hexCoords}");
        }
    }
    internal void Deselect()
    {
        glowHighlight.ToggleGlow(false);
    }
    public void Select()
    {
        glowHighlight.ToggleGlow(true);
    }

    internal void MoveTroughPath(List<Vector3> currentPath)
    {
        if (currentPath.Count > movementPoints)
        {
            Debug.LogWarning("Not enough movement points!");
            return;
        }
        pathPositions = new Queue<Vector3>(currentPath);
        Vector3 firstTarget = pathPositions.Dequeue();
        StartCoroutine(RotationCoroutine(firstTarget, rotationDuration, true));
    }
    private IEnumerator RotationCoroutine(Vector3 endPosition, float rotationDuration, bool firstRotation = false)
    {
        Quaternion startRotation = transform.rotation;
        endPosition.y = transform.position.y;
        Vector3 direction = endPosition - transform.position;
        Quaternion endRotation = Quaternion.LookRotation(direction, Vector3.up);

        if (Mathf.Approximately(Mathf.Abs(Quaternion.Dot(startRotation, endRotation)), 1.0f) == false)
        {
            float timeElapsed = 0;
            while (timeElapsed < rotationDuration)
            {
                timeElapsed += Time.deltaTime;
                float lerpStep = timeElapsed / rotationDuration;
                transform.rotation = Quaternion.Lerp(startRotation, endRotation, lerpStep);
                yield return null;
            }
            transform.rotation = endRotation;
        }
        StartCoroutine(MovementCoroutine(endPosition));
    }

    private IEnumerator MovementCoroutine(Vector3 endPosition)
    {
        HexGrid hexGrid = FindObjectOfType<HexGrid>();

        if (currentHex != null)
        {
            currentHex.ClearUnit();
        }

        Vector3 startPosition = transform.position;
        endPosition.y = startPosition.y;
        float timeElapsed = 0;

        while (timeElapsed < movementDuration)
        {
            timeElapsed += Time.deltaTime;
            float lerpStep = timeElapsed / movementDuration;
            transform.position = Vector3.Lerp(startPosition, endPosition, lerpStep);
            yield return null;
        }
        transform.position = endPosition;

        Vector3Int newHexCoords = hexGrid.GetClosestHex(endPosition);
        currentHex = hexGrid.GetTileAt(newHexCoords);
        if (currentHex != null)
        {
            currentHex.SetUnit(this);
        }
        else
        {
            Debug.Log("Error");
        }

        if (pathPositions.Count > 0)
        {
            Debug.Log("Seleceting the next position!");
            StartCoroutine(RotationCoroutine(pathPositions.Dequeue(), rotationDuration));
        }
        else
        {
            Debug.Log("Movement finished!");
            movementPoints = 0;
                
            MovementFinished?.Invoke(this);
        }
    }
}