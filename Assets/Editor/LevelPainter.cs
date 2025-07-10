using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class HexPrefabMapping
{
    public string name;
    public GameObject prefab;
    public bool isDefault;
}

public class LevelPainter : EditorWindow
{
    [SerializeField] private LevelPainterSettings settings;
    [SerializeField] private Transform gridParent;
    [SerializeField] private HexGrid hexGridComponent;
    
    private int gridWidth = 20;
    private int gridHeight = 20;
    private float heightStep = 0.5f;
    private int currentRoomID = 1;
    private enum BrushType { Hex, PlaceableObject }
    private BrushType selectedBrushType = BrushType.Hex;
    private int selectedBrushIndex = 0;
    private ToolMode currentTool = ToolMode.Paint;
    private enum ToolMode { Paint, Erase, Pick, Select }
    private string[] toolNames = { "Paint", "Erase", "Pick", "Select" };
    private GameObject selectedHex;
    private Vector2 scrollPos;
    private SerializedObject serializedWindowObject;
    private SerializedObject serializedSettingsObject;
    private SerializedProperty serializedGridParent;
    private SerializedProperty serializedPrefabs;
    private SerializedProperty serializedPlaceableObjects;

    [MenuItem("Tools/Level Painter")]
    public static void ShowWindow() => GetWindow<LevelPainter>("Level Painter");

