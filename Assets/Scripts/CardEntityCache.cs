using System.Collections.Generic;
using UnityEngine;

public class CardEntityCache : MonoBehaviour
{
    public static CardEntityCache Instance { get; private set; }

    public List<CardEntity> AllCardEntities { get; private set; } = new List<CardEntity>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // シーンをまたいでも保持
    }

    public void SetCardEntities(IList<CardEntity> entities)
    {
        AllCardEntities = new List<CardEntity>(entities);
    }
}
