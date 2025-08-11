using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ShieldSystem : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int maxShields = 11;

    [SerializeField] private GameObject shieldVFXPrefab;
    [SerializeField] private Vector3 shieldVFXOffset = new Vector3(0, -1, 0); // Offset for VFX position
    [SerializeField] private float vfxDuration;

    [Header("Assigned UI Images")]
    [SerializeField] private List<Image> shieldIcons = new List<Image>();

    private int currentShields;
    private bool[] shieldsUnlocked;
    
    public static ShieldSystem Instance { get; private set; }


    private void Start()
    {
        shieldsUnlocked = new bool[maxShields];
        InitializeShields(0); 
    }

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
    public void InitializeShields(int startingShields)
    {
        currentShields = startingShields;

        for (int i = 0; i < startingShields; i++)
        {
            if (i < shieldsUnlocked.Length)
            {
                shieldsUnlocked[i] = true;
            }
        }

        UpdateShieldDisplay();
    }

    public void AddShields(int amount)
    {

        int newShields = Mathf.Min(currentShields + amount, maxShields);
        for (int i = currentShields; i < newShields; i++)
        {
            if (i < shieldsUnlocked.Length)
            {
                shieldsUnlocked[i] = true;
            }
        }

        currentShields = newShields;
        UpdateShieldDisplay();
    }

    public void ShieldVFX(Vector3 position)
    {
        if (shieldVFXPrefab != null)
        {
            GameObject vfx = Instantiate(shieldVFXPrefab, position + shieldVFXOffset, Quaternion.identity);
            Destroy(vfx, vfxDuration);
        }
        else
        {
            Debug.LogWarning("Shield VFX prefab is not assigned!");
        }
    }

    public void LoseShields(int amount)
    {
        currentShields = Mathf.Max(currentShields - amount, 0);
        UpdateShieldDisplay();
    }


    private void UpdateShieldDisplay()
    {
        for (int i = 0; i < shieldIcons.Count; i++)
        {
            bool shouldShow = i < shieldsUnlocked.Length && shieldsUnlocked[i];
            shieldIcons[i].gameObject.SetActive(shouldShow && i < currentShields);
        }
    }

    public int GetCurrentShields()
    {
        return currentShields;
    }

    public void SetShields(int amount)
    {
        currentShields = amount;
    }
}
