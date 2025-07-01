// Place this script in a folder named "Editor"

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

public class HexGridGenerator : EditorWindow
{
    [SerializeField] private Transform gridParent;
    [SerializeField] private List<HexPrefabMapping> hexPrefabs = new List<HexPrefabMapping>();

    private SerializedObject serializedObject;
    private SerializedProperty serializedPrefabs;
    private SerializedProperty serializedGridParent;

    private int gridWidth = 20;
    private int gridHeight = 20;

    private int selectedPrefabIndex = 0;
    private ToolMode currentTool = ToolMode.Paint;
    private enum ToolMode { Paint, Erase, Pick }
    private string[] toolNames = { "Paint", "Erase", "Pick" };
    private Vector2 scrollPos;

    [MenuItem("Tools/Hex Grid Generator")]
    public static void ShowWindow()
    {
        GetWindow<HexGridGenerator>("Hex Grid Generator");
    }

    private void OnEnable()
    {
        serializedObject = new SerializedObject(this);
        serializedPrefabs = serializedObject.FindProperty("hexPrefabs");
        serializedGridParent = serializedObject.FindProperty("gridParent");
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnGUI()
    {
        serializedObject.Update();
        
        EditorGUILayout.LabelField("Hex Grid Generator", EditorStyles.boldLabel);
        
        EditorGUILayout.HelpBox(
            "1. Assign your HexGrid object to the 'Grid Parent' field.\n" +
            "2. Add your prefabs and mark one as 'isDefault'.\n" +
            "3. Click 'Calculate Size From Default Prefab' to automatically set grid dimensions.\n" +
            "4. Use the tools to paint your level.", MessageType.Info);
        
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(serializedGridParent);

        // --- NEW SIZE CALCULATION BUTTON ---
        if (gridParent != null && gridParent.GetComponent<HexGrid>() != null)
        {
            if (GUILayout.Button("Calculate Size From Default Prefab"))
            {
                CalculateAndSetHexSize();
            }
        }
        
        EditorGUILayout.PropertyField(serializedPrefabs, true);

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
        EditorGUILayout.LabelField("Painting Tools", EditorStyles.boldLabel);
        currentTool = (ToolMode)GUILayout.Toolbar((int)currentTool, toolNames);
        
        // Rest of GUI is unchanged...
        EditorGUILayout.Space();
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        EditorGUILayout.LabelField("Brushes", EditorStyles.boldLabel);
        int gridCols = Mathf.FloorToInt(position.width / 110);
        if (gridCols < 1) gridCols = 1;
        int current_col = 0;
        EditorGUILayout.BeginHorizontal();
        for (int i = 0; i < hexPrefabs.Count; i++)
        {
            if (hexPrefabs[i] == null || hexPrefabs[i].prefab == null) continue;
            GUI.backgroundColor = (i == selectedPrefabIndex && currentTool == ToolMode.Paint) ? Color.cyan : Color.white;
            if (GUILayout.Button(hexPrefabs[i].prefab.name, GUILayout.Width(100), GUILayout.Height(40))) { selectedPrefabIndex = i; currentTool = ToolMode.Paint; }
            current_col++;
            if (current_col >= gridCols) { EditorGUILayout.EndHorizontal(); EditorGUILayout.BeginHorizontal(); current_col = 0; }
        }
        EditorGUILayout.EndHorizontal();
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndScrollView();

        serializedObject.ApplyModifiedProperties();
    }

    private void CalculateAndSetHexSize()
    {
        GameObject defaultPrefab = hexPrefabs.FirstOrDefault(p => p != null && p.isDefault)?.prefab;
        if (defaultPrefab == null)
        {
            Debug.LogError("Cannot calculate size: Please mark one prefab as 'isDefault'.");
            return;
        }

        var renderer = defaultPrefab.GetComponentInChildren<MeshRenderer>();
        if (renderer == null)
        {
            Debug.LogError($"Cannot calculate size: The default prefab '{defaultPrefab.name}' and its children have no MeshRenderer component.");
            return;
        }

        Vector3 size = renderer.bounds.size;
        HexGrid grid = gridParent.GetComponent<HexGrid>();
        
        // For a pointy-topped hex, the width is X and the height is Z.
        grid.hexWidth = size.x;
        grid.hexHeight = size.z;
        
        EditorUtility.SetDirty(grid); // Mark the HexGrid component as changed so it saves.
        Debug.Log($"Hex dimensions calculated and set on HexGrid: Width={grid.hexWidth}, Height={grid.hexHeight}");
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (!CheckPrerequisites()) return;

        HexGrid grid = gridParent.GetComponent<HexGrid>();

        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, gridParent.position);

        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 worldPosition = ray.GetPoint(enter);
            Vector3Int hexCoords = WorldPositionToCoords(worldPosition, grid);
            Vector3 placementPosition = CoordsToWorldPosition(hexCoords, grid);

            Handles.color = currentTool == ToolMode.Erase ? Color.red : Color.green;
            Handles.DrawWireDisc(placementPosition, Vector3.up, grid.hexWidth / 2f);
            
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                Event.current.Use();
                switch(currentTool)
                {
                    case ToolMode.Paint: PaintHex(hexCoords, placementPosition); break;
                    case ToolMode.Erase: EraseHex(hexCoords, grid); break;
                    case ToolMode.Pick: PickHex(hexCoords, grid); break;
                }
            }
        }
        sceneView.Repaint();
    }
    
    // --- POSITIONING LOGIC NOW USES VALUES FROM THE HEXGRID COMPONENT ---
    private Vector3 CoordsToWorldPosition(Vector3Int coords, HexGrid grid)
    {
        float x = coords.x * grid.hexWidth;
        float z = coords.z * grid.ZSpacing;
        
        if (coords.z % 2 != 0) // Stagger odd rows
        {
            x += grid.hexWidth / 2f;
        }
        return new Vector3(x, 0, z) + gridParent.position;
    }
    
    private Vector3Int WorldPositionToCoords(Vector3 worldPos, HexGrid grid)
    {
        worldPos -= gridParent.position;

        int z = Mathf.RoundToInt(worldPos.z / grid.ZSpacing);
        float xOffset = (z % 2 != 0) ? grid.hexWidth / 2f : 0;
        int x = Mathf.RoundToInt((worldPos.x - xOffset) / grid.hexWidth);

        return new Vector3Int(x, 0, z);
    }
    
    // Most painter functions now need the grid reference
    private void EraseHex(Vector3Int coords, HexGrid grid, bool registerUndo = true)
    {
        GameObject hexToErase = FindHexAt(coords, grid);
        if (hexToErase != null)
        {
            if(registerUndo) Undo.DestroyObjectImmediate(hexToErase);
            else DestroyImmediate(hexToErase);
        }
    }

    private void PickHex(Vector3Int coords, HexGrid grid)
    {
        GameObject hexToPick = FindHexAt(coords, grid);
        if (hexToPick == null) return;
        
        for(int i = 0; i < hexPrefabs.Count; i++)
        {
            if (hexPrefabs[i] != null && hexPrefabs[i].prefab != null && PrefabUtility.GetCorrespondingObjectFromSource(hexToPick) == hexPrefabs[i].prefab)
            {
                selectedPrefabIndex = i;
                currentTool = ToolMode.Paint;
                Repaint();
                return;
            }
        }
    }
    
    private GameObject FindHexAt(Vector3Int coords, HexGrid grid)
    {
        if (gridParent == null) return null;
        foreach (Transform hexTransform in gridParent)
        {
            if (WorldPositionToCoords(hexTransform.position, grid) == coords)
            {
                return hexTransform.gameObject;
            }
        }
        return null;
    }

    // Unchanged functions...
    private void PaintHex(Vector3Int coords, Vector3 position)
    {
        HexGrid grid = gridParent.GetComponent<HexGrid>();
        EraseHex(coords, grid, false);
        if (selectedPrefabIndex >= hexPrefabs.Count) return;
        GameObject prefabToInstantiate = hexPrefabs[selectedPrefabIndex].prefab;
        if (prefabToInstantiate == null) return;
        GameObject newHex = (GameObject)PrefabUtility.InstantiatePrefab(prefabToInstantiate, gridParent);
        newHex.transform.position = position;
        Undo.RegisterCreatedObjectUndo(newHex, "Paint Hex");
        newHex.name = $"Hex_{coords.x}_{coords.z} ({prefabToInstantiate.name})";
    }
    
    private void GenerateGrid()
    {
        ClearGrid();
        GameObject defaultPrefab = hexPrefabs.FirstOrDefault(p => p != null && p.isDefault)?.prefab;
        if (defaultPrefab == null) { Debug.LogError("Please mark one prefab as 'isDefault' to be used for grid generation."); return; }
        
        HexGrid grid = gridParent.GetComponent<HexGrid>();
        for (int z = 0; z < gridHeight; z++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                 Vector3Int hexCoords = new Vector3Int(x, 0, z);
                 Vector3 placementPosition = CoordsToWorldPosition(hexCoords, grid);
                 GameObject newHex = (GameObject)PrefabUtility.InstantiatePrefab(defaultPrefab, gridParent);
                 newHex.transform.position = placementPosition;
                 newHex.name = $"Hex_{hexCoords.x}_{hexCoords.z}";
            }
        }
        Undo.RegisterFullObjectHierarchyUndo(gridParent.gameObject, "Generate Grid");
    }

    private void ClearGrid()
    {
        if (gridParent == null) return;
        for (int i = gridParent.childCount - 1; i >= 0; i--)
        {
            Undo.DestroyObjectImmediate(gridParent.GetChild(i).gameObject);
        }
    }

    private bool CheckPrerequisites()
    {
        if (gridParent == null) return false;
        if (gridParent.GetComponent<HexGrid>() == null)
        {
            if(Event.current.type == EventType.Repaint) EditorGUILayout.HelpBox("The 'Grid Parent' must have a HexGrid component attached.", MessageType.Error);
            return false;
        }
        if (hexPrefabs.Count == 0 || hexPrefabs.All(p => p == null || p.prefab == null)) return false;
        return true;
    }
}