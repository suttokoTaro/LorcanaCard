using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/** SimLab画面 */
public class SimLabUI : MonoBehaviour
{
    public Transform decksAreaContent;
    public GameObject decksItemInSimLabPrefab;
    [SerializeField] private GameObject player1DeckPanel, player2DeckPanel;
    [SerializeField] private Text player1_DeckNameText, player2_DeckNameText;
    [SerializeField] private Image player1_DeckIcon, player1_colorIcon1, player1_colorIcon2;
    [SerializeField] private Image player2_DeckIcon, player2_colorIcon1, player2_colorIcon2;
    [SerializeField] private Button startButton;
    private DeckDataList currentDeckList;
    private bool isSortDescending = false; // 初期状態：昇順
    private DeckData player1Deck;
    private DeckData player2Deck;
    private IList<CardEntity> allCardEntities;

    /** Start */
    async void Start()
    {
        // デッキデータが存在しない場合、デフォルトデッキをロードする
        DeckStorage.EnsureDefaultDecksLoaded();

        LoadAndDisplayDecks();
        UpdateSelectedDecksDisplay();

        // ラベルでCardEntityを非同期ロード
        AsyncOperationHandle<IList<CardEntity>> handle = Addressables.LoadAssetsAsync<CardEntity>("CardEntityList", null);
        allCardEntities = await handle.Task;

        // CardEntityCacheがまだ存在しないなら生成
        if (CardEntityCache.Instance == null)
        {
            GameObject go = new GameObject("CardEntityCache");
            go.AddComponent<CardEntityCache>();
        }
        // キャッシュに保存
        CardEntityCache.Instance.SetCardEntities(allCardEntities);
    }

    /** デッキリスト表示の更新 */
    private void LoadAndDisplayDecks()
    {
        // まず一度デッキリストエリアの既存のオブジェクトをすべて削除
        foreach (Transform child in decksAreaContent) { Destroy(child.gameObject); }

        // デッキリストデータをロード
        currentDeckList = DeckStorage.LoadDecks();
        if (isSortDescending)
        {
            currentDeckList.decks = currentDeckList.decks
                .OrderBy(deck => deck.updatedAt) // 更新日時で降順ソート
                .ToList();
        }
        else
        {
            currentDeckList.decks = currentDeckList.decks
                .OrderByDescending(deck => deck.updatedAt) // 更新日時で昇順ソート
                .ToList();
        }

        foreach (var deck in currentDeckList.decks)
        {
            // decksItemInSimLabプレハブのインスタンス生成
            GameObject item = Instantiate(decksItemInSimLabPrefab, decksAreaContent);
            var deckItem = item.GetComponent<DecksItemInSimLabPrefab>();

            // デッキ名の設定
            deckItem.deckNameText.text = deck.deckName;

            // デッキアイコンの設定
            var defaultLeaderCard = Resources.Load<CardEntity>($"CardEntityList/Card_1001");
            deckItem.deckIcon.sprite = defaultLeaderCard.backIcon;
            var leaderCard = Resources.Load<CardEntity>($"CardEntityList/Card_{deck.leaderCardId}");
            if (leaderCard != null) { deckItem.deckIcon.sprite = leaderCard.icon; }

            // デッキ色アイコンの設定
            var color1 = Resources.Load<ColorEntity>($"ColorEntityList/ColorEntity_{deck.color1}");
            deckItem.colorIcon1.sprite = color1.colorIcon;
            var color2 = Resources.Load<ColorEntity>($"ColorEntityList/ColorEntity_{deck.color2}");
            deckItem.colorIcon2.sprite = color2.colorIcon;

            // デッキパネル押下時の処理
            deckItem.deckPanel.onClick.AddListener(() =>
            {
                Debug.Log("デッキ中身表示: " + deck.deckName);
            });

            // Player1ボタン押下時の処理
            deckItem.player1Button.onClick.AddListener(() =>
            {
                Debug.Log("Player1デッキ選択: " + deck.deckName);
                player1Deck = deck;
                UpdateSelectedDecksDisplay();
            });

            // Player2ボタン押下時の処理
            deckItem.player2Button.onClick.AddListener(() =>
            {
                Debug.Log("Player2デッキ選択: " + deck.deckName);
                player2Deck = deck;
                UpdateSelectedDecksDisplay();
            });
        }
    }

