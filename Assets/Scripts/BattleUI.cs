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
    [SerializeField] Transform playerHandArea, enemyHandArea, playerDeckArea, enemyDeckArea, playerTrushArea, enemyTrushArea, deckMenuArea;
    [SerializeField] Text playerLoaPointText, enemyLoaPointText;
    [SerializeField] Text playerDeckCountText, enemyDeckCountText, deckMenuCountText;
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
            // for (int i = 0; i < playerDeckList.Count; i++)
            // {
            //     int cardId = playerDeckList[i];
            //     CreatePlayerDeckMenuCard(cardId, playerDeckArea.name.ToLower());
            // }
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
        UpdateDeckCountText();
    }


    /** 表画像表示のカードの生成 */
    void CreateFrontIconCard(int cardID, Transform place)
    {
        CardController card = Instantiate(cardPrefab, place);
        card.CreateCardAndViewIcon(cardID);
        card.Initialize(this);

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
                refreshDeckMenuCardList(parentName);
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
        //UpdateDeckCountText();
    }

    /** 裏画像表ののカードの生成 */
    void CreateBackIconCard(int cardID, Transform place)
    {
        CardController card = Instantiate(cardPrefab, place);
        card.CreateCardAndViewBackIcon(cardID);
        card.Initialize(this);

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
                refreshDeckMenuCardList(parentName);
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
        //UpdateDeckCountText();
    }

    /** デッキメニューカードリストの更新 */
    private void refreshDeckMenuCardList(string areaName)
    {
        // まず一度既存のオブジェクトをすべて削除
        foreach (Transform child in deckMenuArea)
        {
            Destroy(child.gameObject);
        }

        if (areaName.Contains("playerdeck"))
        {
            for (int i = playerDeckArea.childCount - 1; i >= 0; i--)
            {
                Transform childTransform = playerDeckArea.GetChild(i);
                CardController child = childTransform.GetComponent<CardController>();

                //Debug.Log("子要素の名前: " + child.model.cardId);
                DeckCardListInBattleUIPrefab deckCard = Instantiate(deckCardPrefab, deckMenuArea);
                deckCard.isPlayer1 = true;
                deckCard.createDeckCardFront(child.model.cardId);
                deckCard.deckCardPanel.onClick.AddListener(() => { deckCard.changeFrontAndBack(); });
                deckCard.handButton.onClick.AddListener(() => { OnClickHandButton(deckCard); });
                deckCard.trushButton.onClick.AddListener(() => { OnClickTrushButton(deckCard); });
                deckCard.bottomButton.onClick.AddListener(() => { OnClickBottomButton(deckCard); });
                deckCard.changeFrontAndBack();
            }
            deckMenuCountText.text = $"DECK({playerDeckArea.childCount})";
            UpdateDeckCountText();
        }

        if (areaName.Contains("enemydeck"))
        {
            for (int i = enemyDeckArea.childCount - 1; i >= 0; i--)
            {
                Transform childTransform = enemyDeckArea.GetChild(i);
                CardController child = childTransform.GetComponent<CardController>();

                //Debug.Log("子要素の名前: " + child.model.cardId);
                DeckCardListInBattleUIPrefab deckCard = Instantiate(deckCardPrefab, deckMenuArea);
                deckCard.isPlayer1 = false;
                deckCard.createDeckCardFront(child.model.cardId);
                deckCard.deckCardPanel.onClick.AddListener(() => { deckCard.changeFrontAndBack(); });
                deckCard.handButton.onClick.AddListener(() => { OnClickHandButton(deckCard); });
                deckCard.trushButton.onClick.AddListener(() => { OnClickTrushButton(deckCard); });
                deckCard.bottomButton.onClick.AddListener(() => { OnClickBottomButton(deckCard); });
                deckCard.changeFrontAndBack();
            }
            deckMenuCountText.text = $"DECK({enemyDeckArea.childCount})";
            UpdateDeckCountText();
        }
    }

    /** HANDボタン押下時の処理 */
    private void OnClickHandButton(DeckCardListInBattleUIPrefab deckCard)
    {
        int index = deckCard.transform.GetSiblingIndex();
        // Debug.Log("handボタン：" + index);
        // Debug.Log("handボタン：" + deckCard.isPlayer1);

        if (deckCard.isPlayer1)
        {
            int reverseIndex = playerDeckArea.childCount - index;

            Transform childTransform = playerDeckArea.GetChild(reverseIndex - 1);
            CardController cardCtrl = childTransform.GetComponent<CardController>();
            cardCtrl.view.ShowIcon(cardCtrl.model);
            childTransform.SetParent(playerHandArea);
            //refreshDeckMenuCardList(playerDeckArea.name.ToLower());
            Destroy(deckMenuArea.GetChild(index).gameObject);
        }
        if (!deckCard.isPlayer1)
        {
            int reverseIndex = enemyDeckArea.childCount - index;

            Transform childTransform = enemyDeckArea.GetChild(reverseIndex - 1);
            CardController cardCtrl = childTransform.GetComponent<CardController>();
            cardCtrl.view.ShowIcon(cardCtrl.model);
            childTransform.SetParent(enemyHandArea);
            //refreshDeckMenuCardList(enemyDeckArea.name.ToLower());
            Destroy(deckMenuArea.GetChild(index).gameObject);
        }
    }

    /** TRUSHボタン押下時の処理 */
    private void OnClickTrushButton(DeckCardListInBattleUIPrefab deckCard)
    {
        int index = deckCard.transform.GetSiblingIndex();


        if (deckCard.isPlayer1)
        {
            int reverseIndex = playerDeckArea.childCount - index;

            Transform childTransform = playerDeckArea.GetChild(reverseIndex - 1);
            CardController cardCtrl = childTransform.GetComponent<CardController>();
            cardCtrl.view.ShowIcon(cardCtrl.model);
            childTransform.SetParent(playerTrushArea);
            //refreshDeckMenuCardList(playerDeckArea.name.ToLower());
            Destroy(deckMenuArea.GetChild(index).gameObject);
        }
        if (!deckCard.isPlayer1)
        {
            int reverseIndex = enemyDeckArea.childCount - index;

            Transform childTransform = enemyDeckArea.GetChild(reverseIndex - 1);
            CardController cardCtrl = childTransform.GetComponent<CardController>();
            cardCtrl.view.ShowIcon(cardCtrl.model);
            childTransform.SetParent(enemyTrushArea);
            //refreshDeckMenuCardList(enemyDeckArea.name.ToLower());
            Destroy(deckMenuArea.GetChild(index).gameObject);
        }
    }

    /** BOTTOMボタン押下時の処理 */
    private void OnClickBottomButton(DeckCardListInBattleUIPrefab deckCard)
    {
        int index = deckCard.transform.GetSiblingIndex();

        if (deckCard.isPlayer1)
        {
            int reverseIndex = playerDeckArea.childCount - index;
            Transform childTransform = playerDeckArea.GetChild(reverseIndex - 1);
            childTransform.SetAsFirstSibling();
            //refreshDeckMenuCardList(playerDeckArea.name.ToLower());
            deckMenuArea.GetChild(index).SetAsLastSibling();
        }
        if (!deckCard.isPlayer1)
        {
            int reverseIndex = enemyDeckArea.childCount - index;
            Transform childTransform = enemyDeckArea.GetChild(reverseIndex - 1);
            childTransform.SetAsFirstSibling();
            //refreshDeckMenuCardList(enemyDeckArea.name.ToLower());
            deckMenuArea.GetChild(index).SetAsLastSibling();
        }
    }

    /** デッキ枚数表示値の更新処理 */
    public void UpdateDeckCountText()
    {
        if (playerDeckCountText != null)
            //playerDeckCountText.text = $"枚数: {playerDeckList.Count}";
            playerDeckCountText.text = $"Deck({playerDeckArea.childCount})";

        if (enemyDeckCountText != null)
            //enemyDeckCountText.text = $"枚数: {enemyDeckList.Count}";
            enemyDeckCountText.text = $"Deck({enemyDeckArea.childCount})";
    }

    /** デッキメニューカード4枚表示ボタン押下時の処理 */
    public void OnClickFourDeckMenuCardsButton()
    {
        int fourCount = 4;
        for (int i = 0; i < fourCount; i++)
        {
            Transform childTransform = deckMenuArea.GetChild(i);
            DeckCardListInBattleUIPrefab child = childTransform.GetComponent<DeckCardListInBattleUIPrefab>();
            if (!child.isFront)
            {
                child.changeFrontAndBack();
            }
        }
    }

    /** デッキメニューカード全表示ボタン押下時の処理 */
    public void OnClickAlllDeckMenuCardsButton()
    {
        for (int i = 0; i < deckMenuArea.childCount; i++)
        {
            Transform childTransform = deckMenuArea.GetChild(i);
            DeckCardListInBattleUIPrefab child = childTransform.GetComponent<DeckCardListInBattleUIPrefab>();
            if (!child.isFront)
            {
                child.changeFrontAndBack();
            }
        }
    }

    /** シャッフルボタン押下時の処理 */
    public void ShuffleDeckArea()
    {
        Transform deckArea = null;
        int deckMenuAreaCount = deckMenuArea.childCount;
        if (deckMenuAreaCount < 2) { return; }
        if (deckMenuAreaCount >= 2)
        {
            Transform child1Transform = deckMenuArea.GetChild(0);
            DeckCardListInBattleUIPrefab child1 = child1Transform.GetComponent<DeckCardListInBattleUIPrefab>();
            Transform child2Transform = deckMenuArea.GetChild(1);
            DeckCardListInBattleUIPrefab child2 = child2Transform.GetComponent<DeckCardListInBattleUIPrefab>();
            // どちらも同じプレイヤー側なら deckArea を選択
            if (child1.isPlayer1 == child2.isPlayer1)
            {
                deckArea = child1.isPlayer1 ? playerDeckArea : enemyDeckArea;
            }
            else
            {
                return; // プレイヤーが混在していれば中断
            }
        }

        if (deckArea == null) return;
        List<Transform> cards = new List<Transform>();
        for (int i = 0; i < deckArea.childCount; i++)
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
        refreshDeckMenuCardList(deckArea.name.ToLower());
    }


    // public void MoveTopDeckCardToBottom(Transform deckArea)
    // {
    //     int count = deckArea.childCount;
    //     if (count <= 1) return;
    //     // 一番上のカード（＝最後の子）を取得
    //     Transform topCard = deckArea.GetChild(count - 1);
    //     // 一番下に移動
    //     topCard.SetAsFirstSibling();

    //     Debug.Log("Moved top deck card to bottom.");
    // }

    /** デッキメニュー画面のトップカードを削除する*/
    // public void RemoveTopDeckMenuCard(Transform deckArea)
    // {
    //     if (deckArea.name.ToLower().Contains("playerdeck"))
    //     {
    //         //int count = playerDeckMenuArea.childCount;
    //         Transform topDeckCard = playerDeckMenuArea.GetChild(0);
    //         Debug.Log("RemoveTopDeckMenuCard 削除対象：" + topDeckCard);
    //         Destroy(topDeckCard.gameObject);
    //     }
    // }




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
