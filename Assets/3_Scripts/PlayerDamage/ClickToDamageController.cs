using UnityEngine;
using UnityEngine.EventSystems;

public class ClickToDamageController : MonoBehaviour
{
    public Camera mainCamera;

    private bool isDamageModeActive = false;
    private int currentDamageToDeal = 0;

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("Main Camera not found or assigned!", this);
                this.enabled = false;
            }
        }
    }

    public void ActivateDamageMode(int damageFromSource)
    {
        currentDamageToDeal = damageFromSource; 
        isDamageModeActive = true;
        Debug.Log($"Damage Mode Activated. Ready to deal {currentDamageToDeal} damage. Click on a target.");
    }

    void Update()
    {
        if (!isDamageModeActive || !Input.GetMouseButtonDown(0))
        {
            return;
        }
        
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 5.0f);
        RaycastHit hit;

        Debug.Log("Casting Ray from mouse position");


        if (Physics.Raycast(ray, out hit))
        {
            Debug.Log("Ray hit: " + hit.collider.gameObject.name);

            HealthManager target = hit.collider.GetComponent<HealthManager>();

            if (target != null)
            {
                Debug.Log("Dealing damage.");
                target.TakeDamage(currentDamageToDeal);
            }
            else
            {
                Debug.Log("Hit object does not have HealthManager script.");
            }
        }
        else
        {
            Debug.Log("Ray did not hit any colliders.");
        }


        isDamageModeActive = false;
        Debug.Log("Damage Mode Deactivated.");
    }
}