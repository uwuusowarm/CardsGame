using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance { get; private set; }
    
    [SerializeField] private PlayerDataSO playerData;
    
    public bool IsDataLoaded { get; private set; } = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        IsDataLoaded = false;
        StartCoroutine(LoadDataAfterSceneLoad());
    }

    private IEnumerator LoadDataAfterSceneLoad()
    {
        yield return new WaitUntil(() => HealthSystem.Instance != null &&
                                         ExhaustionSystem.Instance != null &&
                                         EquipmentManager.Instance != null);

        EquipmentManager.Instance.UnequipAll();
        foreach (var entry in playerData.savedEquippedItems)
        {
            if (entry.itemData != null)
            {
                EquipmentManager.Instance.EquipItem(entry.itemData);
            }
        }

        HealthSystem.Instance.UnlockExtraHealth(playerData.savedUnlockedExtraHealthSlots);
        HealthSystem.Instance.InitializeHealth(playerData.savedCurrentHealth);
        
        ExhaustionSystem.Instance.SetExhaustionStacks(playerData.savedExhaustionStacks);
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            Unit playerUnit = playerObj.GetComponent<Unit>();
            if (playerUnit != null)
            {
                playerUnit.currentHealth = HealthSystem.Instance.GetCurrentHealth();
                playerUnit.maxHealth = HealthSystem.Instance.GetMaxHealth();
            }
        }
        
        Debug.Log("Player data loaded from PlayerDataSO.");
        IsDataLoaded = true;
    }

    public void SavePlayerState()
    {
        if (HealthSystem.Instance != null)
        {
            playerData.savedCurrentHealth = HealthSystem.Instance.GetCurrentHealth();
            playerData.savedUnlockedExtraHealthSlots = HealthSystem.Instance.GetUnlockedExtraHealthCount();
        }

        if (ExhaustionSystem.Instance != null)
        {
            playerData.savedExhaustionStacks = ExhaustionSystem.Instance.GetExhaustionStacks();
        }

        if (EquipmentManager.Instance != null)
        {
            playerData.savedEquippedItems.Clear();
            foreach (ItemSlot slot in System.Enum.GetValues(typeof(ItemSlot)))
            {
                ItemData item = EquipmentManager.Instance.GetEquippedItem(slot);
                if (item != null)
                {
                    playerData.savedEquippedItems.Add(new PlayerDataSO.EquippedItemEntry { slot = slot, itemData = item });
                }
            }
        }
        
        Debug.Log("Player data saved to PlayerDataSO.");
    }
}