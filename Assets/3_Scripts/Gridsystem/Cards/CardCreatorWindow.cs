#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

public class CardCreatorWindow : EditorWindow
{
    private CardData newCard;
    private Vector2 scrollPos;
    private Texture2D cardTexture;
    private GameObject cardPrefabTemplate;
    private Dictionary<string, Sprite> classBackgrounds = new Dictionary<string, Sprite>();

    [MenuItem("Tools/Card Creator")]
    public static void ShowWindow()
    {
        GetWindow<CardCreatorWindow>("Card Creator");
    }

    private void OnEnable()
    {
        newCard = CreateInstance<CardData>();
        LoadClassBackgrounds();
        LoadCardTemplate();
    }

    private void LoadCardTemplate()
    {
        string templatePath = "Assets/3_Scripts/Gridsystem/Cards/Card.prefab";
        cardPrefabTemplate = AssetDatabase.LoadAssetAtPath<GameObject>(templatePath);

        if (cardPrefabTemplate == null)
        {
            Debug.LogError($"Could not load card template at path: {templatePath}");
        }
    }

    private void LoadClassBackgrounds()
    {
        string bgPath = "Assets/3_Scripts/Gridsystem/Cards/Backgrounds";
        string[] bgGUIDs = AssetDatabase.FindAssets("t:Sprite", new[] { bgPath });

        foreach (string guid in bgGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Sprite bg = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (bg != null)
            {
                classBackgrounds[bg.name] = bg;
            }
        }
    }

    private void OnGUI()
    {
        if (cardPrefabTemplate == null)
        {
            EditorGUILayout.HelpBox("Card template not found! Please ensure Card.prefab exists in the correct path.", MessageType.Error);
            if (GUILayout.Button("Try Reload Template"))
            {
                LoadCardTemplate();
            }
            return;
        }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        EditorGUILayout.LabelField("Card Basics", EditorStyles.boldLabel);
        newCard.cardName = EditorGUILayout.TextField("Name", newCard.cardName);
        newCard.manaCost = EditorGUILayout.IntField("Mana Cost", newCard.manaCost);
        newCard.description = EditorGUILayout.TextArea(newCard.description, GUILayout.Height(50));
        newCard.cardArt = (Sprite)EditorGUILayout.ObjectField("Art", newCard.cardArt, typeof(Sprite), false);
        newCard.cardClass = (CardClass)EditorGUILayout.EnumPopup("Class", newCard.cardClass);
        newCard.rarity = (CardRarity)EditorGUILayout.EnumPopup("Rarity", newCard.rarity);

        DrawEffectSection("Left Swipe Effects", newCard.leftEffects);
        DrawEffectSection("Right Swipe Effects", newCard.rightEffects);
        DrawEffectSection("Always Active Effects", newCard.alwaysEffects);

        if (GUILayout.Button("Save Card", GUILayout.Height(40)))
        {
            SaveCard();
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawEffectSection(string header, List<CardEffect> effects)
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField(header, EditorStyles.boldLabel);

        for (int i = 0; i < effects.Count; i++)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            effects[i].effectType = (CardEffect.EffectType)EditorGUILayout.EnumPopup("Type", effects[i].effectType);
            effects[i].value = EditorGUILayout.IntField("Value", effects[i].value);

            if (effects[i].effectType == CardEffect.EffectType.Attack)
            {
                effects[i].range = EditorGUILayout.IntField("Range", effects[i].range);
            }

            if (GUILayout.Button("Remove Effect"))
            {
                effects.RemoveAt(i);
                i--;
            }

            EditorGUILayout.EndVertical();
        }

        if (GUILayout.Button("Add Effect"))
        {
            effects.Add(new CardEffect());
        }
    }

    private void SaveCard()
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "Save Card",
            "Card_" + newCard.cardName,
            "asset",
            "Select where to save the card");

        if (string.IsNullOrEmpty(path)) return;
        string directory = Path.GetDirectoryName(path);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        AssetDatabase.CreateAsset(newCard, path);
        AssetDatabase.SaveAssets();
        CreateCardPrefab(path);

        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = newCard;
    }

    private void CreateCardPrefab(string cardDataPath)
    {
        GameObject cardInstance = (GameObject)PrefabUtility.InstantiatePrefab(cardPrefabTemplate);

        try
        {
            CardUI cardUI = cardInstance.GetComponent<CardUI>();
            if (cardUI == null)
            {
                Debug.LogError("Card template missing CardUI component!");
                DestroyImmediate(cardInstance);
                return;
            }

            cardUI.Initialize(newCard);
            Image background = cardInstance.transform.Find("Background")?.GetComponent<Image>();
            if (background != null && classBackgrounds.TryGetValue(newCard.cardClass.ToString(), out Sprite bgSprite))
            {
                background.sprite = bgSprite;
            }
            Image border = cardInstance.transform.Find("Border")?.GetComponent<Image>();
            if (border != null)
            {
                border.color = GetRarityColor(newCard.rarity);
            }
            string prefabPath = Path.Combine(
                Path.GetDirectoryName(cardDataPath),
                Path.GetFileNameWithoutExtension(cardDataPath) + ".prefab");

            prefabPath = AssetDatabase.GenerateUniqueAssetPath(prefabPath);
            PrefabUtility.SaveAsPrefabAsset(cardInstance, prefabPath);
        }
        finally
        {
            DestroyImmediate(cardInstance);
        }
    }

    private Color GetRarityColor(CardRarity rarity)
    {
        switch (rarity)
        {
            case CardRarity.Common: return new Color(0.65f, 0.5f, 0.35f); 
            case CardRarity.Rare: return new Color(0.75f, 0.75f, 0.75f); 
            case CardRarity.Legendary: return new Color(1f, 0.84f, 0f);
            default: return Color.white;
        }
    }
}
#endif