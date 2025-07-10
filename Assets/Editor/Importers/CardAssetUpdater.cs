using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public static class CardAssetUpdater
{
    private const string CARD_ART_FOLDER_PATH = "Assets/Art Complete/UI/2D_Card_Art/Cards_Image_2D/Card Images/";

    public static Sprite FindCardArtSprite(string cardName)
    {
        if (string.IsNullOrEmpty(cardName)) return null;

        string searchName = cardName.Replace(' ', '_');

        string[] guids = AssetDatabase.FindAssets($"t:Sprite {searchName}", new[] { CARD_ART_FOLDER_PATH });

        if (guids.Length == 0)
        {
            return null;
        }

        if (guids.Length > 1)
        {
            Debug.LogWarning($"Found multiple sprites for '{cardName}' in {CARD_ART_FOLDER_PATH}. Using the first one found.");
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
    
    public static void UpdateCardDataResources(CardData cardData)
    {
        if (cardData == null) return;

        cardData.cardArt = FindCardArtSprite(cardData.cardName);

        string bgPath = "Assets/Art Complete/UI/2D_Card_Art/Cards_Image_2D/Card Layouts";
        var classBackgrounds = new Dictionary<string, Sprite>()
        {
            { "Wizard", AssetDatabase.LoadAssetAtPath<Sprite>($"{bgPath}/Card_Layout_Wizard_v1_NC.png") },
            { "Warrior", AssetDatabase.LoadAssetAtPath<Sprite>($"{bgPath}/Card_Layout_Warrior_v1_NC.png") },
            { "Rogue", AssetDatabase.LoadAssetAtPath<Sprite>($"{bgPath}/Card_Layout_Rogue_v1_NC.png") },
            { "Monster", AssetDatabase.LoadAssetAtPath<Sprite>($"{bgPath}/Card_Layout_Monster_v1_NC.png") },
            { "Base", AssetDatabase.LoadAssetAtPath<Sprite>($"{bgPath}/Card_Layout_Base_v1_NC.png") }
        };
        
        if (classBackgrounds.TryGetValue(cardData.cardClass.ToString(), out Sprite bgSprite))
        {
            cardData.backgroundSprite = bgSprite;
        }

        string rarityPath = "Assets/UI/Art Complete/UI/2D_Card_Art/Cards_Image_2D/Rarity Layer";
        var rarityBorders = new Dictionary<CardRarity, Sprite>()
        {
            { CardRarity.Common, AssetDatabase.LoadAssetAtPath<Sprite>($"{rarityPath}/Card_Rarity_Common_v1_NC.png") },
            { CardRarity.Rare, AssetDatabase.LoadAssetAtPath<Sprite>($"{rarityPath}/Card_Rarity_Rare_v1_NC.png") },
            { CardRarity.Legendary, AssetDatabase.LoadAssetAtPath<Sprite>($"{rarityPath}/Card_Rarity_Legendary_v1_NC.png") },
            { CardRarity.Monster, AssetDatabase.LoadAssetAtPath<Sprite>($"{rarityPath}/Card_Rarity_Monster_v1_NC.png") }
        };
        
        if (rarityBorders.TryGetValue(cardData.rarity, out Sprite border))
        {
            cardData.borderSprite = border;
        }
        
        string iconsPath = "Assets/Art Complete/UI/2D_Card_Art/Cards_Image_2D/Card Icons";
        var effectIcons = new Dictionary<CardEffect.EffectType, Sprite>()
        {
            { CardEffect.EffectType.Attack, AssetDatabase.LoadAssetAtPath<Sprite>($"{iconsPath}/Card_Icon_Core_Attack_v1_NC.png") },
            { CardEffect.EffectType.Block, AssetDatabase.LoadAssetAtPath<Sprite>($"{iconsPath}/Card_Icon_Core_Defense_v1_NC.png") },
            { CardEffect.EffectType.Heal, AssetDatabase.LoadAssetAtPath<Sprite>($"{iconsPath}/Card_Icon_Core_Heal_v1_NC.png") },
            { CardEffect.EffectType.Move, AssetDatabase.LoadAssetAtPath<Sprite>($"{iconsPath}/Card_Icon_Core_Movement_v1_NC.png") }
        };

        cardData.leftEffectIcon = (cardData.leftEffects.Count > 0 && effectIcons.TryGetValue(cardData.leftEffects[0].effectType, out Sprite lIcon)) ? lIcon : null;
        cardData.rightEffectIcon = (cardData.rightEffects.Count > 0 && effectIcons.TryGetValue(cardData.rightEffects[0].effectType, out Sprite rIcon)) ? rIcon : null;
    }
    
    public static void UpdateAssociatedPrefab(CardData cardData)
    {
        if (cardData == null) return;

        string assetPath = AssetDatabase.GetAssetPath(cardData);
        if (string.IsNullOrEmpty(assetPath)) return;

        string prefabPath = Path.ChangeExtension(assetPath, ".prefab");
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if (prefab == null) return;

        CardUI cardUI = prefab.GetComponent<CardUI>();
        if (cardUI == null)
        {
            Debug.LogError($"Prefab for {cardData.name} is missing a CardUI component.", prefab);
            return;
        }

        cardUI.Initialize(cardData);
        EditorUtility.SetDirty(prefab); 
        Debug.Log($"Updated prefab '{prefab.name}' with data from '{cardData.name}'.", prefab);
    }
}