using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

[CustomEditor(typeof(CardManager))]
public class CardManagerEditor : Editor
{
    private bool showDeckList = true;
    private bool showHandList = true;
    private bool showDiscardPileList = true;
    private bool showLeftZoneList = true;
    private bool showRightZoneList = true;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CardManager cardManager = (CardManager)target;
        var managerType = typeof(CardManager);

        FieldInfo deckField = managerType.GetField("deck", BindingFlags.NonPublic | BindingFlags.Instance);
        if (deckField != null)
        {
            List<CardData> deckCardList = (List<CardData>)deckField.GetValue(cardManager);
            if (deckCardList != null)
            {
                showDeckList = EditorGUILayout.Foldout(showDeckList, "Deck (" + deckCardList.Count + ")");
                if (showDeckList)
                {
                    EditorGUI.indentLevel = EditorGUI.indentLevel + 1;
                    foreach (CardData card in deckCardList)
                    {
                        EditorGUILayout.LabelField("• " + card.cardName);
                    }
                    EditorGUI.indentLevel = EditorGUI.indentLevel - 1;
                }
            }
        }

        FieldInfo handField = managerType.GetField("hand", BindingFlags.NonPublic | BindingFlags.Instance);
        if (handField != null)
        {
            List<CardData> handCardList = (List<CardData>)handField.GetValue(cardManager);
            if (handCardList != null)
            {
                showHandList = EditorGUILayout.Foldout(showHandList, "Hand (" + handCardList.Count + ")");
                if (showHandList)
                {
                    EditorGUI.indentLevel = EditorGUI.indentLevel + 1;
                    foreach (CardData card in handCardList)
                    {
                        EditorGUILayout.LabelField("• " + card.cardName);
                    }
                    EditorGUI.indentLevel = EditorGUI.indentLevel - 1;
                }
            }
        }

        FieldInfo discardPileField = managerType.GetField("discardPile", BindingFlags.NonPublic | BindingFlags.Instance);
        if (discardPileField != null)
        {
            List<CardData> discardPileCardList = (List<CardData>)discardPileField.GetValue(cardManager);
            if (discardPileCardList != null)
            {
                showDiscardPileList = EditorGUILayout.Foldout(showDiscardPileList, "Discard Pile (" + discardPileCardList.Count + ")");
                if (showDiscardPileList)
                {
                    EditorGUI.indentLevel = EditorGUI.indentLevel + 1;
                    foreach (CardData card in discardPileCardList)
                    {
                        EditorGUILayout.LabelField("• " + card.cardName);
                    }
                    EditorGUI.indentLevel = EditorGUI.indentLevel - 1;
                }
            }
        }

        FieldInfo leftZoneField = managerType.GetField("leftZone", BindingFlags.NonPublic | BindingFlags.Instance);
        if (leftZoneField != null)
        {
            List<CardData> leftZoneCardList = (List<CardData>)leftZoneField.GetValue(cardManager);
            if (leftZoneCardList != null)
            {
                showLeftZoneList = EditorGUILayout.Foldout(showLeftZoneList, "Left Zone (" + leftZoneCardList.Count + ")");
                if (showLeftZoneList)
                {
                    EditorGUI.indentLevel = EditorGUI.indentLevel + 1;
                    foreach (CardData card in leftZoneCardList)
                    {
                        EditorGUILayout.LabelField("• " + card.cardName);
                    }
                    EditorGUI.indentLevel = EditorGUI.indentLevel - 1;
                }
            }
        }

        FieldInfo rightZoneField = managerType.GetField("rightZone", BindingFlags.NonPublic | BindingFlags.Instance);
        if (rightZoneField != null)
        {
            List<CardData> rightZoneCardList = (List<CardData>)rightZoneField.GetValue(cardManager);
            if (rightZoneCardList != null)
            {
                showRightZoneList = EditorGUILayout.Foldout(showRightZoneList, "Right Zone (" + rightZoneCardList.Count + ")");
                if (showRightZoneList)
                {
                    EditorGUI.indentLevel = EditorGUI.indentLevel + 1;
                    foreach (CardData card in rightZoneCardList)
                    {
                        EditorGUILayout.LabelField("• " + card.cardName);
                    }
                    EditorGUI.indentLevel = EditorGUI.indentLevel - 1;
                }
            }
        }

        if (Application.isPlaying)
        {
            Repaint();
        }
    }
}