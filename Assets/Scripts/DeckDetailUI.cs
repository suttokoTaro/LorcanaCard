using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.EventSystems;
using System;
using System.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;

public class DeckDetailUI : MonoBehaviour
{
    [SerializeField] private SwipeDetector2 swipeDetector2;
    [SerializeField] private InputField deckNameInputField;
    [SerializeField] private Image deckIcon, colorIcon1, colorIcon2;
    [SerializeField] private Button colorButton1, colorButton2, umberButton, amethystButton, emeraldButton, rubyButton, sapphireButton, steelButton, noColorButton;
    private int changedColorN;
    [SerializeField] private Canvas selectColorCanvas;
    [SerializeField] private Text deckCountText, filteredCardCountText;
    [SerializeField] private Canvas deckCardSelectCanvas;
    [SerializeField] private GameObject zoomCanvas;
    [SerializeField] private Image zoomImage;
    [SerializeField] private InputField searchNameInputField;
    public Text deckNameText;
    private List<(CardEntity entity, int count)> deckCardList = new List<(CardEntity, int)>();
    public Transform deckCardListContent, deckCardListContentInSelectCanvas, selectCardContent; // デッキ内カード表示用
    public GameObject cardItemPrefab;
    private DeckData currentDeck;
    IList<CardEntity> allCardEntities;
    private Coroutine refreshCoroutine;
    async void Start()
    {
        // デッキリスト画面で選択したデッキ情報が取得できない場合エラーを返す
        currentDeck = SelectedDeckData.selectedDeck;
        if (currentDeck == null)
        {
            DeckData newDeck = new DeckData();
            newDeck.deckId = System.Guid.NewGuid().ToString(); // デッキのユニークIDの生成 
            newDeck.deckName = "新しいデッキ";
            newDeck.cardIDs = new List<int>();
            currentDeck = newDeck;
        }
        // デッキ名入力エリアが存在する場合、取得したデッキ情報のデッキ名を設定し、編集可能とする
        if (deckNameInputField != null)
        {
            deckNameInputField.text = currentDeck.deckName;
            deckNameInputField.onEndEdit.AddListener(OnDeckNameChanged);
        }

        colorButton1.onClick.AddListener(() => { OpenColorPanel(1); });
        colorButton2.onClick.AddListener(() => { OpenColorPanel(2); });
        umberButton.onClick.AddListener(() => { ChangeDeckColor(1); });
        amethystButton.onClick.AddListener(() => { ChangeDeckColor(2); });
        emeraldButton.onClick.AddListener(() => { ChangeDeckColor(3); });
        rubyButton.onClick.AddListener(() => { ChangeDeckColor(4); });
        sapphireButton.onClick.AddListener(() => { ChangeDeckColor(5); });
        steelButton.onClick.AddListener(() => { ChangeDeckColor(6); });
        noColorButton.onClick.AddListener(() => { ChangeDeckColor(0); });

        // デッキリストエリアの表示の更新
        RefreshDeckCardList();

        // ラベルでCardEntityを非同期ロード
        //AsyncOperationHandle<IList<CardEntity>> handle = Addressables.LoadAssetsAsync<CardEntity>("CardEntityList", null);
        //allCardEntities = await handle.Task;
        allCardEntities = CardEntityCache.Instance.AllCardEntities;

        // 入力が変更されたらカードリスト更新
        if (searchNameInputField != null)
        {
            searchNameInputField.onValueChanged.AddListener((_) => RefreshCardListFiltered());
        }

        // 非同期表示（バッチ処理）
        StartCoroutine(RefreshSelectCardList());
    }


