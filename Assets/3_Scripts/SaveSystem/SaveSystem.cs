using UnityEngine;
using System.IO;
using System.Collections.Generic;

public static class SaveSystem
{
    private static readonly string SAVE_FILE_NAME = "decks.json";

    public static void SaveDecks(List<Deck> decks)
    {
        DeckSaveData data = new DeckSaveData { playerDecks = decks };
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetSavePath(), json);
        Debug.Log("Decks saved to: " + GetSavePath());
    }

    public static List<Deck> LoadDecks()
    {
        string path = GetSavePath();
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            DeckSaveData data = JsonUtility.FromJson<DeckSaveData>(json);
            Debug.Log("Decks loaded from: " + path);
            return data.playerDecks;
        }
        else
        {
            Debug.LogWarning("No save file found. Returning new empty deck list.");
            return new List<Deck>();
        }
    }

    private static string GetSavePath()
    {
        return Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
    }
}