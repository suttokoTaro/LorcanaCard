using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DeckStorage : MonoBehaviour
{
    public static string savePath => Path.Combine(Application.persistentDataPath, "decks.json");

    public static DeckDataList LoadDecks()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            return JsonUtility.FromJson<DeckDataList>(json);
        }
        else
        {
            return new DeckDataList(); // 空のリストを返す
        }
    }

    public static void SaveDecks(DeckDataList deckList)
    {
        string json = JsonUtility.ToJson(deckList, true);
        File.WriteAllText(savePath, json);
    }
}
