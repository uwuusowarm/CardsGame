using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

[CustomEditor(typeof(_CardManager))]
public class _CardManagerEditor : Editor
{
    private _CardManager manager;
    private FieldInfo deckField, handField, discardField;
    private bool showDeck = true, showHand = true, showDiscard = true;

    private void OnEnable()
    {
        manager = (_CardManager)target;
        var fmlCissostupid = typeof(_CardManager);
        deckField = fmlCissostupid.GetField("deck", BindingFlags.NonPublic | BindingFlags.Instance);
        handField = fmlCissostupid.GetField("hand", BindingFlags.NonPublic | BindingFlags.Instance);
        discardField = fmlCissostupid.GetField("discard", BindingFlags.NonPublic | BindingFlags.Instance);
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Runtime Card Lists", EditorStyles.boldLabel);

        DrawList(deckField, ref showDeck, "Deck");
        DrawList(handField, ref showHand, "Hand");
        DrawList(discardField, ref showDiscard, "Ablagestapel");

        if (Application.isPlaying)
            Repaint();
    }

    private void DrawList(FieldInfo fi, ref bool foldout, string label)
    {
        var list = fi.GetValue(manager) as List<CardData>;
        int count = list != null ? list.Count : 0;
        foldout = EditorGUILayout.Foldout(foldout, $"{label} ({count})");

        if (foldout && list != null)
        {
            EditorGUI.indentLevel++;

            foreach (var c in list)
                EditorGUILayout.LabelField("• " + c.cardName);

            EditorGUI.indentLevel--;
        }
    }

    public override bool RequiresConstantRepaint()
    {
        return Application.isPlaying;
    }
}