    private void OnEnable()
    {
        serializedWindowObject = new SerializedObject(this);
        serializedGridParent = serializedWindowObject.FindProperty("gridParent");
        LoadSettings();
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable() => SceneView.duringSceneGui -= OnSceneGUI;

    
    private void OnGUI()
{
    serializedWindowObject.Update();
    if (serializedSettingsObject != null) serializedSettingsObject.Update();

    scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

    EditorGUILayout.LabelField("Level Painter", EditorStyles.boldLabel);
    settings = (LevelPainterSettings)EditorGUILayout.ObjectField("Settings Asset", settings, typeof(LevelPainterSettings), false);
    EditorGUILayout.PropertyField(serializedGridParent, new GUIContent("Ground Object (Hex Parent)"));
    hexGridComponent = (HexGrid)EditorGUILayout.ObjectField("HexGrid Component", hexGridComponent, typeof(HexGrid), true);

    if (settings == null)
    {
        EditorGUILayout.HelpBox("Keine Level Painter Settings gefunden. Eine neue Datei wird unter 'Assets/Editor/LevelPainterSettings.asset' erstellt, sobald Sie Prefabs hinzufügen.", MessageType.Info);
        LoadSettings(); 
    }

    if (settings == null || serializedSettingsObject == null)
    {
        EditorGUILayout.EndScrollView(); 
        serializedWindowObject.ApplyModifiedProperties();
        return;
    }

    EditorGUILayout.PropertyField(serializedPrefabs, true);
    EditorGUILayout.Space();
    EditorGUILayout.PropertyField(serializedPlaceableObjects, true);
    
    EditorGUILayout.Space();
    if (hexGridComponent != null)
    {
        if (GUILayout.Button("Calculate Size From Default Prefab"))
        {
            CalculateAndSetHexSize();
        }
    }
    
    EditorGUILayout.Space();
    EditorGUILayout.LabelField("Base Grid Generation", EditorStyles.boldLabel);
    gridWidth = EditorGUILayout.IntField("Grid Width", gridWidth);
    gridHeight = EditorGUILayout.IntField("Grid Height", gridHeight);

    if (GUILayout.Button("Generate Base Grid"))
    {
        if (CheckPrerequisites()) GenerateGrid();
    }

    if (GUILayout.Button("Clear Entire Grid"))
    {
        if (CheckPrerequisites() && EditorUtility.DisplayDialog("Clear Grid?", "Are you sure you want to delete all child objects of the grid parent?", "Yes", "No"))
        {
            ClearGrid();
        }
    }

    EditorGUILayout.Space();
    EditorGUILayout.LabelField("Room Tagging", EditorStyles.boldLabel);
    EditorGUILayout.LabelField("Next Room ID to be assigned: " + currentRoomID);
    if (GUILayout.Button("Reset Room Counter to 1"))
    {
        currentRoomID = 1;
    }
    
    EditorGUILayout.Space();
    EditorGUILayout.LabelField("Painting Tools", EditorStyles.boldLabel);
    
    EditorGUI.BeginChangeCheck();
    currentTool = (ToolMode)GUILayout.Toolbar((int)currentTool, toolNames);
    if (EditorGUI.EndChangeCheck() && currentTool != ToolMode.Select)
    {
        selectedHex = null;
    }
    
    EditorGUILayout.Space();
    EditorGUILayout.LabelField("Brushes", EditorStyles.boldLabel);
    DrawBrushSelectionUI();

    EditorGUILayout.EndScrollView();

    serializedWindowObject.ApplyModifiedProperties();
    if (serializedSettingsObject != null) serializedSettingsObject.ApplyModifiedProperties();
}

    
    private void LoadSettings()
    {
        if (settings != null)
        {
            serializedSettingsObject = new SerializedObject(settings);
            serializedPrefabs = serializedSettingsObject.FindProperty("hexPrefabs");
            serializedPlaceableObjects = serializedSettingsObject.FindProperty("placeableObjects");
            return;
        }

        var guids = AssetDatabase.FindAssets("t:LevelPainterSettings");
        if (guids.Length > 0)
        {
            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            settings = AssetDatabase.LoadAssetAtPath<LevelPainterSettings>(path);
        }

        if (settings != null)
        {
            serializedSettingsObject = new SerializedObject(settings);
            serializedPrefabs = serializedSettingsObject.FindProperty("hexPrefabs");
            serializedPlaceableObjects = serializedSettingsObject.FindProperty("placeableObjects");
        }
    }
    
    private void OnSceneGUI(SceneView sceneView)
    {
        if (!CheckPrerequisites()) return;

        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        var groundPlane = new Plane(Vector3.up, gridParent.position);

        if (groundPlane.Raycast(ray, out var enter))
        {
            var worldPosition = ray.GetPoint(enter);
            var hexCoords = WorldPositionToCoords(worldPosition, hexGridComponent);
            HandleTool(Event.current, hexCoords, hexGridComponent);
        }
        
        if (selectedHex != null)
        {
            DrawSelectionHighlight();
            DrawInteractionWindow();
        }
        
        sceneView.Repaint();
    }
    
    private void HandleTool(Event e, Vector3Int hexCoords, HexGrid grid)
    {
        var worldPositionOnGrid = CoordsToWorldPosition(hexCoords, grid);

        switch (currentTool)
        {
            case ToolMode.Paint:
                if (selectedBrushType == BrushType.Hex)
                {
                    Handles.color = Color.green;
                    Handles.DrawWireDisc(worldPositionOnGrid, Vector3.up, grid.hexWidth / 2f);
                }
                else 
                {
                    var targetHexObject = FindHexAt(hexCoords, grid);
                    if (targetHexObject != null)
                    {
                        var targetHexPosition = targetHexObject.transform.position;
                        Handles.color = Color.green;
                        Handles.DrawWireDisc(targetHexPosition, Vector3.up, grid.hexWidth / 2f);
                        var objectToPlace = settings.placeableObjects[selectedBrushIndex];
                        var finalObjectPosition = targetHexPosition + new Vector3(0, objectToPlace.yOffset, 0);
                        Handles.DrawWireCube(finalObjectPosition, Vector3.one * 0.5f); 
                        Handles.DrawLine(targetHexPosition, finalObjectPosition); 
                    }
                    else
                    {
                        Handles.color = Color.red;
                        Handles.DrawWireDisc(worldPositionOnGrid, Vector3.up, grid.hexWidth / 2f);
                    }
                }
                break;

            case ToolMode.Erase:
                Handles.color = Color.red;
                Handles.DrawWireDisc(worldPositionOnGrid, Vector3.up, grid.hexWidth / 2f);
                break;

            case ToolMode.Pick:
                Handles.color = Color.cyan;
                Handles.DrawWireDisc(worldPositionOnGrid, Vector3.up, grid.hexWidth / 2f);
                break;

            case ToolMode.Select:
                var potentialHex = FindHexAt(hexCoords, grid);
                if(potentialHex != null)
                {
                    Handles.color = Color.yellow;
                    Handles.DrawWireDisc(potentialHex.transform.position, Vector3.up, grid.hexWidth / 2f);
                }
                break;
        }

        if (e.type != EventType.MouseDown || e.button != 0) return;
        e.Use();
        switch (currentTool)
        {
            case ToolMode.Paint:
                switch (selectedBrushType)
                {
                    case BrushType.Hex:
                        PaintHex(hexCoords, worldPositionOnGrid);
                        break;
                    case BrushType.PlaceableObject:
                        PaintObject(hexCoords, grid);
                        break;
                }
                break;
            case ToolMode.Erase:
                EraseHex(hexCoords, grid);
                break;
            case ToolMode.Pick:
                PickHex(hexCoords, grid);
                break;
            case ToolMode.Select:
                SelectHex(hexCoords, grid);
                break;
        }
    }

    private void SelectHex(Vector3Int coords, HexGrid grid)
    {
        selectedHex = FindHexAt(coords, grid);
    }
    
    private void DrawSelectionHighlight()
    {
        if (selectedHex == null) return;
        Handles.color = Color.yellow;
        Handles.DrawWireCube(selectedHex.transform.position, selectedHex.GetComponentInChildren<MeshRenderer>().bounds.size * 1.05f);
    }
    
    private void DrawInteractionWindow()
    {
        Handles.BeginGUI();
        var windowRect = new Rect(20, 40, 250, 0);
        GUI.skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene);
        windowRect = GUILayout.Window(0, windowRect, InteractionWindowFunction, $"Hexagon: {selectedHex.name}");
        Handles.EndGUI();
    }

