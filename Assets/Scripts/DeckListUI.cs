using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckListUI : MonoBehaviour
{
    public Transform contentParent; // ScrollView の Content
    public GameObject deckItemPrefab;

    private DeckDataList currentDeckList;

    void Start()
    {
        LoadAndDisplayDecks();
    }

    void LoadAndDisplayDecks()
    {
        currentDeckList = DeckStorage.LoadDecks();

        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject); // 既存の表示を消す
        }

        foreach (var deck in currentDeckList.decks)
        {
            GameObject item = Instantiate(deckItemPrefab, contentParent);
            item.transform.Find("DeckNameText").GetComponent<Text>().text = deck.deckName;

            // 編集ボタン
            item.transform.Find("EditButton").GetComponent<Button>().onClick.AddListener(() => {
                Debug.Log("編集: " + deck.deckName);
                SelectedDeckData.selectedDeck = deck;
                UnityEngine.SceneManagement.SceneManager.LoadScene("DeckBuilderScene");
            });

            // 削除ボタン
            item.transform.Find("DeleteButton").GetComponent<Button>().onClick.AddListener(() => {
                currentDeckList.decks.Remove(deck);
                DeckStorage.SaveDecks(currentDeckList);
                LoadAndDisplayDecks(); // 再読み込み
            });
        }
    }
    public void OnClickCreateNewDeck()
    {  
        // 1. 新しい空のデッキを作成
        DeckData newDeck = new DeckData();
        newDeck.deckName = "新しいデッキ";
        newDeck.cardIDs = new List<int>(); // 空のカードリスト

        // 2. 既存のデッキを読み込み
        DeckDataList deckList = DeckStorage.LoadDecks();

        // 3. 新しいデッキを追加
        deckList.decks.Add(newDeck);

        // 4. 保存
        DeckStorage.SaveDecks(deckList);

        // 5. リストを再表示（再描画）
        LoadAndDisplayDecks();
    }
}
