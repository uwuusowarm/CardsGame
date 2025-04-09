using TMPro; 
using UnityEngine;

public class HealthManager : MonoBehaviour
{
    [Header("Health Value")]
    [Range(0, 30)]
    public int health = 1; 

    private TextMeshProUGUI healthText; 
    private RectTransform healthBarTransform; 

    private int maxHealth; 
    
    void Awake()
    {
        maxHealth = health;
        
        Transform textChild = transform.Find("healthVal");
        if (textChild) healthText = textChild.GetComponent<TextMeshProUGUI>();

        Transform barChild = transform.Find("healthBar");
        if (barChild) healthBarTransform = barChild.GetComponent<RectTransform>();
    }

    void Start()
    {
        UpdateHealthUI(); 
    }

    public void TakeDamage(int damage)
    {
        health -= damage; 

        if (health <= 0)
        {
            health = 0; 
            if (transform.parent.gameObject != null)
            {
                Destroy(transform.parent.gameObject);
            }
        }
        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        if (healthText != null)
            healthText.text = health.ToString(); 

        if (healthBarTransform != null)
        {
            float scaleX = Mathf.Clamp01((float)health / maxHealth);
            healthBarTransform.localScale = new Vector3(scaleX, 1f, 1f);
        }
    }
}