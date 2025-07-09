using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DeckStorage : MonoBehaviour
{
    public static string savePath => Path.Combine(Application.persistentDataPath, "decks.json");

    public static DeckDataList LoadDecks()
    {
        string path = Path.Combine(Application.persistentDataPath, "decks.json");
        if (!File.Exists(path))
            return new DeckDataList { decks = new List<DeckData>() };

        string json = File.ReadAllText(path);
        DeckDataList data = JsonUtility.FromJson<DeckDataList>(json);

        // 🔁 古いデッキに deckId を補完（任意）
        foreach (var deck in data.decks)
        {
            if (string.IsNullOrEmpty(deck.deckId))
                deck.deckId = System.Guid.NewGuid().ToString();
        }

        return data;
    }

    public static void SaveDecks(DeckDataList deckList)
    {
        string json = JsonUtility.ToJson(deckList, true);
        File.WriteAllText(savePath, json);
    }

    public static void EnsureDefaultDecksLoaded()
    {
        if (File.Exists(savePath))
            return; // 既に保存データが存在するならスキップ
        Debug.Log("初回起動 - デフォルトデッキをロードします");

        // デフォルトデッキデータを Resources から読み込み
        TextAsset[] defaultDecks = Resources.LoadAll<TextAsset>("DefaultDecks");
        DeckDataList newList = new DeckDataList { decks = new List<DeckData>() };

        foreach (var deckAsset in defaultDecks)
        {
            DeckData deck = JsonUtility.FromJson<DeckData>(deckAsset.text);
            if (string.IsNullOrEmpty(deck.deckId))
                deck.deckId = System.Guid.NewGuid().ToString();
            newList.decks.Add(deck);
        }

        SaveDecks(newList);
    }
}
