using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardTemplate : MonoBehaviour
{
    [Header("Class")] public String cardClass;

    [Header("Card Name")] public String cardName;

    [Header("Left Value")] [Range(0, 10)] public int leftVal = 1;

    [Header("Right Value")] [Range(0, 10)] public int rightVal = 1;

    [Header("Range Value")] [Range(1, 3)] public int rangeVal = 1;

    [Header("Effect")] public String effectName;

    [Header("Illustration")] public Sprite illustrationBig;
    
    [Header("Icons")] public Sprite iconLeft; public Sprite iconRight;

    private TextMeshProUGUI valLeftText;
    private TextMeshProUGUI valRightText;
    private TextMeshProUGUI effectNameText;
    private TextMeshProUGUI cardNameText;
    private Image leftIconImage;
    private Image rightIconImage;
    private Image illustrationImage;


    void Awake()
    {
        valLeftText = transform.Find("valLeft").GetComponent<TextMeshProUGUI>();
        valRightText = transform.Find("valRight").GetComponent<TextMeshProUGUI>();
        effectNameText = transform.Find("effect").GetComponent<TextMeshProUGUI>();
        leftIconImage = transform.Find("leftIcon").GetComponent<Image>();
        rightIconImage = transform.Find("rightIcon").GetComponent<Image>();
        illustrationImage = transform.Find("illustration").GetComponent<Image>();
        cardNameText = transform.Find("name").GetComponent<TextMeshProUGUI>();
    }
    void Start()
    {
        if (valLeftText != null) valLeftText.text = leftVal.ToString();
        if (valRightText != null) valRightText.text = rightVal.ToString();
        if (effectNameText != null) effectNameText.text = effectName;
        if (leftIconImage != null) leftIconImage.sprite = iconLeft;
        if (rightIconImage != null) rightIconImage.sprite = iconRight;
        if (illustrationImage != null) illustrationImage.sprite = illustrationBig;
        if (cardNameText != null) cardNameText.text = cardName;
    }

    void Update()
    {

    }
}
