using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
            item.transform.Find("EditButton").GetComponent<Button>().onClick.AddListener(() =>
            {
                Debug.Log("編集: " + deck.deckName);
                SelectedDeckData.selectedDeck = deck;
                UnityEngine.SceneManagement.SceneManager.LoadScene("DeckBuilderScene");
            });

            // 削除ボタン
            item.transform.Find("DeleteButton").GetComponent<Button>().onClick.AddListener(() =>
            {
                currentDeckList.decks.Remove(deck);
                DeckStorage.SaveDecks(currentDeckList);
                LoadAndDisplayDecks(); // 再読み込み
            });
        }
    }
    public void OnClickCreateNewDeck()
    {
        DeckData newDeck = new DeckData();
        newDeck.deckId = System.Guid.NewGuid().ToString(); // 🔥 一意なID生成
        newDeck.deckName = "新しいデッキ";
        newDeck.cardIDs = new List<int>();

        DeckDataList deckList = DeckStorage.LoadDecks();
        deckList.decks.Add(newDeck);
        DeckStorage.SaveDecks(deckList);

        SelectedDeckData.selectedDeck = newDeck;
        SceneManager.LoadScene("DeckBuilderScene");
    }
    public void OnClickBackToMainMenu()
    {
        SceneManager.LoadScene("MainMenuScene"); // 実際のシーン名に置き換えてください
    }

}
