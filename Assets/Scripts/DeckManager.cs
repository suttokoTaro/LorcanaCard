using System.Collections.Generic;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance;

    public List<int> selectedPlayerDeck;
    public List<int> selectedEnemyDeck;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // シーンをまたいでも保持
        }
        else
        {
            Destroy(gameObject); // 複製を防ぐ
        }
    }
    public static string GetDeckPath(string deckName)
    {
        return Path.Combine(Application.persistentDataPath, $"deck_{deckName}.json");
    }

    public static void SaveDeck(DeckData deck)
    {
        if (deck == null) return;

        string path = GetDeckPath(deck.deckName);
        string json = JsonUtility.ToJson(deck, true);
        File.WriteAllText(path, json);
        Debug.Log($"デッキ保存: {path}");
    }
}
