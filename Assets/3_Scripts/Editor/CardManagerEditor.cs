using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

[CustomEditor(typeof(CardManager))]
public class CardManagerEditor : Editor
{
    private CardManager manager;
    private FieldInfo deckF;
    private FieldInfo discardF;
    private FieldInfo handF;
    private FieldInfo leftF;
    private FieldInfo rightF;

    private bool showDeck = true;
    private bool showHand = true;
    private bool showDiscard = true;
    private bool showLeft = true;
    private bool showRight = true;
    public override bool RequiresConstantRepaint() => Application.isPlaying;

    private void OnEnable()
    {
        var t = typeof(CardManager);
        manager = (CardManager)target;
        deckF = t.GetField("deck", BindingFlags.NonPublic | BindingFlags.Instance);
        handF = t.GetField("hand", BindingFlags.NonPublic | BindingFlags.Instance);
        discardF = t.GetField("discardPile", BindingFlags.NonPublic | BindingFlags.Instance);
        leftF = t.GetField("leftZone", BindingFlags.NonPublic | BindingFlags.Instance);
        rightF = t.GetField("rightZone", BindingFlags.NonPublic | BindingFlags.Instance);
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GUILayout.Space(8);
        EditorGUILayout.LabelField("Runtime Card Lists", EditorStyles.boldLabel);
        DrawList(deckF, ref showDeck, "Deck");
        DrawList(handF, ref showHand, "Hand");
        DrawList(discardF, ref showDiscard, "Discard Pile");
        DrawList(leftF, ref showLeft, "Left Zone");
        DrawList(rightF, ref showRight, "Right Zone");

        if (Application.isPlaying)
            Repaint();
    }

    private void DrawList(FieldInfo fi, ref bool fold, string label)
    {
        var list = fi.GetValue(manager) as List<CardData>;
        int count = list?.Count ?? 0;

        fold = EditorGUILayout.Foldout(fold, $"{label} ({count})");

        if (fold && list != null)
        {
            EditorGUI.indentLevel++;
            foreach (var c in list)
                EditorGUILayout.LabelField("• " + c.cardName);
            EditorGUI.indentLevel--;
        }
    }
}
