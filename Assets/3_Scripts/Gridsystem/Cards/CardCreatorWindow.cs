#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using TMPro;

public class CardCreatorWindow : EditorWindow
{
    private CardData newCard;
    private Vector2 scrollPos;
    private GameObject cardPrefabTemplate;
    private Dictionary<string, Sprite> classBackgrounds = new Dictionary<string, Sprite>();
    private Dictionary<CardRarity, Sprite> rarityBorders = new Dictionary<CardRarity, Sprite>();
    private Dictionary<CardEffect.EffectType, Sprite> effectIcons = new Dictionary<CardEffect.EffectType, Sprite>();

    [MenuItem("Tools/Card Creator")]
    public static void ShowWindow()
    {
        GetWindow<CardCreatorWindow>("Card Creator");
    }

    private void OnEnable()
    {
        newCard = CreateInstance<CardData>();
        LoadResources();
    }

    private void LoadResources()
    {

        cardPrefabTemplate = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/3_Scripts/Gridsystem/Cards/Card.prefab");

        string bgPath = "Assets/2_Art/Cards/Card Layouts";
        classBackgrounds = new Dictionary<string, Sprite>()
        {
            { "Wizard", AssetDatabase.LoadAssetAtPath<Sprite>($"{bgPath}/Card_Layout_Wizard_v1_NC.png") },
            { "Warrior", AssetDatabase.LoadAssetAtPath<Sprite>($"{bgPath}/Card_Layout_Warrior_v1_NC.png") },
            { "Rogue", AssetDatabase.LoadAssetAtPath<Sprite>($"{bgPath}/Card_Layout_Rogue_v1_NC.png") },
            { "Monster", AssetDatabase.LoadAssetAtPath<Sprite>($"{bgPath}/Card_Layout_Monster_v1_NC.png") },
            { "Base", AssetDatabase.LoadAssetAtPath<Sprite>($"{bgPath}/Card_Layout_Base_v1_NC.png") }
        };

        string rarityPath = "Assets/2_Art/Cards/Rarity Layer";
        rarityBorders = new Dictionary<CardRarity, Sprite>()
        {
            { CardRarity.Common, AssetDatabase.LoadAssetAtPath<Sprite>($"{rarityPath}/Card_Rarity_Common_v1_NC.png") },
            { CardRarity.Rare, AssetDatabase.LoadAssetAtPath<Sprite>($"{rarityPath}/Card_Rarity_Rare_v1_NC.png") },
            { CardRarity.Legendary, AssetDatabase.LoadAssetAtPath<Sprite>($"{rarityPath}/Card_Rarity_Legendary_v1_NC.png") },
            { CardRarity.Monster, AssetDatabase.LoadAssetAtPath<Sprite>($"{rarityPath}/Card_Rarity_Monster_v1_NC.png") }
        };

        string iconsPath = "Assets/2_Art/Cards/Card Icons";
        effectIcons = new Dictionary<CardEffect.EffectType, Sprite>()
        {
            { CardEffect.EffectType.Attack, AssetDatabase.LoadAssetAtPath<Sprite>($"{iconsPath}/Card_Icon_Core_Attack_v1_NC.png") },
            { CardEffect.EffectType.Block, AssetDatabase.LoadAssetAtPath<Sprite>($"{iconsPath}/Card_Icon_Core_Defense_v1_NC.png") },
            { CardEffect.EffectType.Heal, AssetDatabase.LoadAssetAtPath<Sprite>($"{iconsPath}/Card_Icon_Core_Heal_v1_NC.png") },
            { CardEffect.EffectType.Move, AssetDatabase.LoadAssetAtPath<Sprite>($"{iconsPath}/Card_Icon_Core_Movement_v1_NC.png") }
        };
    }

    private void OnGUI()
    {
        if (cardPrefabTemplate == null)
        {
            EditorGUILayout.HelpBox("Card template not found!", MessageType.Error);
            if (GUILayout.Button("Reload Resources"))
            {
                LoadResources();
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
            EditorGUILayout.BeginHorizontal();

            if (effectIcons.TryGetValue(effects[i].effectType, out Sprite icon))
            {
                Rect rect = GUILayoutUtility.GetRect(40, 40);
                GUI.DrawTexture(rect, icon.texture, ScaleMode.ScaleToFit);
            }

            EditorGUILayout.BeginVertical();
            effects[i].effectType = (CardEffect.EffectType)EditorGUILayout.EnumPopup("Type", effects[i].effectType);
            effects[i].value = EditorGUILayout.IntField("Value", effects[i].value);
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

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
        if (newCard.leftEffects.Count > 0 && effectIcons.TryGetValue(newCard.leftEffects[0].effectType, out Sprite leftIcon))
        {
            newCard.leftEffectIcon = leftIcon;
        }

        if (newCard.rightEffects.Count > 0 && effectIcons.TryGetValue(newCard.rightEffects[0].effectType, out Sprite rightIcon))
        {
            newCard.rightEffectIcon = rightIcon;
        }
        if (classBackgrounds.TryGetValue(newCard.cardClass.ToString(), out Sprite bgSprite))
        {
            newCard.backgroundSprite = bgSprite;
        }
        if (rarityBorders.TryGetValue(newCard.rarity, out Sprite border))
        {
            newCard.borderSprite = border;
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
                return;
            }
            cardUI.Initialize(newCard);
            if (classBackgrounds.TryGetValue(newCard.cardClass.ToString(), out Sprite bgSprite))
            {
                cardUI.SetBackground(bgSprite);
            }

            if (rarityBorders.TryGetValue(newCard.rarity, out Sprite borderSprite))
            {
                cardUI.SetBorder(borderSprite);
            }
            if (newCard.leftEffects != null && newCard.leftEffects.Count > 0)
            {
                var leftEffect = newCard.leftEffects[0];
                if (effectIcons.TryGetValue(leftEffect.effectType, out Sprite leftIcon))
                {
                    cardUI.SetLeftEffect(leftEffect.value, leftIcon);
                }
            }

            if (newCard.rightEffects != null && newCard.rightEffects.Count > 0)
            {
                var rightEffect = newCard.rightEffects[0];
                if (effectIcons.TryGetValue(rightEffect.effectType, out Sprite rightIcon))
                {
                    cardUI.SetRightEffect(rightEffect.value, rightIcon);
                }
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

    private void SetEffectVisuals(CardUI cardUI, List<CardEffect> effects, bool isLeft)
    {
        if (effects == null || effects.Count == 0) return;

        CardEffect primaryEffect = effects[0];
        if (effectIcons.TryGetValue(primaryEffect.effectType, out Sprite icon))
        {
            if (isLeft)
            {
                cardUI.SetLeftEffect(primaryEffect.value, icon);
            }
            else
            {
                cardUI.SetRightEffect(primaryEffect.value, icon);
            }
        }
    }
}
#endif