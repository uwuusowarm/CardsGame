/*using UnityEngine;
using UnityEngine.UI; 

[RequireComponent(typeof(Button))] 
public class DamageButtonHelper : MonoBehaviour
{
    public ClickToDamageController damageController;

    private Button _button;
    
    private int damageToDeal;

    void Start()
    {
        _button = GetComponent<Button>();
        if (damageController == null)
        {
            Debug.LogError("DamageButtonHelper: ClickToDamageController not assigned!", this);
            _button.interactable = false; 
        }
    }
    
    public void TriggerDamageModeFromParent()
    {
        if (damageController == null)
        {
            Debug.LogError("Cannot trigger damage mode: Controller reference is missing.", this);
            return;
        }
        
        Transform parentTransform = transform.parent;
        if (parentTransform == null)
        {
            Debug.LogError("Button has no parent object!", this);
            return;
        }
        Debug.Log(parentTransform.name + " trigger damage mode from parent!", this);
        
        CardTemplate damageInfo = parentTransform.GetComponent<CardTemplate>();
        if (damageInfo == null)
        {
            Debug.LogError($"Parent object '{parentTransform.name}' does not have a CardTemplate component!", parentTransform);
            return; 
        }

       

        Debug.Log(damageInfo.leftVal, this);
        damageController.ActivateDamageMode(damageToDeal);
    }
}*/