using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class BattleUI : MonoBehaviour
{
    [SerializeField] CardController cardPrefab;
    [SerializeField] DeckCardListInBattleUIPrefab deckCardPrefab;
    [SerializeField] Transform playerHandArea, enemyHandArea, playerDeckArea, enemyDeckArea, playerDeckMenuArea, enemyDeckMenuArea;
    [SerializeField] Text playerLoaPointText, enemyLoaPointText;
    [SerializeField] Text playerDeckCountText, enemyDeckCountText, player1DeckMenuCountText;
    [SerializeField] private GameObject confirmExitPanel;
    [SerializeField] private GameObject zoomCanvas;
    [SerializeField] private Image zoomImage;
    [SerializeField] public Canvas deckMenuCanvas; // Inspector でセット

    private Coroutine zoomCoroutine;
    private Coroutine deckMenuCoroutine;
    public int playerLoaPoint, enemyLoaPoint;

    bool isPlayerTurn = true;
    List<int> playerDeckList = new List<int>() { };
    List<int> enemyDeckList = new List<int>() { };
    public static BattleUI Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        // ロア値の初期表示
        playerLoaPoint = 0;
        enemyLoaPoint = 0;
        ShowLoaPoint();

        // DeckManagerから受け取ったカードをセットする
        if (DeckManager.Instance != null)
        {
            // 初期手札を除くデッキをデッキエリアにセットする
            playerDeckList = new List<int>(DeckManager.Instance.selectedPlayerDeck);
            enemyDeckList = new List<int>(DeckManager.Instance.selectedEnemyDeck);

            for (int i = playerDeckList.Count - 1; i >= 0; i--)
            {
                int cardId = playerDeckList[i];
                CreateBackIconCard(cardId, playerDeckArea);
            }
            for (int i = 0; i < playerDeckList.Count; i++)
            {
                int cardId = playerDeckList[i];
                CreateDeckCard(cardId, playerDeckMenuArea);
            }
            for (int i = enemyDeckList.Count - 1; i >= 0; i--)
            {
                int cardId = enemyDeckList[i];
                CreateBackIconCard(cardId, enemyDeckArea);
            }

            // 初期手札をハンドエリアにセットする
            foreach (int cardId in DeckManager.Instance.playerInitialHand)
            {
                CreateFrontIconCard(cardId, playerHandArea);
            }
            foreach (int cardId in DeckManager.Instance.enemyInitialHand)
            {
                CreateFrontIconCard(cardId, enemyHandArea);
            }
        }
    }


    /** 表画像表示のカードの生成 */
    void CreateFrontIconCard(int cardID, Transform place)
    {
        CardController card = Instantiate(cardPrefab, place);
        card.CreateCardAndViewIcon(cardID);

        // ズーム表示の EventTrigger を追加
        EventTrigger trigger = card.gameObject.AddComponent<EventTrigger>();

        // === PointerDown: 長押し検出で表示開始 ===
        var down = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        down.callback.AddListener((eventData) =>
        {
            // 親エリア名で Deck にいるか判定
            string parentName = card.transform.parent?.name.ToLower();
            bool isDeck = parentName != null && parentName.Contains("deck");
            if (isDeck)
            {
                deckMenuCoroutine = StartCoroutine(ShowDeckMenu(card)); // ✅ Deck メニュー表示（Zoomはしない）
            }
            else
            {
                CardEntity entity = Resources.Load<CardEntity>($"CardEntityList/Card_{cardID}");
                if (entity?.icon != null)
                    zoomCoroutine = StartCoroutine(ShowZoom(entity.icon));
            }
        });
        trigger.triggers.Add(down);

        // === BeginDrag: 長押し中に移動開始した場合、コルーチンを中止する ===
        var beginDrag = new EventTrigger.Entry { eventID = EventTriggerType.BeginDrag };
        beginDrag.callback.AddListener((eventData) =>
        {
            if (zoomCoroutine != null)
            {
                StopCoroutine(zoomCoroutine);
                zoomCoroutine = null;
            }
            if (deckMenuCoroutine != null)
            {
                StopCoroutine(deckMenuCoroutine);
                deckMenuCoroutine = null;
            }
        });
        trigger.triggers.Add(beginDrag);

        // === PointerUp: 表示を閉じない（StopCoroutine のみ） ===
        var up = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        up.callback.AddListener((eventData) =>
        {
            if (zoomCoroutine != null)
            {
                StopCoroutine(zoomCoroutine);
                zoomCoroutine = null;
            }
            if (deckMenuCoroutine != null)
            {
                StopCoroutine(deckMenuCoroutine);
                deckMenuCoroutine = null;
            }
        });
        trigger.triggers.Add(up);

        // デッキ枚数表示値の更新処理
        UpdateDeckCountText();
    }

    /** 裏画像表ののカードの生成 */
    void CreateBackIconCard(int cardID, Transform place)
    {
        CardController card = Instantiate(cardPrefab, place);
        card.CreateCardAndViewBackIcon(cardID);

        // ズーム表示の EventTrigger を追加
        EventTrigger trigger = card.gameObject.AddComponent<EventTrigger>();

        var down = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        down.callback.AddListener((eventData) =>
        {
            // 親エリア名で Deck にいるか判定
            string parentName = card.transform.parent?.name.ToLower();
            bool isDeck = parentName != null && parentName.Contains("deck");
            if (isDeck)
            {
                deckMenuCoroutine = StartCoroutine(ShowDeckMenu(card)); // ✅ Deck メニュー表示（Zoomはしない）
            }
            else
            {
                CardEntity entity = Resources.Load<CardEntity>($"CardEntityList/Card_{cardID}");
                if (entity?.icon != null)
                    zoomCoroutine = StartCoroutine(ShowZoom(entity.icon));
            }
        });
        trigger.triggers.Add(down);

        var up = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        up.callback.AddListener((eventData) =>
        {
            if (zoomCoroutine != null)
            {
                StopCoroutine(zoomCoroutine);
                zoomCoroutine = null;
            }
            if (deckMenuCoroutine != null)
            {
                StopCoroutine(deckMenuCoroutine);
                deckMenuCoroutine = null;
            }
        });
        trigger.triggers.Add(up);

        // === BeginDrag: 長押し中に移動開始した場合、コルーチンを中止する ===
        var beginDrag = new EventTrigger.Entry { eventID = EventTriggerType.BeginDrag };
        beginDrag.callback.AddListener((eventData) =>
        {
            if (zoomCoroutine != null)
            {
                StopCoroutine(zoomCoroutine);
                zoomCoroutine = null;
            }
            if (deckMenuCoroutine != null)
            {
                StopCoroutine(deckMenuCoroutine);
                deckMenuCoroutine = null;
            }
        });
        trigger.triggers.Add(beginDrag);

        // デッキ枚数表示値の更新処理
        UpdateDeckCountText();
    }

    // デッキメニューエリアの更新
    void CreateDeckCard(int cardID, Transform place)
    {
        DeckCardListInBattleUIPrefab deckCard = Instantiate(deckCardPrefab, place);
        deckCard.createDeckCardFront(cardID);
        deckCard.deckPanel.onClick.AddListener(() =>
            {
                deckCard.changeFrontAndBack();
            });

        deckCard.changeFrontAndBack();
    }


    /** デッキ枚数表示値の更新処理 */
    public void UpdateDeckCountText()
    {
        if (playerDeckCountText != null)
            //playerDeckCountText.text = $"枚数: {playerDeckList.Count}";
            playerDeckCountText.text = $"Deck({playerDeckArea.childCount})";
        if (player1DeckMenuCountText != null)
            player1DeckMenuCountText.text = $"DECK ({playerDeckArea.childCount})";
        if (enemyDeckCountText != null)
            //enemyDeckCountText.text = $"枚数: {enemyDeckList.Count}";
            enemyDeckCountText.text = $"Deck({enemyDeckArea.childCount})";
    }



    public void ShuffleDeckArea(Transform deckArea)
    {
        int childCount = deckArea.childCount;

        // 子オブジェクトを一時リストに取得
        List<Transform> cards = new List<Transform>();
        for (int i = 0; i < childCount; i++)
        {
            cards.Add(deckArea.GetChild(i));
        }

        // シャッフル
        for (int i = 0; i < cards.Count; i++)
        {
            int randIndex = Random.Range(i, cards.Count);
            var temp = cards[i];
            cards[i] = cards[randIndex];
            cards[randIndex] = temp;
        }

        // 並び替えた順に SetAsLastSibling で再配置
        foreach (var card in cards)
        {
            card.SetAsLastSibling();
        }

        Debug.Log("Deck shuffled visually!");
    }
    public void MoveTopDeckCardToBottom(Transform deckArea)
    {
        int count = deckArea.childCount;
        if (count <= 1) return;
        // 一番上のカード（＝最後の子）を取得
        Transform topCard = deckArea.GetChild(count - 1);
        // 一番下に移動
        topCard.SetAsFirstSibling();

        Debug.Log("Moved top deck card to bottom.");
    }

    public void RemoveTopDeckMenuCard(Transform deckArea)
    {
        if (deckArea.name.ToLower().Contains("playerdeck"))
        {
            int count = playerDeckMenuArea.childCount;
            Transform topDeckCard = playerDeckMenuArea.GetChild(0);
            Destroy(topDeckCard.gameObject);
        }
    }




    /** ロア値の加減ボタン押下時の処理 */
    void ShowLoaPoint()
    {
        playerLoaPointText.text = playerLoaPoint.ToString();
        enemyLoaPointText.text = enemyLoaPoint.ToString();
    }
    public void PlayerLoaPointPlus()
    {
        playerLoaPoint = playerLoaPoint + 1;
        ShowLoaPoint();
    }
    public void PlayerLoaPointMinus()
    {
        playerLoaPoint = playerLoaPoint - 1;
        ShowLoaPoint();
    }
    public void EnemyLoaPointPlus()
    {
        enemyLoaPoint = enemyLoaPoint + 1;
        ShowLoaPoint();
    }
    public void EnemyLoaPointMinus()
    {
        enemyLoaPoint = enemyLoaPoint - 1;
        ShowLoaPoint();
    }


    /** 対戦終了ボタン押下時の処理 */
    public void OnClickEndBattleButton()
    {
        if (confirmExitPanel != null)
            confirmExitPanel.SetActive(true);
    }
    public void OnClickConfirmExitYes()
    {
        //SceneManager.LoadScene("MainMenuScene");
        SceneManager.LoadScene("SimLabScene");
    }
    public void OnClickConfirmExitNo()
    {
        if (confirmExitPanel != null)
            confirmExitPanel.SetActive(false);
    }

    /** デッキニューCanvasの表示 */
    private IEnumerator ShowDeckMenu(CardController card)
    {
        yield return new WaitForSeconds(0.6f);
        //deckMenuCanvas.SetActive(true);
        deckMenuCanvas.sortingOrder = 10;
        // 必要に応じて cardId やカード参照を保存
    }

    /** デッキニューCanvasの非表示 */
    public void HideDeckMenu()
    {
        //deckMenuCanvas.SetActive(false);
        deckMenuCanvas.sortingOrder = -10;
    }

    /** ZoomCanvasの表示 */
    private IEnumerator ShowZoom(Sprite sprite)
    {
        yield return new WaitForSeconds(0.6f);
        if (zoomCanvas != null && zoomImage != null)
        {
            zoomImage.sprite = sprite;
            zoomCanvas.SetActive(true);
        }
    }

    /** ZoomCanvasの非表示 */
    public void HideZoom()
    {
        if (zoomCanvas != null)
            zoomCanvas.SetActive(false);
    }
}
