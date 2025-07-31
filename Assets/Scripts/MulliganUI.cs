using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

class MulliganManager : MonoBehaviour
{
    [SerializeField] private GameObject zoomCanvas;
    [SerializeField] private Image zoomImage;

    [SerializeField] private Image player1BackIcon, player2BackIcon;
    private Coroutine zoomCoroutine;

    private IEnumerator ShowZoom(Sprite sprite)
    {
        yield return new WaitForSeconds(0.6f);
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

    [Header("Player")]
    public Transform playerHandArea;
    public Button playerRedrawButton;
    [Header("Enemy")]
    public Transform enemyHandArea;
    public Button enemyRedrawButton;

    [Header("共通")]
    public GameObject cardViewPrefab; // ← カード表示用プレハブ
    public Button battleStartButton;

    private List<int> playerDeck;
    private List<int> enemyDeck;
    private List<int> playerHand = new();
    private List<int> enemyHand = new();
    private List<int> playerRedraw = new();
    private List<int> enemyRedraw = new();

    private bool playerDone = false;
    private bool enemyDone = false;
    void Start()
    {
        // デッキ取得・シャッフル
        playerDeck = new List<int>(DeckManager.Instance.selectedPlayerDeck);
        enemyDeck = new List<int>(DeckManager.Instance.selectedEnemyDeck);
        Shuffle(playerDeck);
        Shuffle(enemyDeck);

        // 初手7枚ドロー
        DrawInitialHand(playerDeck, playerHand);
        DrawInitialHand(enemyDeck, enemyHand);

        // 表示
        DisplayHand(playerHandArea, playerHand, "Player");
        DisplayHand(enemyHandArea, enemyHand, "Enemy");

        var defaultLeaderCard = Resources.Load<CardEntity>($"CardEntityList/Card_1001");

        player1BackIcon.sprite = defaultLeaderCard.backIcon;
        var player1_leaderCard = Resources.Load<CardEntity>($"CardEntityList/Card_{DeckManager.Instance.selectedPlayer1DeckData.leaderCardId}");
        if (player1_leaderCard != null) { player1BackIcon.sprite = player1_leaderCard.icon; }

        player2BackIcon.sprite = defaultLeaderCard.backIcon;
        var player2_leaderCard = Resources.Load<CardEntity>($"CardEntityList/Card_{DeckManager.Instance.selectedPlayer2DeckData.leaderCardId}");
        if (player2_leaderCard != null) { player2BackIcon.sprite = player2_leaderCard.icon; }

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
    void DisplayHand(Transform parent, List<int> hand, string side)
    {
        // 現在の選択リスト
        List<int> redrawList = side == "Player" ? playerRedraw : enemyRedraw;

        // 表示を一度全削除
        foreach (Transform child in parent)
            Destroy(child.gameObject);

        for (int i = 0; i < hand.Count; i++)
        {
            int cardId = hand[i];
            GameObject card = Instantiate(cardViewPrefab, parent);

            // カード画像表示
            CardEntity entity = LoadCardEntity(cardId);
            if (entity != null)
            {
                var image = card.GetComponent<Image>();
                if (image != null && entity.icon != null)
                {
                    image.sprite = entity.icon;
                    image.color = Color.white;
                }
            }
            // 🔽 長押しズーム用 EventTrigger を追加
            if (entity != null && entity.icon != null)
            {
                EventTrigger trigger = card.AddComponent<EventTrigger>();

                var down = new EventTrigger.Entry();
                down.eventID = EventTriggerType.PointerDown;
                down.callback.AddListener((eventData) =>
                {
                    zoomCoroutine = StartCoroutine(ShowZoom(entity.icon));
                });
                trigger.triggers.Add(down);

                var up = new EventTrigger.Entry();
                up.eventID = EventTriggerType.PointerUp;
                up.callback.AddListener((eventData) =>
                {
                    if (zoomCoroutine != null)
                        StopCoroutine(zoomCoroutine);
                    //HideZoom();
                });
                trigger.triggers.Add(up);
            }


            // 🔑 index で管理
            int cardIndex = i;
            Button button = card.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() =>
                {
                    ToggleRedraw(side, cardIndex);
                    UpdateCardVisuals(parent, hand, side);
                });
            }
        }

        UpdateCardVisuals(parent, hand, side);
    }

    void ToggleRedraw(string side, int index)
    {
        List<int> list = side == "Player" ? playerRedraw : enemyRedraw;
        if (list.Contains(index))
            list.Remove(index);
        else
            list.Add(index);
    }

    void UpdateCardVisuals(Transform parent, List<int> hand, string side)
    {
        List<int> redrawList = side == "Player" ? playerRedraw : enemyRedraw;

        int index = 0;
        foreach (Transform card in parent)
        {
            if (index >= hand.Count) break;

            var image = card.GetComponent<Image>();
            if (image != null)
            {
                bool isSelected = redrawList.Contains(index);
                image.color = isSelected ? Color.gray : Color.white;
            }

            index++;
        }
    }

    public void OnClickRedraw(string side)
    {
        List<int> deck = side == "Player" ? playerDeck : enemyDeck;
        List<int> hand = side == "Player" ? playerHand : enemyHand;
        List<int> redraw = side == "Player" ? playerRedraw : enemyRedraw;

        // 🔁 降順で index を処理
        List<int> sorted = new List<int>(redraw);
        sorted.Sort((a, b) => b.CompareTo(a)); // 降順ソート

        foreach (int i in sorted)
        {
            if (i >= 0 && i < hand.Count)
            {
                deck.Add(hand[i]);       // 手札からデッキへ戻す
                hand.RemoveAt(i);        // 手札から削除
            }
        }

        // 引き直し
        Shuffle(deck);
        while (hand.Count < 7 && deck.Count > 0)
        {
            hand.Add(deck[0]);
            deck.RemoveAt(0);
        }
        Shuffle(deck);

        // 状態更新
        redraw.Clear();

        if (side == "Player")
        {
            playerDone = true;
            DisplayHand(playerHandArea, playerHand, "Player");
        }
        else
        {
            enemyDone = true;
            DisplayHand(enemyHandArea, enemyHand, "Enemy");
        }

        battleStartButton.interactable = playerDone && enemyDone;
    }

    void Shuffle(List<int> deck)
    {
        for (int i = 0; i < deck.Count; i++)
        {
            int rand = Random.Range(i, deck.Count);
            (deck[i], deck[rand]) = (deck[rand], deck[i]);
        }
    }

    private CardEntity LoadCardEntity(int cardId)
    {
        return Resources.Load<CardEntity>($"CardEntityList/Card_{cardId}");
    }
    public void OnClickPlayerRedraw()
    {
        OnClickRedraw("Player");
    }

    public void OnClickEnemyRedraw()
    {
        OnClickRedraw("Enemy");
    }

    public void OnClickBattleStart()
    {
        // 手札とデッキを DeckManager に渡す
        DeckManager.Instance.playerInitialHand = new List<int>(playerHand);
        DeckManager.Instance.enemyInitialHand = new List<int>(enemyHand);
        DeckManager.Instance.selectedPlayerDeck = new List<int>(playerDeck);
        DeckManager.Instance.selectedEnemyDeck = new List<int>(enemyDeck);

        // シーン遷移
        SceneManager.LoadScene("BattleScene");
    }

    public void OnClickBackToMainMenu()
    {
        //SceneManager.LoadScene("MainMenuScene");
        SceneManager.LoadScene("SimLabScene");
    }
}