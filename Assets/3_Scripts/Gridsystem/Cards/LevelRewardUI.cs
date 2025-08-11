using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LevelRewardUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject rewardPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI killCountText;
    [SerializeField] private Transform cardContainer;
    [SerializeField] private Button continueButton;
    
    [Header("Animation")]
    [SerializeField] private float cardRevealDelay = 0.3f;
    [SerializeField] private float animationDuration = 0.5f;
    
    private List<CardData> rewardCards = new List<CardData>();
    private bool hasSelectedCard = false;
    
    private void Start()
    {
        if (rewardPanel != null)
            rewardPanel.SetActive(false);
            
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinuePressed);
    }
    
    public void ShowRewards()
    {
        if (LevelRewardSystem.Instance == null)
        {
            Debug.LogError("LevelRewardSystem instance not found!");
            return;
        }
        
        rewardCards = LevelRewardSystem.Instance.GenerateRewards();
        
        if (rewardCards.Count == 0)
        {
            Debug.LogError("No reward cards generated!");
            return;
        }
        
        if (rewardPanel != null)                                 
            rewardPanel.SetActive(true);
        
        int killCount = LevelRewardSystem.Instance.GetEnemiesKilled();
        if (titleText != null)
            titleText.text = "Level Complete! Choose Your Reward";
            
        if (killCountText != null)
            killCountText.text = $"Enemies Defeated: {killCount}";
        
        foreach (Transform child in cardContainer)
        {
            Destroy(child.gameObject);
        }
        
        StartCoroutine(RevealCardsSequentially());
        
        hasSelectedCard = false;
        if (continueButton != null)
            continueButton.interactable = false;
    }
    
    private IEnumerator RevealCardsSequentially()
    {
        for (int i = 0; i < rewardCards.Count; i++)
        {
            CreateRewardCard(rewardCards[i], i);
            yield return new WaitForSeconds(cardRevealDelay);
        }
    }
    
    private void CreateRewardCard(CardData cardData, int index)
    {
        GameObject prefabToUse = CardManager.Instance.CardPrefab;
    
        if (prefabToUse == null || cardContainer == null)
        {
            Debug.LogError("Card prefab not found in CardManager!");
            return;
        }
    
        GameObject cardObj = Instantiate(prefabToUse, cardContainer);
    
        CardUI cardUI = cardObj.GetComponent<CardUI>();
        if (cardUI != null)
        {
            cardUI.Initialize(cardData);
        }
    
        if (cardObj.TryGetComponent<CardDragHandler>(out var dragHandler))
        {
            Destroy(dragHandler);
        }
    
        cardObj.transform.rotation = Quaternion.identity;
        cardObj.transform.localScale = Vector3.one;
    
        Button cardButton = cardObj.GetComponent<Button>();
        if (cardButton == null)
            cardButton = cardObj.AddComponent<Button>();
        
        cardButton.onClick.AddListener(() => OnCardSelected(cardData));
    
        if (cardObj.GetComponent<CardHoverSound>() == null)
            cardObj.AddComponent<CardHoverSound>();
    
        StartCoroutine(AnimateCardReveal(cardObj));
    }
    
    private IEnumerator AnimateCardReveal(GameObject cardObj)
    {
        cardObj.transform.localScale = Vector3.zero;
        
        float elapsedTime = 0f;
        Vector3 targetScale = Vector3.one;
        
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / animationDuration;
            
            float bounceScale = BounceEaseOut(progress);
            cardObj.transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, bounceScale);
            
            yield return null;
        }
        
        cardObj.transform.localScale = targetScale;
    }
    
    private float BounceEaseOut(float t)
    {
        if (t < (1f / 2.75f))
        {
            return 7.5625f * t * t;
        }
        else if (t < (2f / 2.75f))
        {
            t -= (1.5f / 2.75f);
            return 7.5625f * t * t + 0.75f;
        }
        else if (t < (2.5f / 2.75f))
        {
            t -= (2.25f / 2.75f);
            return 7.5625f * t * t + 0.9375f;
        }
        else
        {
            t -= (2.625f / 2.75f);
            return 7.5625f * t * t + 0.984375f;
        }
    }
    
    private void OnCardSelected(CardData selectedCard)
    {
        if (hasSelectedCard)
            return;
            
        hasSelectedCard = true;
        
        if (GameDataManager.Instance != null && GameDataManager.Instance.selectedDeck != null)
        {
            GameDataManager.Instance.selectedDeck.Cards.Add(selectedCard);
            Debug.Log($"Added {selectedCard.cardName} to player's deck");
            
            var allDecks = new List<Deck> { GameDataManager.Instance.selectedDeck };
            SaveSystem.SaveDecks(allDecks);
        }
        
        if (Sound_Manager.instance != null)
            Sound_Manager.instance.Play("Hover_V1");
        
        StartCoroutine(AnimateCardSelection(selectedCard));
        
        if (continueButton != null)
            continueButton.interactable = true;
    }
    
    private IEnumerator AnimateCardSelection(CardData selectedCard)
    {
        foreach (Transform child in cardContainer)
        {
            Button cardButton = child.GetComponent<Button>();
            if (cardButton != null)
            {
                CardUI cardUI = child.GetComponent<CardUI>();
                if (cardUI != null && cardUI.GetCardData() == selectedCard)
                {
                    StartCoroutine(AnimateScale(child.gameObject, Vector3.one, Vector3.one * 1.1f, 0.2f));
                    
                    Image cardImage = child.GetComponent<Image>();
                    if (cardImage != null)
                        cardImage.color = Color.green;
                }
                else
                {
                    Image cardImage = child.GetComponent<Image>();
                    if (cardImage != null)
                        cardImage.color = Color.gray;
                }
                cardButton.interactable = false;
            }
        }
        yield return null;
    }
    
    private IEnumerator AnimateScale(GameObject obj, Vector3 startScale, Vector3 endScale, float duration)
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
            
            obj.transform.localScale = Vector3.Lerp(startScale, endScale, progress);
            yield return null;
        }
        
        obj.transform.localScale = endScale;
    }
    
    private void OnContinuePressed()
    {
        if (rewardPanel != null)
            rewardPanel.SetActive(false);
        
        if (LevelRewardSystem.Instance != null)
            LevelRewardSystem.Instance.ResetKillCount();
        
        Debug.Log("Continuing to next level...");
        StairsToMenu.Instance.NextLevel();
    }
}