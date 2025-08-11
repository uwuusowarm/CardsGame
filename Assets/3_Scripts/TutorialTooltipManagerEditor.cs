
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TutorialTooltipManager))]
public class TutorialTooltipManagerEditor : Editor
{
    private SerializedProperty tooltipsProp;
    private SerializedProperty tooltipCanvasProp;
    private SerializedProperty tooltipPrefabProp;
    private SerializedProperty skipButtonProp;
    private SerializedProperty startAutomaticallyProp;
    private SerializedProperty delayBetweenTooltipsProp;
    private SerializedProperty pauseGameProp;
    
    void OnEnable()
    {
        tooltipsProp = serializedObject.FindProperty("tooltips");
        tooltipCanvasProp = serializedObject.FindProperty("tooltipCanvas");
        tooltipPrefabProp = serializedObject.FindProperty("tooltipPrefab");
        skipButtonProp = serializedObject.FindProperty("skipTutorialButton");
        startAutomaticallyProp = serializedObject.FindProperty("startAutomatically");
        delayBetweenTooltipsProp = serializedObject.FindProperty("delayBetweenTooltips");
        pauseGameProp = serializedObject.FindProperty("pauseGameDuringTutorial");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        TutorialTooltipManager manager = (TutorialTooltipManager)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Tutorial Tooltip Manager", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.LabelField("Setup", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(tooltipCanvasProp);
        EditorGUILayout.PropertyField(tooltipPrefabProp);
        EditorGUILayout.PropertyField(skipButtonProp);
        
        EditorGUILayout.Space();
        
        EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(startAutomaticallyProp);
        EditorGUILayout.PropertyField(delayBetweenTooltipsProp);
        EditorGUILayout.PropertyField(pauseGameProp);
        
        EditorGUILayout.Space();
        
        EditorGUILayout.LabelField($"Tooltips ({tooltipsProp.arraySize})", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Add New Tooltip"))
        {
            tooltipsProp.arraySize++;
            SerializedProperty newTooltip = tooltipsProp.GetArrayElementAtIndex(tooltipsProp.arraySize - 1);
            newTooltip.FindPropertyRelative("title").stringValue = "New Tooltip";
            newTooltip.FindPropertyRelative("description").stringValue = "Enter tooltip description here...";
            newTooltip.FindPropertyRelative("backgroundColor").colorValue = Color.white;
            newTooltip.FindPropertyRelative("size").vector2Value = new Vector2(300, 150);
            newTooltip.FindPropertyRelative("fadeInDuration").floatValue = 0.3f;
            newTooltip.FindPropertyRelative("fadeOutDuration").floatValue = 0.2f;
            newTooltip.FindPropertyRelative("highlightTarget").boolValue = true;
            newTooltip.FindPropertyRelative("targetOffset").vector2Value = new Vector2(100, 50);
            newTooltip.FindPropertyRelative("highlightColor").colorValue = Color.yellow;
        }
        
        for (int i = 0; i < tooltipsProp.arraySize; i++)
        {
            SerializedProperty tooltip = tooltipsProp.GetArrayElementAtIndex(i);
            
            EditorGUILayout.BeginVertical("box");
            
            EditorGUILayout.BeginHorizontal();
            string title = tooltip.FindPropertyRelative("title").stringValue;
            if (string.IsNullOrEmpty(title)) title = $"Tooltip {i + 1}";
            
            tooltip.isExpanded = EditorGUILayout.Foldout(tooltip.isExpanded, $"{i + 1}. {title}");
            
            if (GUILayout.Button("×", GUILayout.Width(20)))
            {
                tooltipsProp.DeleteArrayElementAtIndex(i);
                break;
            }
            EditorGUILayout.EndHorizontal();
            
            if (tooltip.isExpanded)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.LabelField("Content", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(tooltip.FindPropertyRelative("title"));
                EditorGUILayout.PropertyField(tooltip.FindPropertyRelative("description"));
                
                EditorGUILayout.Space();
                
                EditorGUILayout.LabelField("Visual Settings", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(tooltip.FindPropertyRelative("backgroundSprite"));
                EditorGUILayout.PropertyField(tooltip.FindPropertyRelative("backgroundColor"));
                EditorGUILayout.PropertyField(tooltip.FindPropertyRelative("position"));
                EditorGUILayout.PropertyField(tooltip.FindPropertyRelative("size"));
                
                EditorGUILayout.Space();
                
                EditorGUILayout.LabelField("Target Settings", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(tooltip.FindPropertyRelative("targetObject"));
                EditorGUILayout.PropertyField(tooltip.FindPropertyRelative("highlightTarget"));
                EditorGUILayout.PropertyField(tooltip.FindPropertyRelative("targetOffset"));
                
                EditorGUILayout.Space();
                
                EditorGUILayout.LabelField("Highlight Settings", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(tooltip.FindPropertyRelative("customHighlightMaterial"));
                EditorGUILayout.PropertyField(tooltip.FindPropertyRelative("highlightColor"));
                
                EditorGUILayout.Space();
                
                EditorGUILayout.LabelField("Animation", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(tooltip.FindPropertyRelative("fadeInDuration"));
                EditorGUILayout.PropertyField(tooltip.FindPropertyRelative("fadeOutDuration"));
                
                EditorGUILayout.Space();
                
                EditorGUILayout.LabelField("Interaction", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(tooltip.FindPropertyRelative("waitForSpecificClick"));
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
        
        if (Application.isPlaying)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Runtime Controls", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Start Tutorial"))
            {
                manager.StartTutorial();
            }
            if (GUILayout.Button("Skip Tutorial"))
            {
                manager.SkipTutorial();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.LabelField($"Tutorial Active: {manager.IsTutorialActive()}");
            EditorGUILayout.LabelField($"Current Tooltip: {manager.GetCurrentTooltipIndex() + 1}/{manager.GetTotalTooltipCount()}");
        }
        
        serializedObject.ApplyModifiedProperties();
    }
}
#endif