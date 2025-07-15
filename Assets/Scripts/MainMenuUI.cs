using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MainMenuUI : MonoBehaviour
{
    public Transform deckListParent; // デッキ一覧の親（ScrollViewのContentなど）
    public GameObject deckListItemPrefab; // デッキ1つ分のプレハブ
    public Text playerDeckText;
    public Text enemyDeckText;
    public Button battleButton;

    private DeckData playerDeck;
    private DeckData enemyDeck;
    private DeckDataList allDecks;

    [SerializeField] public Transform deckCardListContent; // デッキ内カード表示用
    [SerializeField] private GameObject showDeckCanvas;
    [SerializeField] private Text deckNameText;
    [SerializeField] private Text deckCountText;
    public GameObject cardItemPrefab;
    [SerializeField] private GameObject zoomCanvas;
    [SerializeField] private Image zoomImage;
    void Start()
    {
        Debug.Log("PersistentDataPath: " + Application.persistentDataPath);
        DeckStorage.EnsureDefaultDecksLoaded(); // ← これを先頭に追加！
        Debug.Log("Start(): " + this.name + " deckListParent = " + (deckListParent == null ? "NULL" : deckListParent.name));
        LoadDecksAndDisplay();
        UpdateSelectedDecksDisplay();
    }

    void LoadDecksAndDisplay()
    {
        allDecks = DeckStorage.LoadDecks();

        foreach (Transform child in deckListParent)
            Destroy(child.gameObject);

        foreach (var deck in allDecks.decks)
        {
            GameObject item = Instantiate(deckListItemPrefab, deckListParent);
            item.transform.Find("DeckNameText").GetComponent<Text>().text = deck.deckName;

            item.transform.Find("ShowDeckButton").GetComponent<Button>().onClick.AddListener(() =>
            {
                Debug.Log("デッキ表示: " + deck.deckName);
                ShowDeckCanvas(deck);
            });

            item.transform.Find("PlayerDeckButton").GetComponent<Button>().onClick.AddListener(() =>
            {
                playerDeck = deck;
                UpdateSelectedDecksDisplay();
            });

            item.transform.Find("EnemyDeckButton").GetComponent<Button>().onClick.AddListener(() =>
            {
                enemyDeck = deck;
                UpdateSelectedDecksDisplay();
            });
        }
    }

    void UpdateSelectedDecksDisplay()
    {
        playerDeckText.text = playerDeck != null ? "PlayerDeck：" + playerDeck.deckName : "PlayerDeck：未選択";
        enemyDeckText.text = enemyDeck != null ? "EnemyDeck：" + enemyDeck.deckName : "EnemyDeck：未選択";
        battleButton.interactable = playerDeck != null && enemyDeck != null;

        // オプション: 対戦時に使うデッキ情報を保存しておく
        SelectedDeckData.playerDeck = playerDeck;
        SelectedDeckData.enemyDeck = enemyDeck;
    }

    public void OnClickDeckBuilder()
    {
        SceneManager.LoadScene("DeckListScene");
    }
    public void OnClickCardList()
    {
        SceneManager.LoadScene("CardListScene");
    }

    // public void OnClickBattle()
    // {
    //     DeckManager.Instance.selectedPlayerDeck = new List<int> { 1002, 1002, 1051, 1051, 1113, 1113 }; // プレイヤーデッキ
    //     DeckManager.Instance.selectedEnemyDeck = new List<int> { 3001, 3001, 1113, 1113, 1051, 1051 }; // 敵デッキ

    //     SceneManager.LoadScene("BattleScene");
    // }

    public void OnClickBattle()
    {
        if (playerDeck == null || enemyDeck == null) return;

        DeckManager.Instance.selectedPlayerDeck = new List<int>(playerDeck.cardIDs);
        DeckManager.Instance.selectedEnemyDeck = new List<int>(enemyDeck.cardIDs);

        //SceneManager.LoadScene("BattleScene");
        SceneManager.LoadScene("MulliganScene");
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