    /** デッキリストエリアの表示の更新 */
    public void RefreshDeckCardList()
    {
        // まず一度デッキリストエリアの既存のオブジェクトをすべて削除
        foreach (Transform child in deckCardListContent) { Destroy(child.gameObject); }
        foreach (Transform child in deckCardListContentInSelectCanvas) { Destroy(child.gameObject); }
        deckCardList.Clear();

        // カードIDごとの枚数の算出
        var cardCountDict = new Dictionary<int, int>();
        foreach (int id in currentDeck.cardIDs)
        {
            if (!cardCountDict.ContainsKey(id))
                cardCountDict[id] = 0;
            cardCountDict[id]++;
        }

        foreach (var pair in cardCountDict)
        {
            var entity = Resources.Load<CardEntity>($"CardEntityList/Card_{pair.Key}");
            //var entity = await LoadCardEntityByIdAsync(pair.Key);
            if (entity != null)
                deckCardList.Add((entity, pair.Value));
        }

        // ソート：コスト昇順 → ID昇順
        deckCardList.Sort((a, b) =>
        {
            int costCompare = a.entity.cost.CompareTo(b.entity.cost);
            if (costCompare != 0) return costCompare;
            return a.entity.cardId.CompareTo(b.entity.cardId);
        });

        // UI生成
        var log = string.Join("\n", deckCardList.Select(pair =>
            $"cardId: {pair.entity.cardId}, count: {pair.count}"));
        //Debug.Log(log);

        // スワイプビューアに渡す
        swipeDetector2.SetDeckCardList(deckCardList);

        int currentIndex = 0;
        foreach (var (entity, count) in deckCardList)
        {
            GameObject item = Instantiate(cardItemPrefab, deckCardListContent);
            var icon = item.transform.Find("Image")?.GetComponent<Image>();
            if (icon != null) icon.sprite = entity.icon;
            var countText = item.transform.Find("CountText")?.GetComponent<Text>();
            if (countText != null) countText.text = $"{count}";

            Text index = item.transform.Find("Index")?.GetComponent<Text>();
            index.text = currentIndex.ToString();

            item.GetComponent<Button>().onClick.AddListener(() =>
            {
                // 拡大表示
                ShowZoom(icon.sprite, index);
            });

            GameObject itemInSelectCanvas = Instantiate(cardItemPrefab, deckCardListContentInSelectCanvas);
            var iconInSelectCanvas = itemInSelectCanvas.transform.Find("Image")?.GetComponent<Image>();
            if (iconInSelectCanvas != null) iconInSelectCanvas.sprite = entity.icon;
            var countTextInSelectCanvas = itemInSelectCanvas.transform.Find("CountText")?.GetComponent<Text>();
            if (countTextInSelectCanvas != null) countTextInSelectCanvas.text = $"{count}";

            // 押下時に1枚削除する
            itemInSelectCanvas.GetComponent<Button>().onClick.AddListener(() =>
            {
                currentDeck.cardIDs.Remove(entity.cardId); // 1枚削除
                RefreshDeckCardList();
                RefreshCardListFiltered();
            });

            currentIndex = currentIndex + 1;
        }

        // デッキ枚数表示値の更新
        if (deckCountText != null)
        {
            deckCountText.text = $"{currentDeck.cardIDs.Count}枚";
        }

        // デッキアイコンの設定
        var defaultLeaderCard = Resources.Load<CardEntity>($"CardEntityList/Card_1001");
        //var defaultLeaderCard = await LoadCardEntityByIdAsync(1001);
        deckIcon.sprite = defaultLeaderCard.backIcon;
        var leaderCard = Resources.Load<CardEntity>($"CardEntityList/Card_{currentDeck.leaderCardId}");
        //var leaderCard = await LoadCardEntityByIdAsync(currentDeck.leaderCardId);
        if (leaderCard != null)
        {
            deckIcon.sprite = leaderCard.icon;
        }
        // デッキ色アイコンの設定
        var color1 = Resources.Load<ColorEntity>($"ColorEntityList/ColorEntity_{currentDeck.color1}");
        colorIcon1.sprite = color1.colorIcon;
        var color2 = Resources.Load<ColorEntity>($"ColorEntityList/ColorEntity_{currentDeck.color2}");
        colorIcon2.sprite = color2.colorIcon;
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

    /** デッキカラーの変更 */
    private void OpenColorPanel(int colorN)
    {
        changedColorN = colorN;
        selectColorCanvas.sortingOrder = 10;
    }

    /** デッキカラーの変更 */
    private void ChangeDeckColor(int color)
    {
        if (changedColorN == 1) { currentDeck.color1 = color; }
        if (changedColorN == 2) { currentDeck.color2 = color; }
        RefreshDeckCardList();
        RefreshCardListFiltered();
        selectColorCanvas.sortingOrder = -10;
    }

    /** デッキカラーの変更 */
    public void HideSelectColorCanvas()
    {
        Debug.Log("hide");
        selectColorCanvas.sortingOrder = -10;
    }

    /** デッキ保存ボタン押下時の処理 */
    public void OnClickSaveButton()
    {
        string updatedAt = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
        currentDeck.updatedAt = updatedAt;
        DeckDataList list = DeckStorage.LoadDecks();
        int index = list.decks.FindIndex(d => d.deckId == currentDeck.deckId);
        if (index >= 0)
        {
            list.decks[index] = currentDeck;
        }
        else
        {
            currentDeck.createdAt = updatedAt;
            list.decks.Add(currentDeck);
        }
        DeckStorage.SaveDecks(list);
        Debug.Log("デッキ保存完了");
    }

    /** 戻るボタン押下時の処理 */
    public void OnClickBackButton()
    {
        SceneManager.LoadScene("DecksScene");
    }

    /** カード選択ボタン押下時の処理 */
    public void OnClickSelectCardButton()
    {
        //SceneManager.LoadScene("DeckCardSelectScene");
        if (deckCardSelectCanvas != null)
        {
            deckCardSelectCanvas.sortingOrder = 10;
        }
    }
    /** カードの拡大表示 */
    private void ShowZoom(Sprite sprite, Text index)
    {
        if (zoomCanvas != null && zoomImage != null)
        {
            swipeDetector2.SetCurrentIndex(int.Parse(index.text));
            swipeDetector2.ShowCard(int.Parse(index.text));
            //zoomImage.sprite = sprite;
            zoomCanvas.SetActive(true);
        }
    }

    /** カードIDからcardEntity情報の取得（非同期） */
    public async Task<CardEntity> LoadCardEntityByIdAsync(int id)
    {
        string address = $"Card_{id}";
        AsyncOperationHandle<CardEntity> handle = Addressables.LoadAssetAsync<CardEntity>(address);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            return handle.Result;
        }
        else
        {
            Debug.LogError($"CardEntity {address} の読み込みに失敗しました");
            return null;
        }
    }
    /// <summary>
    /// //////////////////////////////////////////////////////////////////////////////////////////////////
    /// </summary>

