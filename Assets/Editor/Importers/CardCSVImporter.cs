using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Globalization;

public class CardCSVImporter
{
    private const string CSV_PATH_KEY = "CardCSVImporter_Path";
    private const string DEFAULT_ASSET_FOLDER = "Assets/1_Prefabs/Cards/IngameCards"; // Or wherever you want to save them
    private const string CARD_PREFAB_PATH = "Assets/3_Scripts/Gridsystem/Cards/Card.prefab";

    private static Dictionary<string, Sprite> classBackgrounds;
    private static Dictionary<CardRarity, Sprite> rarityBorders;
    private static Dictionary<CardEffect.EffectType, Sprite> effectIcons;
    private static GameObject cardPrefabTemplate;

    [MenuItem("Tools/Import Cards from CSV")]
    public static void ImportCards()
    {
        string lastPath = EditorPrefs.GetString(CSV_PATH_KEY, Application.dataPath);
        string path = EditorUtility.OpenFilePanel("Open Card CSV", lastPath, "csv");

        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        EditorPrefs.SetString(CSV_PATH_KEY, Path.GetDirectoryName(path));

        if (!LoadResources())
        {
            EditorUtility.DisplayDialog("Error", "Failed to load required card resources (prefabs, sprites). Check console for details.", "OK");
            return;
        }

        string[] lines = File.ReadAllLines(path);

        if (lines.Length <= 1)
        {
            EditorUtility.DisplayDialog("Error", "CSV file is empty or only has a header.", "OK");
            return;
        }

        if (!Directory.Exists(DEFAULT_ASSET_FOLDER))
        {
            Directory.CreateDirectory(DEFAULT_ASSET_FOLDER);
        }

        int itemsCreated = 0;
        int itemsUpdated = 0;

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line) || line.StartsWith(";;;"))
            {
                continue;
            }

            string[] values = line.Split(';');
            
            if (values.Length < 12)
            {
                Debug.LogWarning($"Skipping line {i + 1}: Not enough columns (found {values.Length}, expected at least 12). Line: '{line}'");
                continue;
            }
            
            if (values[3].Trim().Equals("Not Ready", System.StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            try
            {
                CardData card = null;
                string cardName = SanitizeFileName(values[1]);
                if (string.IsNullOrEmpty(cardName))
                {
                    cardName = $"Card_ID_{values[0]}";
                }
                
                string assetFileName = $"{values[0]}_{cardName}.asset";
                string assetPath = Path.Combine(DEFAULT_ASSET_FOLDER, assetFileName);

                card = AssetDatabase.LoadAssetAtPath<CardData>(assetPath);

                if (card == null)
                {
                    card = ScriptableObject.CreateInstance<CardData>();
                    PopulateCardData(card, values);
                    AssetDatabase.CreateAsset(card, assetPath);
                    itemsCreated++;
                }
                else
                {
                    PopulateCardData(card, values);
                    itemsUpdated++;
                }
                
                CreateOrUpdateCardPrefab(card, assetPath);

                EditorUtility.SetDirty(card);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error processing line {i + 1}: '{line}'. Exception: {ex.Message}\nStackTrace: {ex.StackTrace}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Import Complete",
            $"{itemsCreated} cards created.\n{itemsUpdated} cards updated.", "OK");
        Debug.Log($"Card import complete. Created: {itemsCreated}, Updated: {itemsUpdated}. Assets saved in {DEFAULT_ASSET_FOLDER}");
    }

    private static void PopulateCardData(CardData card, string[] values)
    {
        card.cardName = values[1];
        card.cardClass = ParseCardClass(values[2]);
        card.rarity = ParseCardRarity(values[3]);
        card.description = "";
        card.cardArt = CardAssetUpdater.FindCardArtSprite(card.cardName);

        
        card.leftEffects.Clear();
        card.rightEffects.Clear();
        card.alwaysEffects.Clear();

        var leftType = ParseEffectType(values[4]);
        if (leftType != CardEffect.EffectType.None)
        {
            card.leftEffects.Add(new CardEffect { effectType = leftType, value = ParseInt(values[5]) });
        }
        
        var rightType = ParseEffectType(values[6]);
        if (rightType != CardEffect.EffectType.None)
        {
            card.rightEffects.Add(new CardEffect { effectType = rightType, value = ParseInt(values[7]) });
        }

        var specialType = ParseEffectType(values[8]);
        if (specialType != CardEffect.EffectType.None)
        {
            card.alwaysEffects.Add(new CardEffect { effectType = specialType, value = ParseInt(values[9]) });
        }
        
        if (card.leftEffects.Count > 0 && effectIcons.TryGetValue(card.leftEffects[0].effectType, out Sprite leftIcon))
        {
            card.leftEffectIcon = leftIcon;
        }
        if (card.rightEffects.Count > 0 && effectIcons.TryGetValue(card.rightEffects[0].effectType, out Sprite rightIcon))
        {
            card.rightEffectIcon = rightIcon;
        }

        if (classBackgrounds.TryGetValue(card.cardClass.ToString(), out Sprite bgSprite))
        {
            card.backgroundSprite = bgSprite;
        }
        else
        {
            classBackgrounds.TryGetValue("Base", out card.backgroundSprite);
        }

        if (rarityBorders.TryGetValue(card.rarity, out Sprite border))
        {
            card.borderSprite = border;
        }
    }

    private static void CreateOrUpdateCardPrefab(CardData cardData, string cardDataPath)
    {
        string prefabPath = Path.ChangeExtension(cardDataPath, ".prefab");
        GameObject cardInstance = null;

        try
        {
            cardInstance = (GameObject)PrefabUtility.InstantiatePrefab(cardPrefabTemplate);
            if(cardInstance == null) {
                Debug.LogError("Failed to instantiate card prefab template.");
                return;
            }

            CardUI cardUI = cardInstance.GetComponent<CardUI>();
            if (cardUI == null)
            {
                Debug.LogError("Card template is missing the CardUI component!");
                return;
            }

            cardUI.Initialize(cardData);
            
            PrefabUtility.SaveAsPrefabAsset(cardInstance, prefabPath);
        }
        finally
        {
            if (cardInstance != null)
            {
                Object.DestroyImmediate(cardInstance);
            }
        }
    }
    
    private static int ParseInt(string value, int defaultValue = 0)
    {
        if (string.IsNullOrWhiteSpace(value)) return defaultValue;
        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result))
        {
            return result;
        }
        return defaultValue;
    }

    private static CardClass ParseCardClass(string value)
    {
        value = value.Trim();
        if (string.IsNullOrWhiteSpace(value)) return CardClass.Base;

        if (value.Equals("Mage", System.StringComparison.OrdinalIgnoreCase))
        {
            return CardClass.Wizard;
        }

        try
        {
            return (CardClass)System.Enum.Parse(typeof(CardClass), value, true);
        }
        catch { return CardClass.Base; }
    }

    private static CardRarity ParseCardRarity(string value)
    {
        value = value.Trim();
        if (string.IsNullOrWhiteSpace(value)) return CardRarity.Common;
        try
        {
            return (CardRarity)System.Enum.Parse(typeof(CardRarity), value, true);
        }
        catch { return CardRarity.Common; }
    }

    private static CardEffect.EffectType ParseEffectType(string value)
    {
        value = value.Trim().Replace(" ", "");
        if (string.IsNullOrWhiteSpace(value) || value.Equals("none", System.StringComparison.OrdinalIgnoreCase))
        {
            return CardEffect.EffectType.None;
        }
        
        if (value.Equals("Defense", System.StringComparison.OrdinalIgnoreCase)) return CardEffect.EffectType.Block;
        if (value.Equals("Movement", System.StringComparison.OrdinalIgnoreCase)) return CardEffect.EffectType.Move;

        try
        {
            return (CardEffect.EffectType)System.Enum.Parse(typeof(CardEffect.EffectType), value, true);
        }
        catch 
        { 
            Debug.LogWarning($"Could not parse EffectType: '{value}'. Returning None.");
            return CardEffect.EffectType.None; 
        }
    }
    
    private static bool LoadResources()
    {
        cardPrefabTemplate = AssetDatabase.LoadAssetAtPath<GameObject>(CARD_PREFAB_PATH);

        string bgPath = "Assets/2_Art/Art Complete/2D_Card_Art/Cards_Image_2D/Card Layouts";
        classBackgrounds = new Dictionary<string, Sprite>()
        {
            { "Wizard", AssetDatabase.LoadAssetAtPath<Sprite>($"{bgPath}/Card_Layout_Wizard_v1_NC.png") },
            { "Warrior", AssetDatabase.LoadAssetAtPath<Sprite>($"{bgPath}/Card_Layout_Warrior_v1_NC.png") },
            { "Rogue", AssetDatabase.LoadAssetAtPath<Sprite>($"{bgPath}/Card_Layout_Rogue_v1_NC.png") },
            { "Monster", AssetDatabase.LoadAssetAtPath<Sprite>($"{bgPath}/Card_Layout_Monster_v1_NC.png") },
            { "Base", AssetDatabase.LoadAssetAtPath<Sprite>($"{bgPath}/Card_Layout_Base_v1_NC.png") }
        };

        string rarityPath = "Assets/2_Art/Art Complete/2D_Card_Art/Cards_Image_2D/Rarity Layer";
        rarityBorders = new Dictionary<CardRarity, Sprite>()
        {
            { CardRarity.Common, AssetDatabase.LoadAssetAtPath<Sprite>($"{rarityPath}/Card_Rarity_Common_v1_NC.png") },
            { CardRarity.Rare, AssetDatabase.LoadAssetAtPath<Sprite>($"{rarityPath}/Card_Rarity_Rare_v1_NC.png") },
            { CardRarity.Legendary, AssetDatabase.LoadAssetAtPath<Sprite>($"{rarityPath}/Card_Rarity_Legendary_v1_NC.png") },
            { CardRarity.Monster, AssetDatabase.LoadAssetAtPath<Sprite>($"{rarityPath}/Card_Rarity_Monster_v1_NC.png") }
        };

        string iconsPath = "Assets/2_Art/Art Complete/2D_Card_Art/Cards_Image_2D/Card Icons";
        effectIcons = new Dictionary<CardEffect.EffectType, Sprite>()
        {
            { CardEffect.EffectType.Attack, AssetDatabase.LoadAssetAtPath<Sprite>($"{iconsPath}/Card_Icon_Core_Attack_v1_NC.png") },
            { CardEffect.EffectType.Block, AssetDatabase.LoadAssetAtPath<Sprite>($"{iconsPath}/Card_Icon_Core_Defense_v1_NC.png") },
            { CardEffect.EffectType.Heal, AssetDatabase.LoadAssetAtPath<Sprite>($"{iconsPath}/Card_Icon_Core_Heal_v1_NC.png") },
            { CardEffect.EffectType.Move, AssetDatabase.LoadAssetAtPath<Sprite>($"{iconsPath}/Card_Icon_Core_Movement_v1_NC.png") }
        };

        if (cardPrefabTemplate == null) { Debug.LogError("Card Prefab Template not found at: " + CARD_PREFAB_PATH); return false; }
        
        return true;
    }

    private static string SanitizeFileName(string name)
    {
        if (string.IsNullOrEmpty(name)) return "UnnamedCard";
        string invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(Path.GetInvalidFileNameChars()));
        string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);
        return System.Text.RegularExpressions.Regex.Replace(name, invalidRegStr, "_");
    }
}
