using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TooltipPrefab : MonoBehaviour
{
    [Header("UI Components")]
    public Image background;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public Button nextButton;
    
    void Awake()
    {
        if (background == null)
            background = GetComponent<Image>();
        
        if (titleText == null)
            titleText = transform.Find("Title")?.GetComponent<TextMeshProUGUI>();
        
        if (descriptionText == null)
            descriptionText = transform.Find("Description")?.GetComponent<TextMeshProUGUI>();
        
        if (nextButton == null)
            nextButton = GetComponentInChildren<Button>();
    }
}