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

    [SerializeField] private GameObject showDeckCanvas;
    [SerializeField] private Text deckNameText;
    [SerializeField]
    private Text deckCountText;
    [SerializeField] private GameObject zoomCanvas;
    [SerializeField] private Image zoomImage;
    public GameObject cardItemPrefab;
    [SerializeField] public Transform deckCardListContent; // デッキ内カード表示用

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

            // 表示ボタンの設定（ボタン押下時にデッキ表示Canvasで中身を表示する）
            item.transform.Find("ShowDeckButton").GetComponent<Button>().onClick.AddListener(() =>
            {
                Debug.Log("デッキ表示: " + deck.deckName);
                ShowDeckCanvas(deck);
            });

            // 編集ボタンの設定（ボタン押下時にデッキ編集画面に遷移する処理）
            item.transform.Find("EditButton").GetComponent<Button>().onClick.AddListener(() =>
            {
                Debug.Log("編集: " + deck.deckName);
                SelectedDeckData.selectedDeck = deck;
                UnityEngine.SceneManagement.SceneManager.LoadScene("DeckBuilderScene");
            });

            // 複製ボタンの設定（ボタン押下時にデッキを複製する処理）
            item.transform.Find("CopyButton").GetComponent<Button>().onClick.AddListener(() =>
            {
                Debug.Log("複製: " + deck.deckName);
                OnClickCopyDeck(deck);
            });

            // 削除ボタンの設定（ボタン押下時にデッキを削除して、デッキリスト表示を更新する処理）
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

    /** 複製ボタン押下時処理： */
    public void OnClickCopyDeck(DeckData deck)
    {
        DeckData newDeck = new DeckData();
        newDeck.deckId = System.Guid.NewGuid().ToString(); // デッキのユニークIDの生成 
        newDeck.deckName = deck.deckName + " - コピー";
        newDeck.cardIDs = deck.cardIDs;

        DeckDataList deckList = DeckStorage.LoadDecks();
        deckList.decks.Add(newDeck);
        DeckStorage.SaveDecks(deckList);

        LoadAndDisplayDecks(); // 再読み込み
    }


    /** 戻るボタン押下時処理：メインメニュー画面に戻る */
    public void OnClickBackToMainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }

    /** 表示ボタン押下時処理：デッキ表示エリアをactiveにし、デッキ内のカードリストを表示する */
    private void ShowDeckCanvas(DeckData deck)
    {
        if (showDeckCanvas != null)
        {
            Debug.Log("ShowDeckCanvas: " + deck.deckName);

            // まずカード一覧を完全に削除
            foreach (Transform child in deckCardListContent)
            {
                Destroy(child.gameObject);
            }


            // カードIDごとの枚数の算出
            var cardCountDict = new Dictionary<int, int>();
            foreach (int id in deck.cardIDs)
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

                // 枚数テキスト
                var countText = item.transform.Find("CountText")?.GetComponent<Text>();
                if (countText != null) countText.text = $"×{count}";

                // 押下時に拡大表示する
                item.GetComponent<Button>().onClick.AddListener(() =>
                {
                    ShowZoom(icon.sprite);
                });
            }


            // デッキ名表示値の更新
            if (deckNameText != null)
            {
                deckNameText.text = $"{deck.deckName}";
            }

            // デッキ枚数表示値の更新
            if (deckCountText != null)
            {
                deckCountText.text = $"現在：{deck.cardIDs.Count}枚";
            }
            showDeckCanvas.SetActive(true);
        }
    }
    public void HideDeckCanvas()
    {
        if (showDeckCanvas != null)
        {
            showDeckCanvas.SetActive(false);
        }
    }

    private void ShowZoom(Sprite sprite)
    {
        if (zoomCanvas != null && zoomImage != null)
        {
            zoomImage.sprite = sprite;
            zoomCanvas.SetActive(true);
        }
    }
    public void HideZoom()
    {
        if (zoomCanvas != null)
            zoomCanvas.SetActive(false);
    }

}