    /** カード表示エリアの更新 */
    private IEnumerator RefreshSelectCardList()
    {
        // まずカード一覧を完全に削除
        foreach (Transform child in selectCardContent)
        {
            Destroy(child.gameObject);
        }
        // デッキカラーに合致したカードリスト
        List<CardEntity> deckColorCardEntities = new List<CardEntity>();
        List<string> deckColors = new List<string>();
        var intColor1 = currentDeck.color1;
        var intColor2 = currentDeck.color2;
        if (intColor1 != 0) { deckColors.Add(intColor1.ToString()); }
        if (intColor2 != 0) { deckColors.Add(intColor2.ToString()); }
        //Debug.LogWarning("デッキカード色：" + string.Join(", ", deckColors));
        foreach (CardEntity cardEntity in allCardEntities)
        {
            string cardColor = cardEntity.color;
            bool matchesColor = deckColors.Count == 0 ||
            (cardColor != null && deckColors.Exists(f => cardColor.Contains(f)));

            if (matchesColor) { deckColorCardEntities.Add(cardEntity); }
        }

        // フィルター条件に合致したカードリスト
        List<CardEntity> filteredCardEntities = new List<CardEntity>();
        foreach (CardEntity cardEntity in deckColorCardEntities)
        {
            // 名前検索フィルター
            string searchText = searchNameInputField?.text?.Trim().ToLower();
            Debug.LogWarning("検索文字列：" + searchText);
            bool matchesName = string.IsNullOrEmpty(searchText) ||
                               (cardEntity.name != null && cardEntity.name.ToLower().Contains(searchText)) ||
                               (cardEntity.versionName != null && cardEntity.versionName.ToLower().Contains(searchText)) ||
                               (cardEntity.classification != null && cardEntity.classification.ToLower().Contains(searchText));

            // 各フィルターに内容と合致している場合は表示対象とする
            if (matchesName)
            {
                filteredCardEntities.Add(cardEntity);
            }
        }

        // ソート：色 → コスト → カードID 昇順
        filteredCardEntities = filteredCardEntities
            .OrderBy(c => (c.color ?? "").Trim().ToLower()) // 色
            .ThenBy(c => c.cost)                            // コスト
            .ThenBy(c => c.cardId)                          // カードID
            .ToList();

        Debug.LogWarning("フィルター後のカード数：" + filteredCardEntities.Count);
        if (filteredCardCountText != null)
        {
            filteredCardCountText.text = $"{filteredCardEntities.Count}";
        }

        // 表示生成（フィルター条件に合致したカードリスト）
        int currentIndex = 0;
        int batchSize = 30;
        foreach (CardEntity cardEntity in filteredCardEntities)
        {
            int cardId = cardEntity.cardId;
            GameObject item = Instantiate(cardItemPrefab, selectCardContent);

            // 画像表示
            Image iconImage = item.transform.Find("Image")?.GetComponent<Image>();
            if (iconImage != null) iconImage.sprite = cardEntity.icon;

            Text countText = item.transform.Find("CountText")?.GetComponent<Text>();
            Transform countTextPanel = item.transform.Find("CountTextPanel");
            if (countText != null) countText.text = "";
            if (countTextPanel != null) countTextPanel.gameObject.SetActive(false);

            var match = deckCardList.FirstOrDefault(pair => pair.entity.cardId == cardId);
            if (match != default)
            {
                // 見つかった場合の処理
                var foundEntity = match.entity;
                var count = match.count;
                if (countText != null) countText.text = $"{count}";
                if (countTextPanel != null) countTextPanel.gameObject.SetActive(true);
            }

            // ボタンに追加処理
            item.GetComponent<Button>().onClick.AddListener(() =>
            {
                currentDeck.cardIDs.Add(cardId);
                RefreshDeckCardList();
                Text countText = item.transform.Find("CountText")?.GetComponent<Text>();
                Transform countTextPanel = item.transform.Find("CountTextPanel");
                if (countText != null) countText.text = "";
                if (countTextPanel != null) countTextPanel.gameObject.SetActive(false);

                var match = deckCardList.FirstOrDefault(pair => pair.entity.cardId == cardId);
                if (match != default)
                {
                    // 見つかった場合の処理
                    var foundEntity = match.entity;
                    var count = match.count;
                    if (countText != null) countText.text = $"{count}";
                    if (countTextPanel != null) countTextPanel.gameObject.SetActive(true);
                }
            });

            Text index = item.transform.Find("Index")?.GetComponent<Text>();
            index.text = currentIndex.ToString();
            if ((currentIndex + 1) % batchSize == 0)
            {
                yield return null;
            }
            currentIndex = currentIndex + 1;
        }
        refreshCoroutine = null;
    }

    public void RefreshCardListFiltered()
    {
        // 既存のコルーチンが動作中なら停止
        if (refreshCoroutine != null)
        {
            StopCoroutine(refreshCoroutine);
            refreshCoroutine = null;
        }

        // 新しく開始
        refreshCoroutine = StartCoroutine(RefreshSelectCardList());
    }

    public void PlusDeckCard(int cardId)
    {
        currentDeck.cardIDs.Add(cardId);
    }
    public void MinusDeckCard(int cardId)
    {
        currentDeck.cardIDs.Remove(cardId);
    }
    public void SetDeckIcon(int cardId)
    {
        currentDeck.leaderCardId = cardId;
    }

    public void OnClickHideSelectCardCanvas()
    {
        if (deckCardSelectCanvas != null)
        {
            deckCardSelectCanvas.sortingOrder = -10;
        }
    }
}
