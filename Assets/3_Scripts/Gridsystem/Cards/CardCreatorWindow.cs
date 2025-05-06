#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class CardCreatorWindow : EditorWindow
{
    private CardData newCard;
    private Vector2 scrollPos;
    private Texture2D cardTexture;

    [MenuItem("Tools/Card Creator")]
    public static void ShowWindow()
    {
        GetWindow<CardCreatorWindow>("Card Creator");
    }

    private void OnEnable()
    {
        newCard = CreateInstance<CardData>();
    }

    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        // Basis-Informationen
        EditorGUILayout.LabelField("Card Basics", EditorStyles.boldLabel);
        newCard.cardName = EditorGUILayout.TextField("Name", newCard.cardName);
        newCard.manaCost = EditorGUILayout.IntField("Mana Cost", newCard.manaCost);
        newCard.description = EditorGUILayout.TextArea(newCard.description, GUILayout.Height(50));
        newCard.cardArt = (Sprite)EditorGUILayout.ObjectField("Art", newCard.cardArt, typeof(Sprite), false);

        // Effekt-Sektionen
        DrawEffectSection("Left Swipe Effects", newCard.leftEffects);
        DrawEffectSection("Right Swipe Effects", newCard.rightEffects);
        DrawEffectSection("Always Active Effects", newCard.alwaysEffects);

        // Speicher-Button
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
            effects[i].isTemporary = EditorGUILayout.Toggle("Is Temporary", effects[i].isTemporary);

            if (effects[i].isTemporary)
            {
                effects[i].duration = EditorGUILayout.IntField("Duration (Turns)", effects[i].duration);
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

        if (!string.IsNullOrEmpty(path))
        {
            AssetDatabase.CreateAsset(newCard, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = newCard;
        }
    }
}
#endif