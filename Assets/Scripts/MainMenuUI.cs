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

    void Start()
    {
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

        SceneManager.LoadScene("BattleScene");
    }
}
