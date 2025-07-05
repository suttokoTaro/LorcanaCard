class MulliganManager : MonoBehaviour
{
    // Player
    List<int> playerDeck;
    List<int> playerHand = new List<int>();
    List<int> playerRedraw = new List<int>();
    bool playerDone = false;

    // Enemy
    List<int> enemyDeck;
    List<int> enemyHand = new List<int>();
    List<int> enemyRedraw = new List<int>();
    bool enemyDone = false;

    public Button battleStartButton;

    void Start()
    {
        // デッキ複製とシャッフル
        playerDeck = new List<int>(DeckManager.Instance.selectedPlayerDeck);
        enemyDeck = new List<int>(DeckManager.Instance.selectedEnemyDeck);
        Shuffle(playerDeck);
        Shuffle(enemyDeck);

        // それぞれ7枚引く
        DrawInitialHand(playerDeck, playerHand);
        DrawInitialHand(enemyDeck, enemyHand);

        // UIに表示
        DisplayHand("Player", playerHand);
        DisplayHand("Enemy", enemyHand);

        battleStartButton.interactable = false;
    }

    void DrawInitialHand(List<int> deck, List<int> hand)
    {
        for (int i = 0; i < 7; i++)
        {
            hand.Add(deck[0]);
            deck.RemoveAt(0);
        }
    }

    public void OnClickRedraw(string side) // "Player" or "Enemy"
    {
        if (side == "Player")
        {
            Redraw(playerRedraw, playerHand, playerDeck);
            playerDone = true;
        }
        else
        {
            Redraw(enemyRedraw, enemyHand, enemyDeck);
            enemyDone = true;
        }

        CheckBattleReady();
    }

    void Redraw(List<int> redrawList, List<int> hand, List<int> deck)
    {
        // 戻す
        foreach (var cardId in redrawList)
        {
            hand.Remove(cardId);
            deck.Add(cardId);
        }

        // 引き直し
        Shuffle(deck);
        while (hand.Count < 7 && deck.Count > 0)
        {
            hand.Add(deck[0]);
            deck.RemoveAt(0);
        }

        Shuffle(deck);
        redrawList.Clear();
    }

    void CheckBattleReady()
    {
        battleStartButton.interactable = playerDone && enemyDone;
    }

    public void OnClickCard(string side, int cardId)
    {
        // カード選択・解除
        var list = (side == "Player") ? playerRedraw : enemyRedraw;
        if (list.Contains(cardId)) list.Remove(cardId);
        else list.Add(cardId);

        UpdateCardSelectionVisuals(side);
    }

    void DisplayHand(string side, List<int> hand)
    {
        // カード表示をUIに反映（上下どちらか）
    }

    void UpdateCardSelectionVisuals(string side)
    {
        // 選択状態の表示更新
    }

    void Shuffle(List<int> deck)
    {
        for (int i = 0; i < deck.Count; i++)
        {
            int rand = Random.Range(i, deck.Count);
            (deck[i], deck[rand]) = (deck[rand], deck[i]);
        }
    }

    public void OnClickBattleStart()
    {
        // 最終手札を DeckManager.Instance に格納して BattleScene へ
        DeckManager.Instance.selectedPlayerDeck = new List<int>(playerHand);
        DeckManager.Instance.selectedEnemyDeck = new List<int>(enemyHand);
        SceneManager.LoadScene("BattleScene");
    }
}
