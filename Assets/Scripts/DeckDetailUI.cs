using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.EventSystems;
using System.Linq;

public class DeckDetailUI : MonoBehaviour
{
    [SerializeField] private InputField deckNameInputField;
    [SerializeField] private Text deckCountText;
    public Text deckNameText;
    public Transform deckCardListContent; // デッキ内カード表示用
    public GameObject cardItemPrefab;
    private DeckData currentDeck;
    void Start()
    {
        // デッキリスト画面で選択したデッキ情報が取得できない場合エラーを返す
        currentDeck = SelectedDeckData.selectedDeck;
        if (currentDeck == null)
        {
            Debug.LogError("選択したデッキ情報が取得できません。");
            return;
        }
        // デッキ名入力エリアが存在する場合、取得したデッキ情報のデッキ名を設定し、編集可能とする
        if (deckNameInputField != null)
        {
            deckNameInputField.text = currentDeck.deckName;
            deckNameInputField.onEndEdit.AddListener(OnDeckNameChanged);
        }
        // デッキリストエリアの表示の更新
        RefreshDeckCardList();
    }

    /** デッキリストエリアの表示の更新 */
    void RefreshDeckCardList()
    {
        // まず一度デッキリストエリアの既存のオブジェクトをすべて削除
        foreach (Transform child in deckCardListContent)
            Destroy(child.gameObject);

        // カードIDごとの枚数の算出
        var cardCountDict = new Dictionary<int, int>();
        foreach (int id in currentDeck.cardIDs)
        {
            if (!cardCountDict.ContainsKey(id))
                cardCountDict[id] = 0;
            cardCountDict[id]++;
        }

        // カード情報と枚数をまとめたリストを作成
        List<(CardEntity entity, int count)> cardList = new List<(CardEntity, int)>();
        foreach (var pair in cardCountDict)
        {
            var entity = Resources.Load<CardEntity>($"CardEntityList/Card_{pair.Key}");
            if (entity != null)
                cardList.Add((entity, pair.Value));
        }


        // ソート：コスト昇順 → ID昇順
        cardList.Sort((a, b) =>
        {
            int costCompare = a.entity.cost.CompareTo(b.entity.cost);
            if (costCompare != 0) return costCompare;
            return a.entity.cardId.CompareTo(b.entity.cardId);
        });

        // UI生成
        foreach (var (entity, count) in cardList)
        {
            GameObject item = Instantiate(cardItemPrefab, deckCardListContent);

            // 画像
            var icon = item.transform.Find("Image")?.GetComponent<Image>();
            if (icon != null) icon.sprite = entity.icon;

            // テキスト
            var nameText = item.transform.Find("nameText")?.GetComponent<Text>();
            var countText = item.transform.Find("CountText")?.GetComponent<Text>();
            if (nameText != null) nameText.text = $"Card ID: {entity.cardId}";
            if (countText != null) countText.text = $"{count}";
        }

        // デッキ枚数表示値の更新
        if (deckCountText != null)
        {
            deckCountText.text = $"{currentDeck.cardIDs.Count}枚";
        }
    }
    /** デッキ名の変更 */
    private void OnDeckNameChanged(string newName)
    {
        if (currentDeck != null)
        {
            currentDeck.deckName = newName;
            //OnClickSaveButton();
            Debug.Log($"デッキ名変更＆保存: {newName}");
        }
    }
    /** デッキの保存 */
    public void OnClickSaveButton()
    {
        DeckDataList list = DeckStorage.LoadDecks();
        int index = list.decks.FindIndex(d => d.deckId == currentDeck.deckId);
        if (index >= 0)
        {
            list.decks[index] = currentDeck;
        }
        else
        {
            list.decks.Add(currentDeck);
        }
        DeckStorage.SaveDecks(list);
        Debug.Log("デッキ保存完了");
    }

    public void OnClickBackButton()
    {
        SceneManager.LoadScene("DecksScene");
    }
}
