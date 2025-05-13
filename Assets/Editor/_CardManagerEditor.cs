using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

[CustomEditor(typeof(_CardManager))]
public class _CardManagerEditor : Editor
{
    private _CardManager manager;
    private FieldInfo deckField, handField, discardField;
    private bool showDeck, showHand, showDiscard;

    private void OnEnable()
    {
        manager = (_CardManager)target;

        var t = typeof(_CardManager);
        deckField = t.GetField("deck", BindingFlags.NonPublic | BindingFlags.Instance);
        handField = t.GetField("hand", BindingFlags.NonPublic | BindingFlags.Instance);
        discardField = t.GetField("discard", BindingFlags.NonPublic | BindingFlags.Instance);
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("▶ Runtime Card Lists", EditorStyles.boldLabel);

        var deckList = deckField?.GetValue(manager) as List<CardData>;

        int deckCount = deckList != null ? deckList.Count : 0;

        showDeck = EditorGUILayout.Foldout(showDeck, $"Deck ({deckCount})");
        if (showDeck && deckList != null)
        {
            EditorGUI.indentLevel++;
            foreach (var c in deckList)
                EditorGUILayout.LabelField("• " + c.cardName);
            EditorGUI.indentLevel--;
        }

        var handList = handField?.GetValue(manager) as List<CardData>;

        int handCount = handList != null ? handList.Count : 0;

        showHand = EditorGUILayout.Foldout(showHand, $"Hand ({handCount})");
        if (showHand && handList != null)
        {
            EditorGUI.indentLevel++;
            foreach (var c in handList)
                EditorGUILayout.LabelField("• " + c.cardName);
            EditorGUI.indentLevel--;
        }

        var discardList = discardField?.GetValue(manager) as List<CardData>;

        int discardCount = discardList != null ? discardList.Count : 0;

        showDiscard = EditorGUILayout.Foldout(showDiscard, $"Ablagestapel ({discardCount})");
        if (showDiscard && discardList != null)
        {
            EditorGUI.indentLevel++;
            foreach (var c in discardList)
                EditorGUILayout.LabelField("• " + c.cardName);
            EditorGUI.indentLevel--;
        }
    }
}

