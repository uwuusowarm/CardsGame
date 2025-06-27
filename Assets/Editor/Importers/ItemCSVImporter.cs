using UnityEngine;
using UnityEditor;
using System.IO;
using System.Globalization; 

public class ItemCSVImporter
{
    private const string CSV_PATH_KEY = "ItemCSVImporter_Path";
    private const string DEFAULT_ASSET_FOLDER = "Assets/GameData/Items"; 

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

        if (!Directory.Exists(DEFAULT_ASSET_FOLDER))
        {
            Directory.CreateDirectory(DEFAULT_ASSET_FOLDER);
        }

        int itemsCreated = 0;
        int itemsUpdated = 0;

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line) || line.StartsWith(";;;;")) 
            {
                continue;
            }

            string[] values = line.Split(';');

            // ID;Klasse;Name;Tier;Rüstungsart;Dmg Bonus ;Ms Bonus;Def Bonus;Heal Bonus;Max HP;Max AP;Range;1 Class Bonus;1 Bonus Amount;2 Class Bonus;2 Bonus Amount;Total
            if (values.Length < 17)
            {
                Debug.LogWarning($"Skipping line {i + 1}: Not enough columns (found {values.Length}, expected 17). Line content: '{line}'");
                continue;
            }

            try
            {
                ItemData item = null;
                string itemName = SanitizeFileName(values[2]); 
                if (string.IsNullOrEmpty(itemName))
                {
                    itemName = $"Item_ID_{values[0]}"; 
                }
                
                string assetFileName = $"{values[0]}_{itemName}.asset";
                string assetPath = Path.Combine(DEFAULT_ASSET_FOLDER, assetFileName);

                item = AssetDatabase.LoadAssetAtPath<ItemData>(assetPath);

                if (item == null)
                {
                    item = ScriptableObject.CreateInstance<ItemData>();
                    AssetDatabase.CreateAsset(item, assetPath);
                    itemsCreated++;
                }
                else
                {
                    itemsUpdated++;
                }

                // Populate data
                item.id = ParseInt(values[0]);
                item.itemClass = ParseItemClassType(values[1]);
                item.name = values[2];
                item.tier = ParseInt(values[3]);
                item.itemSlot = ParseItemSlot(values[4]);

                item.damageBonus = ParseInt(values[5]);
                item.movementSpeedBonus = ParseInt(values[6]);
                item.defenseBonus = ParseInt(values[7]);
                item.healBonus = ParseInt(values[8]);
                item.maxHpBonus = ParseInt(values[9]);
                item.maxApBonus = ParseInt(values[10]);
                item.range = ParseInt(values[11]);

                item.classBonus1Type = ParseStatBonusType(values[12]);
                item.classBonus1Amount = ParseInt(values[13]);
                item.classBonus2Type = ParseStatBonusType(values[14]);
                item.classBonus2Amount = ParseInt(values[15]);

                item.totalValue = ParseInt(values[16]);

                EditorUtility.SetDirty(item); 
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error processing line {i + 1}: '{line}'. Exception: {ex.Message}\nStackTrace: {ex.StackTrace}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Import Complete",
            $"{itemsCreated} items created.\n{itemsUpdated} items updated.", "OK");
        Debug.Log($"Item import complete. Created: {itemsCreated}, Updated: {itemsUpdated}. Assets saved in {DEFAULT_ASSET_FOLDER}");
    }
    
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
}