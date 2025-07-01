using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CardData))]
public class CardDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        base.OnInspectorGUI();

        if (EditorGUI.EndChangeCheck())
        {
            var cardData = (CardData)target;

            serializedObject.ApplyModifiedProperties();

            CardAssetUpdater.UpdateCardDataResources(cardData);

            CardAssetUpdater.UpdateAssociatedPrefab(cardData);
            
            EditorUtility.SetDirty(cardData);
        }
    }
}
