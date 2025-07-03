using System.Collections.Generic;
using UnityEngine;
using System.Collections.Generic;

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
}
