using TMPro;
using UnityEngine;

public class HealthManager : MonoBehaviour
{
    [Header("Health Value")] [Range(0, 10)] public int health = 1;
    
    private TextMeshProUGUI healthText;
    private RectTransform healthBarTransform;
    
    public GameObject parentObject;

    public bool takeDamage = false;
    private int damage =1;
    
    private int maxHealth;
    void Awake()
    {
        maxHealth = health;
        healthText = transform.Find("healthVal").GetComponent<TextMeshProUGUI>();
        
        Transform healthBar = transform.Find("healthBar");

        if (healthBar != null)
            healthBarTransform = healthBar.GetComponent<RectTransform>();
    }
    void Start()
    {
        if (healthText != null) healthText.text = health.ToString();
    }
    
    void Update()
    {
        if(takeDamage) TakeDamage(damage);
    }

    public void TakeDamage(int damage)
    {
        if (health > 0)
        {
            health -= damage;
            UpdateHealthUI();
        }
        else if (health <= 0)
        {
            Destroy(parentObject);
        }
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
