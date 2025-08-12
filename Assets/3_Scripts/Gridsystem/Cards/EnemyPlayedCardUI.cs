
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyPlayedCardUI : MonoBehaviour
{
    [Header("Card Display")]
    [SerializeField] private GameObject cardDisplayContainer;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private float displayDuration = 3f;
    
    [Header("Animation")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private Vector3 slideInOffset = new Vector3(0, 100f, 0);
    
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI enemyNameText;
    [SerializeField] private TextMeshProUGUI actionText;
    
    private GameObject currentCardObject;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Coroutine displayCoroutine;
    
    public static EnemyPlayedCardUI Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Initialize()
    {
        if (cardDisplayContainer == null)
            cardDisplayContainer = gameObject;
            
        canvasGroup = cardDisplayContainer.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = cardDisplayContainer.AddComponent<CanvasGroup>();
            
        rectTransform = cardDisplayContainer.GetComponent<RectTransform>();
        
        canvasGroup.alpha = 0f;
        cardDisplayContainer.SetActive(false);
    }
    
    public void ShowEnemyPlayedCard(CardData cardData, string enemyName, string action, bool actionPerformed = false)
    {
        if (cardData == null) return;
        
        if (!actionPerformed)
        {
            Debug.Log($"Enemy {enemyName} played card {cardData.cardName} but no action was performed - not showing UI");
            return;
        }
        
        if (displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
        }
        
        displayCoroutine = StartCoroutine(DisplayCardCoroutine(cardData, enemyName, action));
    }

    public void ShowEnemyPlayedCardIfActionable(CardData cardData, string enemyName, string action = "plays")
    {
        if (cardData == null) return;
        
        bool hasActionableEffect = CardHasActionableEffects(cardData);
        
        if (!hasActionableEffect)
        {
            Debug.Log($"Enemy {enemyName} played card {cardData.cardName} with no actionable effects - not showing UI");
            return;
        }
        
        ShowEnemyPlayedCard(cardData, enemyName, action, true);
    }
    
    private bool CardHasActionableEffects(CardData cardData)
    {
        if (cardData.leftEffects != null)
        {
            foreach (var effect in cardData.leftEffects)
            {
                if (IsActionableEffect(effect))
                    return true;
            }
        }
        
        if (cardData.rightEffects != null)
        {
            foreach (var effect in cardData.rightEffects)
            {
                if (IsActionableEffect(effect))
                    return true;
            }
        }
        
        return false;
    }

    private bool IsActionableEffect(CardEffect effect)
    {
        switch (effect.effectType)
        {
            case CardEffect.EffectType.Attack:
            case CardEffect.EffectType.Move:
            case CardEffect.EffectType.Heal:
            case CardEffect.EffectType.Burn:
            case CardEffect.EffectType.Stun:
            case CardEffect.EffectType.Poison:
            case CardEffect.EffectType.Freeze:
                return true;
                
            case CardEffect.EffectType.Block:
                return effect.value > 0;
                
            case CardEffect.EffectType.Draw:
            case CardEffect.EffectType.Discard:
                return effect.value > 0; 
                
            case CardEffect.EffectType.ActionPlus:
                return effect.value > 0; 
                
            case CardEffect.EffectType.None:
            default:
                return false;
        }
    }
    
    private IEnumerator DisplayCardCoroutine(CardData cardData, string enemyName, string action)
    {
        CreateCardDisplay(cardData, enemyName, action);
        
        cardDisplayContainer.SetActive(true);
        
        yield return StartCoroutine(AnimateIn());
        
        yield return new WaitForSeconds(displayDuration);
        
        yield return StartCoroutine(AnimateOut());
        
        cardDisplayContainer.SetActive(false);
        
        if (currentCardObject != null)
        {
            Destroy(currentCardObject);
            currentCardObject = null;
        }
        
        displayCoroutine = null;
    }
    
    private void CreateCardDisplay(CardData cardData, string enemyName, string action)
    {
        if (currentCardObject != null)
        {
            Destroy(currentCardObject);
        }
        
        if (cardPrefab != null)
        {
            currentCardObject = Instantiate(cardPrefab, cardDisplayContainer.transform);
        }
        else
        {
            currentCardObject = Instantiate(cardData.cardPrefab, cardDisplayContainer.transform);
        }
        
        CardUI cardUI = currentCardObject.GetComponent<CardUI>();
        if (cardUI != null)
        {
            cardUI.Initialize(cardData);
        }
        
        CardDragHandler dragHandler = currentCardObject.GetComponent<CardDragHandler>();
        if (dragHandler != null)
        {
            Destroy(dragHandler);
        }
        
        if (enemyNameText != null)
        {
            enemyNameText.text = enemyName;
        }
        
        if (actionText != null)
        {
            actionText.text = $"{action} {cardData.cardName}";
        }
        
        RectTransform cardRect = currentCardObject.GetComponent<RectTransform>();
        if (cardRect != null)
        {
            cardRect.anchoredPosition = Vector2.zero;
            cardRect.localScale = Vector3.one;
        }
    }
    
    private IEnumerator AnimateIn()
    {
        Vector2 startPosition = rectTransform.anchoredPosition + (Vector2)slideInOffset;
        Vector2 endPosition = rectTransform.anchoredPosition;
        
        float elapsed = 0f;
        
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / fadeInDuration;
            
            float easedProgress = 1f - Mathf.Pow(1f - progress, 3f);
            
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, easedProgress);
            rectTransform.anchoredPosition = Vector3.Lerp(startPosition, endPosition, easedProgress);
            
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
        rectTransform.anchoredPosition = endPosition;
    }
    
    private IEnumerator AnimateOut()
    {
        Vector3 startPosition = rectTransform.anchoredPosition;
        Vector3 endPosition = rectTransform.anchoredPosition - (Vector2)slideInOffset;
        
        float elapsed = 0f;
        
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / fadeOutDuration;
            
            float easedProgress = Mathf.Pow(progress, 3f);
            
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, easedProgress);
            rectTransform.anchoredPosition = Vector3.Lerp(startPosition, endPosition, easedProgress);
            
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
        rectTransform.anchoredPosition = endPosition;
    }

    public void HideCardDisplay()
    {
        if (displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
            displayCoroutine = null;
        }
        
        cardDisplayContainer.SetActive(false);
        canvasGroup.alpha = 0f;
        
        if (currentCardObject != null)
        {
            Destroy(currentCardObject);
            currentCardObject = null;
        }
    }
    
    public bool IsDisplayingCard()
    {
        return displayCoroutine != null;
    }
}