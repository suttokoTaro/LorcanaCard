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

    /** 初手7枚ドロー */
    private void DrawInitialHand(List<int> deck, List<int> hand)
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

        // 降順で index を処理
        List<int> sorted = new List<int>(redraw);
        sorted.Sort((a, b) => b.CompareTo(a)); // 降順ソート

        //Debug.Log("1.引き直し対象枚数：" + sorted.Count);
        //Debug.Log("1.引き直し対象枚数：" + string.Join(", ", sorted));

        // 🔁 一時的に戻すカードを退避
        List<int> tempReturnCards = new();
        foreach (int i in sorted)
        {
            if (i >= 0 && i < hand.Count)
            {
                tempReturnCards.Add(hand[i]);
                hand.RemoveAt(i);
            }
        }
        //Debug.Log("2.退避枚数：" + tempReturnCards.Count);
        //Debug.Log("2.退避枚数：" + string.Join(", ", tempReturnCards));

        // 🔁 残りのデッキから必要な枚数をドロー
        int drawCount = tempReturnCards.Count;
        //Debug.Log("3.引き直し直前枚数：" + deck.Count);
        //Debug.Log("3.引き直し直前枚数：" + string.Join(", ", deck));

        for (int i = 0; i < drawCount && deck.Count > 0; i++)
        {
            hand.Add(deck[0]);
            deck.RemoveAt(0);
        }
        //Debug.Log("4.引き直し途中枚数：" + deck.Count);
        //Debug.Log("4.引き直し途中枚数：" + string.Join(", ", deck));

        // 🔁 一時的に退避したカードをデッキに戻してシャッフル
        deck.AddRange(tempReturnCards);
        //Debug.Log("5.引き直し直後枚数：" + deck.Count);
        //Debug.Log("5.引き直し直後枚数：" + string.Join(", ", deck));
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


    /** デッキのシャッフル */
    private void Shuffle(List<int> deck)
    {
        for (int i = 0; i < deck.Count; i++)
        {
            int rand = Random.Range(i, deck.Count);
            (deck[i], deck[rand]) = (deck[rand], deck[i]);
        }
    }

    /** カード情報の取得 */
    private CardEntity LoadCardEntity(int cardId)
    {
        return Resources.Load<CardEntity>($"CardEntityList/Card_{cardId}");
    }

    /** 引き直しボタン押下時処理：選択中のカードを引き直す */
    public void OnClickPlayerRedraw()
    {
        OnClickRedraw("Player");
    }

    /** 引き直しボタン押下時処理：選択中のカードを引き直す */
    public void OnClickEnemyRedraw()
    {
        OnClickRedraw("Enemy");
    }

    /** STARTボタン押下時処理：Battle画面に遷移 */
    public void OnClickBattleStart()
    {
        // 手札(7枚)とデッキ(53枚以上)を DeckManager に渡す
        DeckManager.Instance.playerInitialHand = new List<int>(playerHand);
        DeckManager.Instance.enemyInitialHand = new List<int>(enemyHand);
        DeckManager.Instance.selectedPlayerDeck = new List<int>(playerDeck);
        DeckManager.Instance.selectedEnemyDeck = new List<int>(enemyDeck);

        SceneManager.LoadScene("BattleScene");
    }

    /** 戻るボタン押下時処理：SimLab画面に遷移 */
    public void OnClickBackToMainMenu()
    {
        SceneManager.LoadScene("SimLabScene");
    }

    private IEnumerator ShowZoom(Sprite sprite)
    {
        yield return new WaitForSeconds(0.6f);
        if (zoomCanvas != null && zoomImage != null)
        {
            zoomImage.sprite = sprite;
            zoomCanvas.SetActive(true);
        }
    }

    /** Zoom画面押下時処理：Zoom画面を非表示にする */
    public void HideZoom()
    {
        if (zoomCanvas != null)
            zoomCanvas.SetActive(false);
    }
}