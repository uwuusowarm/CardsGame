using UnityEngine;
using UnityEditor;
using System.IO;
using System.Globalization;

public class ItemCSVImporter
{
    private const string CSV_PATH_KEY = "ItemCSVImporter_Path";
    private const string ITEM_DATA_FOLDER = "Assets/GameData/Items";
    private const string PREFAB_FOLDER = "Assets/1_Prefabs/Items";
    private const string SPRITE_FOLDER = "Assets/Art Complete/ItemSprites"; 

    [MenuItem("Tools/Import Items from CSV")]
    public static void ImportItems()
    {
        string lastPath = EditorPrefs.GetString(CSV_PATH_KEY, Application.dataPath);
        string path = EditorUtility.OpenFilePanel("Open Item CSV", lastPath, "csv");

        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        EditorPrefs.SetString(CSV_PATH_KEY, Path.GetDirectoryName(path));

        string[] lines = File.ReadAllLines(path);

        if (lines.Length <= 1)
        {
            EditorUtility.DisplayDialog("Error", "CSV file is empty or only has a header.", "OK");
            return;
        }

        if (!Directory.Exists(ITEM_DATA_FOLDER)) Directory.CreateDirectory(ITEM_DATA_FOLDER);
        if (!Directory.Exists(PREFAB_FOLDER)) Directory.CreateDirectory(PREFAB_FOLDER);
        if (!Directory.Exists(SPRITE_FOLDER)) Directory.CreateDirectory(SPRITE_FOLDER);

        int itemsCreated = 0;
        int itemsUpdated = 0;
        int prefabsCreated = 0;
        int prefabsUpdated = 0;

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line) || line.StartsWith(";;;;"))
            {
                continue;
            }

            string[] values = line.Split(';');

            if (values.Length < 17)
            {
                Debug.LogWarning($"Skipping line {i + 1}: Not enough columns (found {values.Length}, expected 17). Line content: '{line}'");
                continue;
            }

            try
            {
                ItemData itemData = null;
                string itemName = SanitizeFileName(values[2]);
                if (string.IsNullOrEmpty(itemName))
                {
                    itemName = $"Item_ID_{values[0]}";
                }

                string assetFileName = $"{values[0]}_{itemName}.asset";
                string assetPath = Path.Combine(ITEM_DATA_FOLDER, assetFileName);

                itemData = AssetDatabase.LoadAssetAtPath<ItemData>(assetPath);
                bool isNewItem = itemData == null;

                if (isNewItem)
                {
                    itemData = ScriptableObject.CreateInstance<ItemData>();
                    AssetDatabase.CreateAsset(itemData, assetPath);
                    itemsCreated++;
                }
                else
                {
                    itemsUpdated++;
                }

                itemData.id = ParseInt(values[0]);
                itemData.itemClass = ParseItemClassType(values[1]);
                itemData.name = values[2];
                itemData.tier = ParseInt(values[3]);
                itemData.itemSlot = ParseItemSlot(values[4]);
                itemData.damageBonus = ParseInt(values[5]);
                itemData.movementSpeedBonus = ParseInt(values[6]);
                itemData.defenseBonus = ParseInt(values[7]);
                itemData.healBonus = ParseInt(values[8]);
                itemData.maxHpBonus = ParseInt(values[9]);
                itemData.maxApBonus = ParseInt(values[10]);
                itemData.range = ParseInt(values[11]);
                itemData.classBonus1Type = ParseStatBonusType(values[12]);
                itemData.classBonus1Amount = ParseInt(values[13]);
                itemData.classBonus2Type = ParseStatBonusType(values[14]);
                itemData.classBonus2Amount = ParseInt(values[15]);
                itemData.totalValue = ParseInt(values[16]);
                
                string[] spriteGuids = AssetDatabase.FindAssets($"t:Sprite {itemData.id}", new[] { SPRITE_FOLDER });
                if (spriteGuids.Length > 0)
                {
                    string spritePath = AssetDatabase.GUIDToAssetPath(spriteGuids[0]);
                    itemData.itemIcon = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                    if (spriteGuids.Length > 1)
                    {
                        Debug.LogWarning($"Found multiple sprites for ID {itemData.id} in {SPRITE_FOLDER}. Using the first one: {spritePath}");
                    }
                }
                else if (isNewItem) 
                {
                    Debug.LogWarning($"No sprite found for item ID {itemData.id} in {SPRITE_FOLDER}. Expected a file named '{itemData.id}.png' (or .jpg, etc).");
                }

                EditorUtility.SetDirty(itemData);

                string prefabFileName = $"{values[0]}_{itemName}.prefab";
                string prefabPath = Path.Combine(PREFAB_FOLDER, prefabFileName);

                GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                
                if (existingPrefab == null)
                {
                    GameObject newPrefabRoot = new GameObject(itemData.name);
                    ItemPickup pickupComponent = newPrefabRoot.AddComponent<ItemPickup>();
                    
                    SpriteRenderer spriteRenderer = newPrefabRoot.AddComponent<SpriteRenderer>();
                    spriteRenderer.sprite = itemData.itemIcon; 

                    SerializedObject so = new SerializedObject(pickupComponent);
                    so.FindProperty("itemData").objectReferenceValue = itemData;
                    so.ApplyModifiedProperties();
                    
                    PrefabUtility.SaveAsPrefabAsset(newPrefabRoot, prefabPath);
                    GameObject.DestroyImmediate(newPrefabRoot);
                    prefabsCreated++;
                }
                else
                {
                    GameObject prefabInstance = (GameObject)PrefabUtility.InstantiatePrefab(existingPrefab);
                    ItemPickup pickupComponent = prefabInstance.GetComponent<ItemPickup>();
                    if(pickupComponent == null) pickupComponent = prefabInstance.AddComponent<ItemPickup>();

                    SpriteRenderer spriteRenderer = prefabInstance.GetComponent<SpriteRenderer>();
                    if (spriteRenderer == null) spriteRenderer = prefabInstance.AddComponent<SpriteRenderer>();
                    spriteRenderer.sprite = itemData.itemIcon; 

                    SerializedObject so = new SerializedObject(pickupComponent);
                    so.FindProperty("itemData").objectReferenceValue = itemData;
                    so.ApplyModifiedProperties();

                    if (prefabInstance.name != itemData.name)
                    {
                        prefabInstance.name = itemData.name;
                    }

                    PrefabUtility.SaveAsPrefabAsset(prefabInstance, prefabPath);
                    GameObject.DestroyImmediate(prefabInstance);
                    prefabsUpdated++;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error processing line {i + 1}: '{line}'. Exception: {ex.Message}\nStackTrace: {ex.StackTrace}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        PopulateItemDatabase();

        EditorUtility.DisplayDialog("Import Complete",
            $"{itemsCreated} items created, {prefabsCreated} prefabs created.\n" +
            $"{itemsUpdated} items updated, {prefabsUpdated} prefabs updated.", "OK");
        Debug.Log($"Item import complete. Assets saved in {ITEM_DATA_FOLDER} and {PREFAB_FOLDER}");
    }

    // --- (No changes to the helper methods below this line) ---

    private static int ParseInt(string value, int defaultValue = 0)
    {
        if (string.IsNullOrWhiteSpace(value)) return defaultValue;
        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result))
        {
            return result;
        }
        Debug.LogWarning($"Could not parse int: '{value}'. Returning default {defaultValue}.");
        return defaultValue;
    }

    private static ItemSlot ParseItemSlot(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return ItemSlot.None;
        try
        {
            return (ItemSlot)System.Enum.Parse(typeof(ItemSlot), value, true);
        }
        catch (System.ArgumentException)
        {
            Debug.LogWarning($"Could not parse ItemSlot: '{value}'. Returning {ItemSlot.None}. Valid options are: {string.Join(", ", System.Enum.GetNames(typeof(ItemSlot)))}");
            return ItemSlot.None;
        }
    }

    private static ItemClassType ParseItemClassType(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return ItemClassType.Base;
        try
        {
            return (ItemClassType)System.Enum.Parse(typeof(ItemClassType), value, true);
        }
        catch (System.ArgumentException)
        {
            Debug.LogWarning($"Could not parse ItemClassType: '{value}'. Returning {ItemClassType.Base}. Valid options are: {string.Join(", ", System.Enum.GetNames(typeof(ItemClassType)))}");
            return ItemClassType.Base;
        }
    }

    private static StatBonusType ParseStatBonusType(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return StatBonusType.None;
        string normalizedValue = value.Replace(" ", "");

        try
        {
            return (StatBonusType)System.Enum.Parse(typeof(StatBonusType), normalizedValue, true);
        }
        catch (System.ArgumentException)
        {
            Debug.LogWarning($"Could not parse StatBonusType: '{value}' (normalized: '{normalizedValue}'). Returning {StatBonusType.None}. Valid options are: {string.Join(", ", System.Enum.GetNames(typeof(StatBonusType)))}");
            return StatBonusType.None;
        }
    }

    private static string SanitizeFileName(string name)
    {
        if (string.IsNullOrEmpty(name)) return "UnnamedItem";
        string invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(Path.GetInvalidFileNameChars()));
        string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);
        return System.Text.RegularExpressions.Regex.Replace(name, invalidRegStr, "_");
    }
    private static void PopulateItemDatabase()
    {
        Debug.Log("Attempting to populate ItemDatabase...");

        string[] dbGuids = AssetDatabase.FindAssets("t:ItemDatabase");
        if (dbGuids.Length == 0)
        {
            Debug.LogError("Could not find an ItemDatabase asset in the project. Please create one via Assets > Create > Game > Item Database.");
            return;
        }
        if (dbGuids.Length > 1)
        {
            Debug.LogWarning("Multiple ItemDatabase assets found. Using the first one.");
        }

        string dbPath = AssetDatabase.GUIDToAssetPath(dbGuids[0]);
        ItemDatabase database = AssetDatabase.LoadAssetAtPath<ItemDatabase>(dbPath);

        string[] itemGuids = AssetDatabase.FindAssets("t:ItemData");
        
        database.allItems.Clear(); 

        foreach (string guid in itemGuids)
        {
            string itemPath = AssetDatabase.GUIDToAssetPath(guid);
            ItemData itemData = AssetDatabase.LoadAssetAtPath<ItemData>(itemPath);
            if (itemData != null)
            {
                database.allItems.Add(itemData);
            }
        }
        
        EditorUtility.SetDirty(database);
        AssetDatabase.SaveAssets();

        Debug.Log($"Successfully populated ItemDatabase with {database.allItems.Count} items.");
    }
}