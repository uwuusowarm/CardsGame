using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

[CustomEditor(typeof(CardManager))]
public class CardManagerEditor : Editor
{
    private CardManager cardManager;

    private FieldInfo deckField;
    private FieldInfo handField;
    private FieldInfo discardField;
    private FieldInfo leftField;
    private FieldInfo rightField;

    private bool showDeck = true;
    private bool showHand = true;
    private bool showDiscard = true;
    private bool showLeft = true;
    private bool showRight = true;

    public override bool RequiresConstantRepaint() => Application.isPlaying;

    private void OnEnable()
    {
        var managerType = typeof(CardManager);
        cardManager = (CardManager)target;

        deckField = managerType.GetField("deck", BindingFlags.NonPublic | BindingFlags.Instance);
        handField = managerType.GetField("hand", BindingFlags.NonPublic | BindingFlags.Instance);
        discardField = managerType.GetField("discardPile", BindingFlags.NonPublic | BindingFlags.Instance);
        leftField = managerType.GetField("leftZone", BindingFlags.NonPublic | BindingFlags.Instance);
        rightField = managerType.GetField("rightZone", BindingFlags.NonPublic | BindingFlags.Instance);
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Zones", EditorStyles.boldLabel);

        DrawCardList(deckField, ref showDeck, "Deck");
        DrawCardList(handField, ref showHand, "Hand");
        DrawCardList(discardField, ref showDiscard, "Discard Pile");
        DrawCardList(leftField, ref showLeft, "Left Zone");
        DrawCardList(rightField, ref showRight, "Right Zone");

        if (Application.isPlaying)
            Repaint();
    }

    private void DrawCardList(FieldInfo field, ref bool foldout, string label)
    {
        var cards = field.GetValue(cardManager) as List<CardData>;
        int cardCount = cards?.Count ?? 0;

        foldout = EditorGUILayout.Foldout(foldout, $"{label} ({cardCount})");

        if (foldout && cards != null)
        {
            EditorGUI.indentLevel++;
            foreach (var card in cards)
            {
                EditorGUILayout.LabelField("• " + card.cardName);
            }
            EditorGUI.indentLevel--;
        }
    }
}