    private void InteractionWindowFunction(int windowID)
    {
        var hexComponent = selectedHex.GetComponent<Hex>();
        if (hexComponent == null) return;

        EditorGUILayout.LabelField("Height Adjustment", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Height +1")) ChangeHexHeight(heightStep);
        if (GUILayout.Button("Height -1")) ChangeHexHeight(-heightStep);
        EditorGUILayout.EndHorizontal();
        heightStep = EditorGUILayout.FloatField("Height Step", heightStep);
        
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Room Definition", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        var newRoomID = EditorGUILayout.IntField("Room ID", hexComponent.RoomID);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(hexComponent, "Change Room ID");
            hexComponent.RoomID = newRoomID;
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Place Object", EditorStyles.boldLabel);
        if (hexComponent.PlacedObject != null)
        {
             EditorGUILayout.HelpBox($"Object '{hexComponent.PlacedObject.name}' is on this hex.", MessageType.Info);
             EditorGUILayout.LabelField("Adjust Placed Object", EditorStyles.boldLabel);
             EditorGUILayout.BeginHorizontal();
             if (GUILayout.Button("Height +1")) ChangePlacedObjectHeight(heightStep);
             if (GUILayout.Button("Height -1")) ChangePlacedObjectHeight(-heightStep);
             EditorGUILayout.EndHorizontal();
             EditorGUILayout.Space();
        }
        
        foreach (var obj in settings.placeableObjects)
        {
            if (obj.prefab != null && GUILayout.Button($"Place {obj.name}"))
            {
                PlaceObjectOnHex(hexComponent, obj);
            }
        }
        
        if (hexComponent.PlacedObject != null && GUILayout.Button("Clear Placed Object"))
        {
            ClearObjectFromHex(hexComponent);
        }
    }

    private void ChangeHexHeight(float amount)
    {
        Undo.RecordObject(selectedHex.transform, "Change Hex Height");
        selectedHex.transform.position += new Vector3(0, amount, 0);
    }
    
    private void ChangePlacedObjectHeight(float amount)
    {
        if (selectedHex == null) return;
        var hexComponent = selectedHex.GetComponent<Hex>();
        if (hexComponent == null || hexComponent.PlacedObject == null) return;

        Undo.RecordObject(hexComponent.PlacedObject.transform, "Change Placed Object Height");
        hexComponent.PlacedObject.transform.position += new Vector3(0, amount, 0);
    }

    private void PlaceObjectOnHex(Hex hexComponent, PlaceableObject objectToPlace)
    {
        if (hexComponent.PlacedObject != null)
        {
            ClearObjectFromHex(hexComponent);
        }
        
        GameObject hex = hexComponent.gameObject;
    
        Transform propsContainer = hex.transform.Find("Props");
        if (propsContainer == null)
        {
            GameObject propsObject = new GameObject("Props");
            propsObject.transform.SetParent(hex.transform, false);
            propsContainer = propsObject.transform;
        }

        GameObject placedObject = PrefabUtility.InstantiatePrefab(objectToPlace.prefab) as GameObject;
        placedObject.transform.position = hex.transform.position + Vector3.up * objectToPlace.yOffset;
        placedObject.transform.SetParent(propsContainer, true);
    
        hexComponent.SetPlacedObject(placedObject);
    
        Undo.RegisterCreatedObjectUndo(placedObject, "Place Object");
        if (propsContainer.gameObject.hideFlags == HideFlags.None)
        {
            Undo.RegisterCreatedObjectUndo(propsContainer.gameObject, "Create Props Container");
        }
    }

    private void ClearObjectFromHex(Hex hex)
    {
        if (hex.PlacedObject == null) return;
        Undo.DestroyObjectImmediate(hex.PlacedObject);
        hex.ClearPlacedObject();
        EditorUtility.SetDirty(hex);
    }

    private void PaintHex(Vector3Int coords, Vector3 paintPosition)
    {
        EraseHex(coords, hexGridComponent, false);
        if (selectedBrushIndex >= settings.hexPrefabs.Count) return;

        var prefabToInstantiate = settings.hexPrefabs[selectedBrushIndex].prefab;
        if (prefabToInstantiate == null) return;

        var newHexObj = (GameObject)PrefabUtility.InstantiatePrefab(prefabToInstantiate, gridParent);
        newHexObj.transform.position = paintPosition;
        Undo.RegisterCreatedObjectUndo(newHexObj, "Paint Hex");
        newHexObj.name = $"Hex_{coords.x}_{coords.z} ({prefabToInstantiate.name})";
        
        var hexComponent = newHexObj.GetComponent<Hex>();
        if (hexComponent == null) return;
        hexComponent.RoomID = currentRoomID;
        EditorUtility.SetDirty(hexComponent);
    }

    private void GenerateGrid()
    {
        ClearGrid();
        var defaultPrefab = settings.hexPrefabs.FirstOrDefault(p => p is { isDefault: true })?.prefab;
        if (defaultPrefab == null) { Debug.LogError("Please mark one prefab as 'isDefault' in your Settings Asset."); return; }
        
        for (var z = 0; z < gridHeight; z++)
        {
            for (var x = 0; x < gridWidth; x++)
            {
                 var hexCoords = new Vector3Int(x, 0, z);
                 var placementPosition = CoordsToWorldPosition(hexCoords, hexGridComponent);
                 var newHexObj = (GameObject)PrefabUtility.InstantiatePrefab(defaultPrefab, gridParent);
                 newHexObj.transform.position = placementPosition;
                 newHexObj.name = $"Hex_{hexCoords.x}_{hexCoords.z}";
                 
                 var hexComponent = newHexObj.GetComponent<Hex>();
                 if(hexComponent != null) hexComponent.RoomID = currentRoomID;
            }
        }
        Undo.RegisterFullObjectHierarchyUndo(gridParent.gameObject, "Generate Grid");
    }

    private void DrawBrushSelectionUI()
    {
        EditorGUILayout.LabelField("Hex Brushes", EditorStyles.boldLabel);
        var gridCols = Mathf.FloorToInt(position.width / 110);
        if (gridCols < 1) gridCols = 1;

        EditorGUILayout.BeginHorizontal();
        var current_col = 0;
        for (var i = 0; i < settings.hexPrefabs.Count; i++)
        {
            if (settings.hexPrefabs[i]?.prefab == null) continue;

            var isSelected = selectedBrushType == BrushType.Hex && selectedBrushIndex == i;
            GUI.backgroundColor = (isSelected && currentTool == ToolMode.Paint) ? Color.cyan : Color.white;
            
            if (GUILayout.Button(settings.hexPrefabs[i].prefab.name, GUILayout.Width(100), GUILayout.Height(40)))
            {
                currentTool = ToolMode.Paint;
                selectedBrushType = BrushType.Hex;
                selectedBrushIndex = i;
            }
            
            current_col++;
            if (current_col < gridCols) continue;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            current_col = 0;
        }
        EditorGUILayout.EndHorizontal();
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Object Brushes", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        current_col = 0;
        for (var i = 0; i < settings.placeableObjects.Count; i++)
        {
            if (settings.placeableObjects[i]?.prefab == null) continue;
            
            var isSelected = selectedBrushType == BrushType.PlaceableObject && selectedBrushIndex == i;
            GUI.backgroundColor = (isSelected && currentTool == ToolMode.Paint) ? Color.cyan : Color.white;

            if (GUILayout.Button(settings.placeableObjects[i].name, GUILayout.Width(100), GUILayout.Height(40)))
            {
                currentTool = ToolMode.Paint;
                selectedBrushType = BrushType.PlaceableObject;
                selectedBrushIndex = i;
            }

            current_col++;
            if (current_col >= gridCols)
            {
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                current_col = 0;
            }
        }
        EditorGUILayout.EndHorizontal();
        GUI.backgroundColor = Color.white;
    }
    
    private void PaintObject(Vector3Int coords, HexGrid grid)
    {
        var hexGameObject = FindHexAt(coords, grid);
        if (hexGameObject == null)
        {
            Debug.LogWarning("Cannot place object: No hex tile exists at this position.");
            return;
        }

        var hexComponent = hexGameObject.GetComponent<Hex>();
        if (hexComponent == null)
        {
            Debug.LogError("Target object is not a valid Hex.");
            return;
        }

        if (selectedBrushIndex >= settings.placeableObjects.Count) return;
        var objectToPlace = settings.placeableObjects[selectedBrushIndex];
        if (objectToPlace == null) return;

        PlaceObjectOnHex(hexComponent, objectToPlace);
    }
    
    private void CalculateAndSetHexSize()
    {
        var defaultPrefab = settings.hexPrefabs.FirstOrDefault(p => p is { isDefault: true })?.prefab;
        if (defaultPrefab == null)
        {
            Debug.LogError("Cannot calculate size: Please mark one prefab as 'isDefault' in your Settings Asset.");
            return;
        }
        var renderer = defaultPrefab.GetComponentInChildren<MeshRenderer>();
        if (renderer == null)
        {
            Debug.LogError($"Cannot calculate size: The default prefab '{defaultPrefab.name}' and its children have no MeshRenderer component.");
            return;
        }
        var size = renderer.bounds.size;
        hexGridComponent.hexWidth = size.x;
        hexGridComponent.hexHeight = size.z;
        EditorUtility.SetDirty(hexGridComponent);
        Debug.Log($"Hex dimensions calculated and set on HexGrid: Width={hexGridComponent.hexWidth}, Height={hexGridComponent.hexHeight}");
    }

    private void PickHex(Vector3Int coords, HexGrid grid)
    {
        var hexToPick = FindHexAt(coords, grid);
        if (hexToPick == null) return;
        
        for(var i = 0; i < settings.hexPrefabs.Count; i++)
        {
            if (settings.hexPrefabs[i] == null || settings.hexPrefabs[i].prefab == null ||
                PrefabUtility.GetCorrespondingObjectFromSource(hexToPick) != settings.hexPrefabs[i].prefab) continue;
            selectedBrushType = BrushType.Hex;
            selectedBrushIndex = i;
            currentTool = ToolMode.Paint;
            Repaint();
            return;
        }
    }
    
    private Vector3 CoordsToWorldPosition(Vector3Int coords, HexGrid grid)
    {
        var x = coords.x * grid.hexWidth;
        var z = coords.z * grid.ZSpacing;
        if (coords.z % 2 != 0) x += grid.hexWidth / 2f;
        return new Vector3(x, 0, z) + gridParent.position;
    }
    
    private Vector3Int WorldPositionToCoords(Vector3 worldPos, HexGrid grid)
    {
        worldPos -= gridParent.position;
        var z = Mathf.RoundToInt(worldPos.z / grid.ZSpacing);
        var xOffset = (z % 2 != 0) ? grid.hexWidth / 2f : 0;
        var x = Mathf.RoundToInt((worldPos.x - xOffset) / grid.hexWidth);
        return new Vector3Int(x, 0, z);
    }
    
    private void EraseHex(Vector3Int coords, HexGrid grid, bool registerUndo = true)
    {
        var hexToErase = FindHexAt(coords, grid);
        if (hexToErase == null) return;
        if (selectedHex == hexToErase) selectedHex = null;
        if(registerUndo) Undo.DestroyObjectImmediate(hexToErase);
        else DestroyImmediate(hexToErase);
    }
    
    private GameObject FindHexAt(Vector3Int coords, HexGrid grid)
    {
        if (gridParent == null) return null;
        var targetWorldPos = CoordsToWorldPosition(coords, grid);
        return (from Transform hexTransform in gridParent 
            where Vector3.Distance(hexTransform.position, targetWorldPos) < 0.01f 
            select hexTransform.gameObject).FirstOrDefault();
    }

    private void ClearGrid()
    {
        if (gridParent == null) return;
        selectedHex = null;
        for (var i = gridParent.childCount - 1; i >= 0; i--)
        {
            Undo.DestroyObjectImmediate(gridParent.GetChild(i).gameObject);
        }
    }

    private bool CheckPrerequisites()
    {
        if (gridParent == null) return false;
        if (hexGridComponent == null)
        {
            if(Event.current.type == EventType.Repaint)
                EditorGUILayout.HelpBox("Please assign a HexGrid component reference.", MessageType.Error);
            return false;
        }
        if (settings == null || settings.hexPrefabs.Count == 0 || settings.hexPrefabs.All(p => p == null || p.prefab == null)) return false;
        return true;
    }
}