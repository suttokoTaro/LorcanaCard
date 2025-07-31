using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine;

public static class OwnedListStorage
{
    private static string SavePath => Path.Combine(Application.persistentDataPath, "ownedlist.json");

    public static OwnedListData Load()
    {
        if (!File.Exists(SavePath))
            return new OwnedListData();

        string json = File.ReadAllText(SavePath);
        return JsonUtility.FromJson<OwnedListData>(json);
    }

    public static void Save(OwnedListData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
    }

    public static void AddOrUpdate(int cardId, int delta)
    {
        var data = Load();
        var entry = data.entries.Find(e => e.cardId == cardId);

        if (entry != null)
        {
            entry.count += delta;
            if (entry.count <= 0)
                data.entries.Remove(entry); // 0以下なら削除
        }
        else if (delta > 0)
        {
            data.entries.Add(new OwnedEntry { cardId = cardId, count = delta });
        }

        Save(data);
    }

    public static int GetCount(int cardId)
    {
        var data = Load();
        var entry = data.entries.Find(e => e.cardId == cardId);
        return entry != null ? entry.count : 0;
    }

    public static void Clear()
    {
        if (File.Exists(SavePath))
            File.Delete(SavePath);
    }
}

