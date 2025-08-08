using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialTooltipManager : MonoBehaviour
{
    [Header("Tooltip Configuration")] [SerializeField]
    private List<TooltipData> tooltips = new List<TooltipData>();

    [Header("UI References")] [SerializeField]
    private Canvas tooltipCanvas;

    [SerializeField] private GameObject tooltipPrefab;
    [SerializeField] private Button skipTutorialButton;

    [Header("Settings")] [SerializeField] private bool startAutomatically = true;
    [SerializeField] private float delayBetweenTooltips = 0.1f;
    [SerializeField] private bool pauseGameDuringTutorial = true;

    [Header("Audio (Optional)")] [SerializeField]
    private AudioClip tooltipAppearSound;

    [SerializeField] private AudioClip tooltipAdvanceSound;

    private int currentTooltipIndex = 0;
    private GameObject currentTooltipObject;
    private bool tutorialActive = false;
    private bool waitingForClick = false;
    private AudioSource audioSource;
    private float originalTimeScale;
    private List<GameObject> highlightedObjects = new List<GameObject>();

    public System.Action OnTutorialStarted;
    public System.Action OnTutorialCompleted;
    public System.Action<int> OnTooltipChanged;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (tooltipAppearSound != null || tooltipAdvanceSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (skipTutorialButton != null)
        {
            skipTutorialButton.onClick.AddListener(SkipTutorial);
            skipTutorialButton.gameObject.SetActive(false);
        }

        if (startAutomatically && tooltips.Count > 0)
        {
            StartTutorial();
        }
    }

    void Update()
    {
        if (tutorialActive && waitingForClick)
        {
            if (Input.GetMouseButtonDown(0))
            {
                HandleMouseClick();
            }
        }
    }

    public void StartTutorial()
    {
        if (tooltips.Count == 0)
        {
            Debug.LogWarning("No tooltips configured for tutorial!");
            return;
        }

        tutorialActive = true;
        currentTooltipIndex = 0;

        if (pauseGameDuringTutorial)
        {
            originalTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }

        if (skipTutorialButton != null)
        {
            skipTutorialButton.gameObject.SetActive(true);
        }

        OnTutorialStarted?.Invoke();
        ShowCurrentTooltip();
    }

    public void SkipTutorial()
    {
        StopAllCoroutines();

        if (currentTooltipObject != null)
        {
            Destroy(currentTooltipObject);
        }

        ClearAllHighlights();
        CompleteTutorial();
    }

    private void ShowCurrentTooltip()
    {
        if (currentTooltipIndex >= tooltips.Count)
        {
            CompleteTutorial();
            return;
        }

        StartCoroutine(ShowTooltipCoroutine(tooltips[currentTooltipIndex]));
    }

    private IEnumerator ShowTooltipCoroutine(TooltipData tooltipData)
    {
        if (currentTooltipIndex > 0 && currentTooltipIndex - 1 < tooltips.Count)
        {
            TooltipData previousData = tooltips[currentTooltipIndex - 1];
            if (previousData.targetObject != null && previousData.highlightTarget)
            {
                HighlightTargetObject(previousData.targetObject, false);
            }
        }

        if (currentTooltipObject != null)
        {
            yield return StartCoroutine(HideTooltip(currentTooltipObject, tooltipData.fadeOutDuration));
        }

        if (delayBetweenTooltips > 0)
        {
            yield return new WaitForSecondsRealtime(delayBetweenTooltips);
        }

        currentTooltipObject = CreateTooltip(tooltipData);

        PlaySound(tooltipAppearSound);

        yield return StartCoroutine(ShowTooltip(currentTooltipObject, tooltipData.fadeInDuration));

        waitingForClick = true;
        OnTooltipChanged?.Invoke(currentTooltipIndex);
    }


    private GameObject CreateTooltip(TooltipData data)
    {
        GameObject tooltip = Instantiate(tooltipPrefab, tooltipCanvas.transform);

        RectTransform rectTransform = tooltip.GetComponent<RectTransform>();

        if (data.targetObject != null)
        {
            Vector2 targetPosition = GetScreenPositionOfTarget(data.targetObject);
            rectTransform.anchoredPosition = targetPosition + data.targetOffset + data.position;

            if (data.highlightTarget)
            {
                HighlightTargetObject(data.targetObject, true, data); 
            }
        }
        else
        {
            rectTransform.anchoredPosition = data.position;
        }

        rectTransform.sizeDelta = data.size;

        TooltipPrefab prefabComponent = tooltip.GetComponent<TooltipPrefab>();
        if (prefabComponent != null)
        {
            if (prefabComponent.background != null)
            {
                if (data.backgroundSprite != null)
                    prefabComponent.background.sprite = data.backgroundSprite;
                prefabComponent.background.color = data.backgroundColor;
            }

            if (prefabComponent.titleText != null)
            {
                prefabComponent.titleText.text = data.title;
                prefabComponent.titleText.gameObject.SetActive(!string.IsNullOrEmpty(data.title));
            }

            if (prefabComponent.descriptionText != null)
            {
                prefabComponent.descriptionText.text = data.description;
            }
        }
        else
        {
            SetupTooltipFallback(tooltip, data);
        }

        if (data.waitForSpecificClick)
        {
            Button tooltipButton = tooltip.GetComponent<Button>();
            if (tooltipButton == null)
                tooltipButton = tooltip.AddComponent<Button>();

            tooltipButton.onClick.AddListener(HandleMouseClick);
        }

        CanvasGroup canvasGroup = tooltip.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = tooltip.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;

        return tooltip;
    }

    private void SetupTooltipFallback(GameObject tooltip, TooltipData data)
    {
        Image background = tooltip.GetComponent<Image>();
        if (background != null)
        {
            if (data.backgroundSprite != null)
                background.sprite = data.backgroundSprite;
            background.color = data.backgroundColor;
        }

        TextMeshProUGUI titleText = null;
        TextMeshProUGUI descriptionText = null;

        Transform titleTransform = tooltip.transform.Find("Title");
        if (titleTransform != null)
            titleText = titleTransform.GetComponent<TextMeshProUGUI>();

        Transform descriptionTransform = tooltip.transform.Find("Description");
        if (descriptionTransform != null)
            descriptionText = descriptionTransform.GetComponent<TextMeshProUGUI>();

        if (titleText == null || descriptionText == null)
        {
            TextMeshProUGUI[] allTexts = tooltip.GetComponentsInChildren<TextMeshProUGUI>();
            if (allTexts.Length >= 1 && titleText == null)
                titleText = allTexts[0];
            if (allTexts.Length >= 2 && descriptionText == null)
                descriptionText = allTexts[1];
        }

        if (titleText != null)
        {
            titleText.text = data.title;
            titleText.gameObject.SetActive(!string.IsNullOrEmpty(data.title));
        }
        else
        {
            Debug.LogWarning("No Title TextMeshPro component found in tooltip prefab!");
        }

        if (descriptionText != null)
        {
            descriptionText.text = data.description;
        }
        else
        {
            Debug.LogWarning("No Description TextMeshPro component found in tooltip prefab!");
        }
    }

    private Vector2 GetScreenPositionOfTarget(GameObject target)
    {
        if (target == null) return Vector2.zero;

        RectTransform targetRect = target.GetComponent<RectTransform>();
        if (targetRect != null)
        {
            Vector2 canvasPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                tooltipCanvas.transform as RectTransform,
                RectTransformUtility.WorldToScreenPoint(tooltipCanvas.worldCamera, targetRect.position),
                tooltipCanvas.worldCamera,
                out canvasPos);
            return canvasPos;
        }
        else
        {
            Vector3 worldPos = target.transform.position;
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

            Vector2 canvasPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                tooltipCanvas.transform as RectTransform,
                screenPos,
                tooltipCanvas.worldCamera,
                out canvasPos);

            return canvasPos;
        }
    }

    private void HighlightTargetObject(GameObject target, bool highlight, TooltipData tooltipData = null) {
    if (target == null) return;
    
    if (highlight)
    {
        highlightedObjects.Add(target);
    }
    else
    {
        highlightedObjects.Remove(target);
    }
    
    GlowHighlight glowHighlight = target.GetComponent<GlowHighlight>();
    if (glowHighlight != null)
    {
        if (highlight && tooltipData != null && tooltipData.customHighlightMaterial != null)
        {
            glowHighlight.SetCustomHighlightMaterial(tooltipData.customHighlightMaterial);
        }
        glowHighlight.ToggleGlow(highlight);
        return;
    }
    
    Hex hex = target.GetComponent<Hex>();
    if (hex != null)
    {
        if (highlight)
            hex.EnableHighlight();
        else
            hex.DisableHighlight();
        return;
    }
    
    EnemyHighlighter enemyHighlighter = target.GetComponent<EnemyHighlighter>();
    if (enemyHighlighter != null)
    {
        enemyHighlighter.ToggleHighlight(highlight);
        return;
    }
    
    Outline outline = target.GetComponent<Outline>();
    if (outline != null)
    {
        outline.enabled = highlight;
        return;
    }
    
    Renderer renderer = target.GetComponent<Renderer>();
    if (renderer != null)
    {
        TutorialHighlightHelper helper = target.GetComponent<TutorialHighlightHelper>();
        if (helper == null)
            helper = target.AddComponent<TutorialHighlightHelper>();
        
        if (highlight)
        {
            helper.StoreOriginalMaterial(renderer.material);
            helper.StoreOriginalColor(renderer.material.color);
            
            if (tooltipData != null && tooltipData.customHighlightMaterial != null)
            {
                renderer.material = tooltipData.customHighlightMaterial;
                if (tooltipData.customHighlightMaterial.HasProperty("_Color"))
                {
                    renderer.material.color = tooltipData.highlightColor;
                }
                if (tooltipData.customHighlightMaterial.HasProperty("_GlowColor"))
                {
                    renderer.material.SetColor("_GlowColor", tooltipData.highlightColor);
                }
            }
            else
            {
                renderer.material.color = tooltipData?.highlightColor ?? Color.yellow;
            }
        }
        else
        {
            helper.RestoreOriginalMaterial();
        }
        return;
    }
    
    Image image = target.GetComponent<Image>();
    if (image != null)
    {
        TutorialHighlightHelper helper = target.GetComponent<TutorialHighlightHelper>();
        if (helper == null)
            helper = target.AddComponent<TutorialHighlightHelper>();
        
        if (highlight)
        {
            helper.StoreOriginalColor(image.color);
            image.color = tooltipData?.highlightColor ?? Color.yellow;
        }
        else
        {
            image.color = helper.GetOriginalColor();
        }
    }
}

    private IEnumerator ShowTooltip(GameObject tooltip, float duration)
    {
        CanvasGroup canvasGroup = tooltip.GetComponent<CanvasGroup>();
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    private IEnumerator HideTooltip(GameObject tooltip, float duration)
    {
        CanvasGroup canvasGroup = tooltip.GetComponent<CanvasGroup>();
        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        Destroy(tooltip);
    }


    private void HandleMouseClick()
    {
        if (!waitingForClick) return;

        TooltipData currentData = tooltips[currentTooltipIndex];

        if (currentData.waitForSpecificClick)
        {
            return;
        }

        waitingForClick = false;
        PlaySound(tooltipAdvanceSound);

        if (currentData.targetObject != null && currentData.highlightTarget)
        {
            HighlightTargetObject(currentData.targetObject, false, null);
        }

        currentTooltipIndex++;
        ShowCurrentTooltip();
    }


    private void CompleteTutorial()
    {
        tutorialActive = false;
        waitingForClick = false;

        if (currentTooltipIndex > 0 && currentTooltipIndex - 1 < tooltips.Count)
        {
            TooltipData lastData = tooltips[currentTooltipIndex - 1];
            if (lastData.targetObject != null && lastData.highlightTarget)
            {
                HighlightTargetObject(lastData.targetObject, false);
            }
        }

        ClearAllHighlights();

        if (pauseGameDuringTutorial)
        {
            Time.timeScale = originalTimeScale;
        }

        if (skipTutorialButton != null)
        {
            skipTutorialButton.gameObject.SetActive(false);
        }
        
        if (currentTooltipObject != null)
        {
            Destroy(currentTooltipObject);
            currentTooltipObject = null;
        }

        OnTutorialCompleted?.Invoke();
    }


private void ClearAllHighlights()
{
    List<GameObject> objectsToProcess = new List<GameObject>(highlightedObjects);
    
    foreach (GameObject obj in objectsToProcess)
    {
        if (obj != null)
        {
            HighlightTargetObject(obj, false);
            
            TutorialHighlightHelper helper = obj.GetComponent<TutorialHighlightHelper>();
            if (helper != null)
            {
                helper.RestoreOriginalMaterial();
                Destroy(helper);
            }
        }
    }
    highlightedObjects.Clear();
}

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public void AddTooltip(TooltipData tooltip)
    {
        tooltips.Add(tooltip);
    }

    public void RemoveTooltip(int index)
    {
        if (index >= 0 && index < tooltips.Count)
        {
            tooltips.RemoveAt(index);
        }
    }

    public void ClearTooltips()
    {
        tooltips.Clear();
    }

    public bool IsTutorialActive()
    {
        return tutorialActive;
    }

    public int GetCurrentTooltipIndex()
    {
        return currentTooltipIndex;
    }

    public int GetTotalTooltipCount()
    {
        return tooltips.Count;
    }
}