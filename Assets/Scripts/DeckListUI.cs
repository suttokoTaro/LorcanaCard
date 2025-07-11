using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/** デッキリスト画面 */
public class DeckListUI : MonoBehaviour
{
    public Transform contentParent; // ScrollView の Content
    public GameObject deckItemPrefab;

    private DeckDataList currentDeckList;

    void Start()
    {
        LoadAndDisplayDecks();
    }

    /** デッキリストの読み込み（表示の更新） */
    void LoadAndDisplayDecks()
    {
        // まず一度デッキリストエリアの既存のオブジェクトをすべて削除
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        currentDeckList = DeckStorage.LoadDecks();
        foreach (var deck in currentDeckList.decks)
        {
            // deckItemプレハブのインスタンスを、デッキリストエリアに生成する
            GameObject item = Instantiate(deckItemPrefab, contentParent);

            // デッキ名の設定
            item.transform.Find("DeckNameText").GetComponent<Text>().text = deck.deckName;

            // 編集ボタンの設定（ボタン押下時にデッキ編集画面に遷移する処理）
            item.transform.Find("EditButton").GetComponent<Button>().onClick.AddListener(() =>
            {
                Debug.Log("編集: " + deck.deckName);
                SelectedDeckData.selectedDeck = deck;
                UnityEngine.SceneManagement.SceneManager.LoadScene("DeckBuilderScene");
            });

            // 削除ボタンの設定（ボタン押下時にデッキを削除して、デッキリスト表示を更新する処理
            item.transform.Find("DeleteButton").GetComponent<Button>().onClick.AddListener(() =>
            {
                currentDeckList.decks.Remove(deck);
                DeckStorage.SaveDecks(currentDeckList);
                LoadAndDisplayDecks(); // 再読み込み
            });
        }
    }

    /** 新規作成ボタン押下時処理：新しいデッキを作成し、デッキ編集画面に遷移する */
    public void OnClickCreateNewDeck()
    {
        DeckData newDeck = new DeckData();
        newDeck.deckId = System.Guid.NewGuid().ToString(); // デッキのユニークIDの生成 
        newDeck.deckName = "新しいデッキ";
        newDeck.cardIDs = new List<int>();

        DeckDataList deckList = DeckStorage.LoadDecks();
        deckList.decks.Add(newDeck);
        DeckStorage.SaveDecks(deckList);

        SelectedDeckData.selectedDeck = newDeck;
        SceneManager.LoadScene("DeckBuilderScene");
    }

    /** 戻るボタン押下時処理：メインメニュー画面に戻る */
    public void OnClickBackToMainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }

}