    void UpdateSelectedDecksDisplay()
    {
        var defaultLeaderCard = Resources.Load<CardEntity>($"CardEntityList/Card_1001");

        /** Player1エリア */
        if (player1Deck != null)
        {
            player1DeckPanel.SetActive(true);

            player1_DeckNameText.text = player1Deck.deckName;

            // デッキアイコンの設定
            player1_DeckIcon.sprite = defaultLeaderCard.backIcon;
            var player1_leaderCard = Resources.Load<CardEntity>($"CardEntityList/Card_{player1Deck.leaderCardId}");
            if (player1_leaderCard != null) { player1_DeckIcon.sprite = player1_leaderCard.icon; }

            // デッキ色アイコンの設定
            var player1_color1 = Resources.Load<ColorEntity>($"ColorEntityList/ColorEntity_{player1Deck.color1}");
            player1_colorIcon1.sprite = player1_color1.colorIcon;
            var player1_color2 = Resources.Load<ColorEntity>($"ColorEntityList/ColorEntity_{player1Deck.color2}");
            player1_colorIcon2.sprite = player1_color2.colorIcon;

            SelectedDeckData.playerDeck = player1Deck;
        }

        /** Player2エリア */
        if (player2Deck != null)
        {
            player2DeckPanel.SetActive(true);

            player2_DeckNameText.text = player2Deck.deckName;

            // デッキアイコンの設定
            player2_DeckIcon.sprite = defaultLeaderCard.backIcon;
            var player2_leaderCard = Resources.Load<CardEntity>($"CardEntityList/Card_{player2Deck.leaderCardId}");
            if (player2_leaderCard != null) { player2_DeckIcon.sprite = player2_leaderCard.icon; }

            // デッキ色アイコンの設定
            var player2_color1 = Resources.Load<ColorEntity>($"ColorEntityList/ColorEntity_{player2Deck.color1}");
            player2_colorIcon1.sprite = player2_color1.colorIcon;
            var player2_color2 = Resources.Load<ColorEntity>($"ColorEntityList/ColorEntity_{player2Deck.color2}");
            player2_colorIcon2.sprite = player2_color2.colorIcon;

            SelectedDeckData.enemyDeck = player2Deck;
        }
        startButton.interactable = player1Deck != null && player2Deck != null;

    }

    /** Player1デッキパネル押下時の処理 */
    public void OnClickPlayer1DeckPanel()
    {
        if (player1Deck != null)
        {
            player1Deck = null;
            SelectedDeckData.playerDeck = null;
            player1DeckPanel.SetActive(false);
            startButton.interactable = player1Deck != null && player2Deck != null;
        }
    }
    /** Player2デッキパネル押下時の処理 */
    public void OnClickPlayer2DeckPanel()
    {
        if (player2Deck != null)
        {
            player2Deck = null;
            SelectedDeckData.enemyDeck = null;
            player2DeckPanel.SetActive(false);
            startButton.interactable = player1Deck != null && player2Deck != null;
        }
    }


    /** STARTボタン押下時の処理 */
    public void OnClickStartButton()
    {
        if (player1Deck == null || player2Deck == null) return;

        DeckManager.Instance.selectedPlayerDeck = new List<int>(player1Deck.cardIDs);
        DeckManager.Instance.selectedEnemyDeck = new List<int>(player2Deck.cardIDs);

        DeckManager.Instance.selectedPlayer1DeckData = player1Deck;
        DeckManager.Instance.selectedPlayer2DeckData = player2Deck;

        SceneManager.LoadScene("MulliganScene");
    }

    /** CardListボタン押下時処理：CardList画面に遷移 */
    public void OnClickCardListButton()
    {
        SceneManager.LoadScene("CardListScene");
    }

    /** Decksボタン押下時処理：Decks画面に遷移 */
    public void OnClickDecksButton()
    {
        SceneManager.LoadScene("DecksScene");
    }


}
