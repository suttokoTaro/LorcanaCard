using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DecksUI : MonoBehaviour
{
    [SerializeField] private GameObject ConfirmDuplicatePanel;
    [SerializeField] private GameObject ConfirmDeletePanel;
    public Transform deckContentParent;
    public GameObject decksItemPrefab;
    private DeckDataList currentDeckList;

    void Start()
    {
        LoadAndDisplayDecks();
    }


    /** デッキリストの読み込み（表示の更新） */
    void LoadAndDisplayDecks()
    {
        // まず一度デッキリストエリアの既存のオブジェクトをすべて削除
        foreach (Transform child in deckContentParent)
        {
            Destroy(child.gameObject);
        }

        currentDeckList = DeckStorage.LoadDecks();
        foreach (var deck in currentDeckList.decks)
        {
            // deckItemプレハブのインスタンスを、デッキリストエリアに生成する
            GameObject item = Instantiate(decksItemPrefab, deckContentParent);

            // デッキ名の設定
            var deckItem = item.GetComponent<DecksItemPrefab>();
            deckItem.deckNameText.text = deck.deckName;

            // デッキアイコンの設定
            var defaultLeaderCard = Resources.Load<CardEntity>($"CardEntityList/Card_1001");
            deckItem.deckIcon.sprite = defaultLeaderCard.backIcon;
            var leaderCard = Resources.Load<CardEntity>($"CardEntityList/Card_{deck.leaderCardId}");
            if (leaderCard != null)
            {
                deckItem.deckIcon.sprite = leaderCard.icon;
            }
            // デッキ色アイコンの設定
            var color1 = Resources.Load<ColorEntity>($"ColorEntityList/ColorEntity_{deck.color1}");
            deckItem.colorIcon1.sprite = color1.colorIcon;
            var color2 = Resources.Load<ColorEntity>($"ColorEntityList/ColorEntity_{deck.color2}");
            deckItem.colorIcon2.sprite = color2.colorIcon;

            // デッキパネル押下時の処理
            deckItem.deckPanel.onClick.AddListener(() =>
            {
                Debug.Log("表示・編集: " + deck.deckName);
                SelectedDeckData.selectedDeck = deck;
                SceneManager.LoadScene("DeckDetailScene");
            });

            // 複製ボタンの設定（ボタン押下時にデッキを複製する処理）
            deckItem.duplicateButton.onClick.AddListener(() =>
            {
                Debug.Log("複製: " + deck.deckName);
                SelectedDeckData.selectedDeck = deck;
                if (ConfirmDuplicatePanel != null)
                    ConfirmDuplicatePanel.SetActive(true);
            });
            // 削除ボタンの設定（ボタン押下時にデッキを削除する処理）
            deckItem.deleteButton.onClick.AddListener(() =>
            {
                Debug.Log("削除: " + deck.deckName);
                SelectedDeckData.selectedDeck = deck;
                if (ConfirmDeletePanel != null)
                    ConfirmDeletePanel.SetActive(true);
            });
        }
    }

    /** デッキ複製確認：はいを押下時処理 */
    public void OnClickDuplicateYes()
    {
        if (SelectedDeckData.selectedDeck != null)
        {
            DeckData newDeck = new DeckData();
            newDeck.deckId = System.Guid.NewGuid().ToString(); // デッキのユニークIDの生成 
            newDeck.deckName = SelectedDeckData.selectedDeck.deckName + "_copy";
            newDeck.cardIDs = SelectedDeckData.selectedDeck.cardIDs;
            newDeck.leaderCardId = SelectedDeckData.selectedDeck.leaderCardId;
            newDeck.color1 = SelectedDeckData.selectedDeck.color1;
            newDeck.color2 = SelectedDeckData.selectedDeck.color2;

            DeckDataList deckList = DeckStorage.LoadDecks();
            deckList.decks.Add(newDeck);
            DeckStorage.SaveDecks(deckList);

            LoadAndDisplayDecks(); // 再読み込み
        }

        if (ConfirmDuplicatePanel != null)
            ConfirmDuplicatePanel.SetActive(false);
    }

    /** デッキ複製確認：いいえを押下時処理 */
    public void OnClickDuplicateNo()
    {
        if (ConfirmDuplicatePanel != null)
            ConfirmDuplicatePanel.SetActive(false);
    }

    /** デッキ削除確認：はいを押下時処理 */
    public void OnClickDeleteYes()
    {
        if (SelectedDeckData.selectedDeck != null)
        {
            currentDeckList.decks.Remove(SelectedDeckData.selectedDeck);
            DeckStorage.SaveDecks(currentDeckList);

            LoadAndDisplayDecks(); // 再読み込み
        }
        if (ConfirmDeletePanel != null)
            ConfirmDeletePanel.SetActive(false);
    }

    /** デッキ削除確認：いいえを押下時処理 */
    public void OnClickDeleteNo()
    {
        if (ConfirmDeletePanel != null)
            ConfirmDeletePanel.SetActive(false);
    }

    /** CardListボタン押下時処理：CardList画面に遷移 */
    public void OnClickCardListButton()
    {
        SceneManager.LoadScene("CardListScene");
    }
}
