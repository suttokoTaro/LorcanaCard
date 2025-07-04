using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class DeckBuilderUI : MonoBehaviour
{
    public Text deckNameText;

    public Transform deckCardListContent; // デッキ内カード表示用
    public Transform allCardListContent;  // 全カード一覧表示用

    public GameObject cardItemPrefab;

    public Button saveButton;
    public Button backButton;

    private DeckData currentDeck;

    void Start()
    {
        currentDeck = SelectedDeckData.selectedDeck;
        if (currentDeck == null)
        {
            Debug.LogError("デッキが選択されていません");
            return;
        }

        deckNameText.text = currentDeck.deckName;

        RefreshDeckCardList();
        GenerateAllCardList();

        saveButton.onClick.AddListener(OnSaveDeck);
        backButton.onClick.AddListener(() => SceneManager.LoadScene("DeckListScene"));
    }

    void RefreshDeckCardList()
    {
        foreach (Transform child in deckCardListContent)
            Destroy(child.gameObject);

        foreach (int cardId in currentDeck.cardIDs)
        {
            int capturedId = cardId; // ローカル変数にキャプチャ！

            GameObject item = Instantiate(cardItemPrefab, deckCardListContent);
            item.GetComponentInChildren<Text>().text = $"Card ID: {capturedId}";

            Button button = item.GetComponent<Button>();
            button.onClick.AddListener(() => {
                currentDeck.cardIDs.Remove(capturedId);
                RefreshDeckCardList();
            });
        }
    }

    void GenerateAllCardList()
    {
        // 例：ID 1001〜1010 までのカードを表示
        for (int cardId = 1001; cardId <= 1050; cardId++)
        {
            int capturedId = cardId; // ローカル変数にキャプチャ！

            GameObject item = Instantiate(cardItemPrefab, allCardListContent);
            item.GetComponentInChildren<Text>().text = $"Card ID: {capturedId}";

            Button button = item.GetComponent<Button>();
            button.onClick.AddListener(() => {
                currentDeck.cardIDs.Add(capturedId);
                RefreshDeckCardList();
            });
        }
    }

    void OnSaveDeck()
    {
        DeckDataList list = DeckStorage.LoadDecks();
        int index = list.decks.FindIndex(d => d.deckName == currentDeck.deckName);
        if (index >= 0)
            list.decks[index] = currentDeck;

        DeckStorage.SaveDecks(list);
        Debug.Log("デッキ保存完了");
    }
}
