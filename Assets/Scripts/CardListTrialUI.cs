using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.EventSystems;
using System.Linq;

public class CardListTrialUI : MonoBehaviour
{
    [SerializeField] private GameObject cardItemPrefab;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform content;
    [SerializeField] private float itemHeight = 280f; // カードの高さ
    [SerializeField] private int visibleItemCount = 25; // 同時表示上限
    private List<GameObject> pooledItems = new List<GameObject>();
    private List<CardEntity> currentFilteredCards = new List<CardEntity>();

    private List<CardEntity> cachedAllEntities = new List<CardEntity>();
    private bool cardDataLoaded = false;

    IEnumerator Start()
    {

        for (int i = 0; i < 100; i++)
        {
            GameObject item = Instantiate(cardItemPrefab, content);

        }
        // カードデータの読み込み（1フレーム遅延付き）
        yield return StartCoroutine(LoadCardEntities());






        // フィルター初期化後、カード選択エリアを表示
        //RefreshSelectableCardList();


        /**
        // 全カード読み込み
        CardEntity[] allCardEntities = Resources.LoadAll<CardEntity>("CardEntityList");
        List<CardEntity> filteredCardEntities = new List<CardEntity>();
        foreach (CardEntity cardEntity in allCardEntities)
        {
            filteredCardEntities.Add(cardEntity);
        }
        foreach (CardEntity cardEntity in filteredCardEntities)
        {
            int cardId = cardEntity.cardId;
            GameObject item = Instantiate(cardItemPrefab, content);

            // 画像表示
            Image iconImage = item.transform.Find("Image")?.GetComponent<Image>();
            if (iconImage != null) iconImage.sprite = cardEntity.icon;
        }
        */

        //scrollRect.onValueChanged.AddListener((_) => UpdateVisibleCards());
        //InitObjectPool();
        //RefreshCardList();

    }

    private IEnumerator LoadCardEntities()
    {
        yield return null; // 初期化待ち（1フレーム遅延）

        cachedAllEntities = new List<CardEntity>(Resources.LoadAll<CardEntity>("CardEntityList"));
        cardDataLoaded = true;
        Debug.LogWarning("読み込み完了：" + cachedAllEntities.Count);
    }

    private void RefreshCardList()
    {
        // フィルター処理だけ行い、表示は後で
        currentFilteredCards = FilterCards();

        float contentHeight = itemHeight * currentFilteredCards.Count;
        content.sizeDelta = new Vector2(content.sizeDelta.x, contentHeight);

        UpdateVisibleCards();
    }

    private List<CardEntity> FilterCards()
    {
        // 全カード読み込み
        CardEntity[] allCardEntities = Resources.LoadAll<CardEntity>("CardEntityList");
        List<CardEntity> filteredCardEntities = new List<CardEntity>();
        foreach (CardEntity cardEntity in allCardEntities)
        {
            filteredCardEntities.Add(cardEntity);
        }
        return filteredCardEntities;
    }

    private void InitObjectPool()
    {
        for (int i = 0; i < visibleItemCount; i++)
        {
            GameObject item = Instantiate(cardItemPrefab, content);
            item.SetActive(false);
            pooledItems.Add(item);
        }
    }
    private void ApplyCardDataToItem(GameObject item, CardEntity cardEntity)
    {
        CardItemUITrial ui = item.GetComponent<CardItemUITrial>();
        if (ui != null)
        {
            ui.SetCard(cardEntity);
        }
    }

    private void UpdateVisibleCards()
    {
        float scrollY = content.anchoredPosition.y;
        int startIndex = Mathf.FloorToInt(scrollY / itemHeight) * 5 + 1;
        Debug.Log("StartIndex: " + startIndex);

        for (int i = 0; i < pooledItems.Count; i++)
        {
            int dataIndex = startIndex + i;
            if (dataIndex >= 0 && dataIndex < currentFilteredCards.Count)
            {
                GameObject item = pooledItems[i];
                item.SetActive(true);
                //item.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -dataIndex * itemHeight);
                ApplyCardDataToItem(item, currentFilteredCards[dataIndex]);
            }
            else
            {
                pooledItems[i].SetActive(false);
            }
        }
    }

}
