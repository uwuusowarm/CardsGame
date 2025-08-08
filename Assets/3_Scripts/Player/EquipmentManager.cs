using System; 
using System.Collections; 
using System.Collections.Generic;
using System.Text; 
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance { get; private set; }

    public event Action<ItemSlot> OnEquipmentChanged;

    [Header("UI Feedback")]
    [SerializeField] private TextMeshProUGUI itemStatsDisplay;
    [SerializeField] private float statsDisplayDuration = 4.0f;
    
    [Header("3D Equipment UI Buttons")]
    [SerializeField] private Button[] equipmentButtons = new Button[4]; 
    [SerializeField] private ItemSlot[] equipmentSlots = new ItemSlot[4]; 


    private Dictionary<ItemSlot, ItemData> equippedItems = new Dictionary<ItemSlot, ItemData>();
    public ItemClassType playerClass = ItemClassType.Warrior; 

    private Coroutine displayCoroutine;
    
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
            Destroy(this);
            return;
        }

        InitializeEquipmentSlots();
        FindItemTooltipText();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindItemTooltipText();
        SetupEquipmentButtons();
    }

    private void FindItemTooltipText()
    {
        var tooltips = FindObjectsOfType<TextMeshProUGUI>(true);
        foreach (var tmp in tooltips)
        {
            if (tmp.gameObject.name.Contains("ItemTooltipText"))
            {
                itemStatsDisplay = tmp;
                Debug.Log($"Found ItemTooltipText: {tmp.gameObject.name} in {tmp.gameObject.scene.name}");
                
                if (itemStatsDisplay != null)
                {
                    itemStatsDisplay.gameObject.SetActive(false);
                }
                return;
            }
        }
        Debug.LogWarning("ItemTooltipText component not found in the scene.");
    }

    private void SetupEquipmentButtons()
    {
        for (int i = 0; i < equipmentButtons.Length; i++)
        {
            if (equipmentButtons[i] != null)
            {
                int slotIndex = i; 
            
                var trigger = equipmentButtons[i].gameObject.GetComponent<EventTrigger>();
                if (trigger == null)
                {
                    trigger = equipmentButtons[i].gameObject.AddComponent<EventTrigger>();
                }
            
                var pointerEnter = new EventTrigger.Entry();
                pointerEnter.eventID = EventTriggerType.PointerEnter;
                pointerEnter.callback.AddListener((data) => {
                    ShowEquipmentTooltip(equipmentSlots[slotIndex]);
                });
                trigger.triggers.Add(pointerEnter);
            
                var pointerExit = new EventTrigger.Entry();
                pointerExit.eventID = EventTriggerType.PointerExit;
                pointerExit.callback.AddListener((data) => {
                    HideEquipmentTooltip();
                });
                trigger.triggers.Add(pointerExit);
            }
        }
    }

    public void HideEquipmentTooltip()
    {
        if (itemStatsDisplay != null)
        {
            itemStatsDisplay.gameObject.SetActive(false);
        }
    }

    public void ShowEquipmentTooltip(ItemSlot slot)
    {
        if (itemStatsDisplay == null) return;

        ItemData equippedItem = GetEquippedItem(slot);
    
        if (equippedItem == null)
        {
            itemStatsDisplay.text = $"<color=#888888>No {slot} equipped</color>";
        }
        else
        {
            itemStatsDisplay.text = FormatItemStats(equippedItem);
        }

        itemStatsDisplay.gameObject.SetActive(true);
    }

    private void InitializeEquipmentSlots()
    {
        equippedItems[ItemSlot.Head] = null;
        equippedItems[ItemSlot.Torso] = null;
        equippedItems[ItemSlot.Shoes] = null;
        equippedItems[ItemSlot.Weapon] = null;
    }

    public void EquipItem(ItemData item)
    {
        if (item == null) return;

        if (equippedItems[item.itemSlot] != null)
        {
            Debug.Log($"Replaced '{equippedItems[item.itemSlot].name}' with '{item.name}' in slot {item.itemSlot}.");
        }

        equippedItems[item.itemSlot] = item;
        Debug.Log($"Equipped '{item.name}'.");
        
        ShowStatsTemporarily(item);

        OnEquipmentChanged?.Invoke(item.itemSlot);
        
        HealthSystem.Instance.UpdateMaxHealth();
    }

    private void ShowStatsTemporarily(ItemData item)
    {
        if (itemStatsDisplay == null)
        {
            Debug.LogWarning("Item Stats Display is not assigned in the EquipmentManager.");
            return;
        }

        if (displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
        }
        
        displayCoroutine = StartCoroutine(DisplayStatsCoroutine(item));
    }

    private IEnumerator DisplayStatsCoroutine(ItemData item)
    {
        itemStatsDisplay.text = FormatItemStats(item);
        
        itemStatsDisplay.gameObject.SetActive(true);
        
        yield return new WaitForSeconds(statsDisplayDuration);
        
        itemStatsDisplay.gameObject.SetActive(false);
        displayCoroutine = null; 
    }

    private string FormatItemStats(ItemData item)
    {
        if (item == null) return "";

        StringBuilder sb = new StringBuilder();

        sb.AppendLine($"<b><color=#FFD700>{item.name}</color></b>"); 
        sb.AppendLine($"<size=80%><i>{item.itemSlot}</i></size>\n");

        if (item.damageBonus > 0) sb.AppendLine($"+{item.damageBonus} Damage");
        if (item.defenseBonus > 0) sb.AppendLine($"+{item.defenseBonus} Defense");
        if (item.healBonus > 0) sb.AppendLine($"+{item.healBonus} Healing");
        if (item.movementSpeedBonus > 0) sb.AppendLine($"+{item.movementSpeedBonus} Move Speed");
        if (item.maxHpBonus > 0) sb.AppendLine($"+{item.maxHpBonus} Max HP");
        if (item.maxApBonus > 0) sb.AppendLine($"+{item.maxApBonus} Max AP");
        if (item.range > 0 && item.itemSlot == ItemSlot.Weapon) sb.AppendLine($"Range: {item.range}");

        bool hasClassBonus = (item.classBonus1Amount > 0 || item.classBonus2Amount > 0);
        if (hasClassBonus)
        {
            string classColor = (playerClass == item.itemClass) ? "#88FF88" : "#FF8888";
            sb.AppendLine($"\n<color={classColor}><b>{item.itemClass} Bonus:</b></color>");
            if (item.classBonus1Amount > 0) sb.AppendLine($"+{item.classBonus1Amount} {item.classBonus1Type}");
            if (item.classBonus2Amount > 0) sb.AppendLine($"+{item.classBonus2Amount} {item.classBonus2Type}");
        }

        return sb.ToString();
    }

    public void UnequipItem(ItemSlot slot)
    {
        if (equippedItems.ContainsKey(slot) && equippedItems[slot] != null)
        {
            Debug.Log($"Unequipped {equippedItems[slot].name} from slot {slot}.");
            equippedItems[slot] = null;
            
            OnEquipmentChanged?.Invoke(slot);
            
            HealthSystem.Instance.UpdateMaxHealth();
        }
    }
    
    public void UnequipAll()
    {
        List<ItemSlot> slotsToUnequip = new List<ItemSlot>(equippedItems.Keys);
        foreach(var slot in slotsToUnequip)
        {
            UnequipItem(slot);
        }
        Debug.Log("All items unequipped.");
    }

    public int GetTotalDamageBonus()
    {
        int bonus = 0;
        foreach (var item in equippedItems.Values)
        {
            if (item == null) continue;
            bonus += item.damageBonus;
            
            if (playerClass == item.itemClass)
            {
                if (item.classBonus1Type == StatBonusType.Dmg) bonus += item.classBonus1Amount;
                if (item.classBonus2Type == StatBonusType.Dmg) bonus += item.classBonus2Amount;
            }
        }
        return bonus;
    }

    public int GetTotalDefenseBonus()
    {
        int bonus = 0;
        foreach (var item in equippedItems.Values)
        {
            if (item == null) continue;
            bonus += item.defenseBonus;
            
            if (playerClass == item.itemClass)
            {
                if (item.classBonus1Type == StatBonusType.Def) bonus += item.classBonus1Amount;
                if (item.classBonus2Type == StatBonusType.Def) bonus += item.classBonus2Amount;
            }
        }
        return bonus;
    }

    public int GetTotalHealBonus()
    {
        int bonus = 0;
        foreach (var item in equippedItems.Values)
        {
            if (item == null) continue;
            bonus += item.healBonus;
            
            if (playerClass == item.itemClass)
            {
                if (item.classBonus1Type == StatBonusType.Heal) bonus += item.classBonus1Amount;
                if (item.classBonus2Type == StatBonusType.Heal) bonus += item.classBonus2Amount;
            }
        }
        return bonus;
    }

    public int GetTotalMovementSpeedBonus()
    {
        int bonus = 0;
        foreach (var item in equippedItems.Values)
        {
            if (item == null) continue;
            bonus += item.movementSpeedBonus;
            
            if (playerClass == item.itemClass)
            {
                if (item.classBonus1Type == StatBonusType.MS) bonus += item.classBonus1Amount;
                if (item.classBonus2Type == StatBonusType.MS) bonus += item.classBonus2Amount;
            }
        }
        return bonus;
    }

    public int GetTotalMaxHPBonus()
    {
        int bonus = 0;
        foreach (var item in equippedItems.Values)
        {
            if (item == null) continue;
            bonus += item.maxHpBonus;
            
            if (playerClass == item.itemClass)
            {
                if (item.classBonus1Type == StatBonusType.MaxHP) bonus += item.classBonus1Amount;
                if (item.classBonus2Type == StatBonusType.MaxHP) bonus += item.classBonus2Amount;
            }
        }
        return bonus;
    }

    public int GetTotalAPBonus()
    {
        int bonus = 0;
        foreach (var item in equippedItems.Values)
        {
            if (item == null) continue;
            bonus += item.maxApBonus;
            
            if (playerClass == item.itemClass)
            {
                if (item.classBonus1Type == StatBonusType.AP) bonus += item.classBonus1Amount;
                if (item.classBonus2Type == StatBonusType.AP) bonus += item.classBonus2Amount;
            }
        }
        return bonus;
    }

    public int GetWeaponRange()
    {
        return equippedItems[ItemSlot.Weapon]?.range ?? 1;
    }

    public ItemData GetEquippedItem(ItemSlot slot)
    {
        return equippedItems.ContainsKey(slot) ? equippedItems[slot] : null;
    }
}