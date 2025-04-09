using UnityEngine;
using UnityEngine.UI;

// Make sure PossibleAction enum is defined

// Removed the ActionSlot enum

[RequireComponent(typeof(Button))]
public class DamagHelperLeftRight : MonoBehaviour
{
    [Tooltip("Assign the GameObject that has ClickToDamageController")]
    public ClickToDamageController damageController;
    
    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        if (damageController == null)
        {
            Debug.LogError("DamageButtonHelper: ClickToDamageController not assigned!", this);
            if (button != null) button.interactable = false;
        }
        else
        {
            if (button != null && button.onClick.GetPersistentEventCount() == 0)
            {
                Debug.LogWarning($"Button '{gameObject.name}' has no OnClick event assigned in the Inspector. Please assign DamageButtonHelper -> TriggerDamageModeFromParent.", this);
            }
        }
    }

    public void TriggerDamageModeFromParent()
    {
        if (damageController == null) {
            Debug.LogError("Cannot trigger damage mode: Controller reference is missing.", this);
            return;
        }

        Transform parentTransform = transform.parent;
        if (parentTransform == null) {
            Debug.LogError("Button has no parent object!", this);
            return;
        }

        CardTemplate damageInfo = parentTransform.GetComponent<CardTemplate>();
        if (damageInfo == null) {
            Debug.LogError($"Parent object '{parentTransform.name}' does not have a DamageSourceInfo component!", parentTransform);
            return;
        }

        if (damageInfo.actionLeft == ActionList.Attack)
        {
            int valueToUse = damageInfo.leftVal;
            damageController.ActivateDamageMode(valueToUse);
            Debug.Log($"Activated damage mode via Left Slot ('{damageInfo.actionLeft}'). Damage: {valueToUse}");
            return; 
        }

        if (damageInfo.actionRight == ActionList.Attack)
        {
            int valueToUse = damageInfo.rightVal;
            damageController.ActivateDamageMode(valueToUse);
            Debug.Log($"Activated damage mode via Right Slot ('{damageInfo.actionRight}'). Damage: {valueToUse}");
            return;
        }
        
        Debug.Log($"No slot on parent '{parentTransform.name}' is set to 'Attack'. Damage mode not activated.");
    }
}