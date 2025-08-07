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
    [SerializeField] private Text deckCountText, deckCountText2, filteredCardCountText, filteredCardCountText2;
    [SerializeField] private Canvas deckCardSelectCanvas;
    [SerializeField] private Canvas filterCanvas;
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
    private bool editDeckFlag;
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

        // 各フィルターのToggleと押下時処理の紐づけ
        LinkedFilterToggles();

        // デッキ編集フラグ
        editDeckFlag = false;
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

                //RefreshCardListFiltered();
                minus1CountTextInSelectCardArea(entity.cardId);
                editDeckFlag = true;
            });

            currentIndex = currentIndex + 1;
        }

        // デッキ枚数表示値の更新
        if (deckCountText != null)
        {
            deckCountText.text = $"{currentDeck.cardIDs.Count}";
            deckCountText2.text = $"{currentDeck.cardIDs.Count}";
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
            //Debug.Log($"デッキ名変更＆保存: {newName}");
            editDeckFlag = true;
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
        editDeckFlag = true;
        selectColorCanvas.sortingOrder = -10;
    }

    /** デッキカラーの変更 */
    public void HideSelectColorCanvas()
    {
        //Debug.Log("hide");
        selectColorCanvas.sortingOrder = -10;
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
    public void PlusDeckCard(int cardId)
    {
        currentDeck.cardIDs.Add(cardId);
        editDeckFlag = true;
    }
    public void MinusDeckCard(int cardId)
    {
        currentDeck.cardIDs.Remove(cardId);
        editDeckFlag = true;
    }
    public void SetDeckIcon(int cardId)
    {
        currentDeck.leaderCardId = cardId;
        editDeckFlag = true;
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
        //Debug.Log("デッキ保存完了");
        editDeckFlag = false;
        SceneManager.LoadScene("DecksScene");
    }

    [SerializeField] private GameObject ConfirmBackUIPanel;

    /** 戻るボタン押下時の処理 */
    public void OnClickBackButton()
    {
        if (editDeckFlag)
        {
            if (ConfirmBackUIPanel != null) { ConfirmBackUIPanel.SetActive(true); }
        }
        else
        {
            SceneManager.LoadScene("DecksScene");
        }
    }

    /** 戻る確認：保存を押下時処理 */
    public void OnClickSaveAndBackUIButton()
    {
        if (ConfirmBackUIPanel != null) { ConfirmBackUIPanel.SetActive(false); }
        OnClickSaveButton();
    }

    /** 戻る確認：保存しないを押下時処理 */
    public void OnClickNoSaveAndBackUIButton()
    {
        if (ConfirmBackUIPanel != null) { ConfirmBackUIPanel.SetActive(false); }
        SceneManager.LoadScene("DecksScene");
    }

    /** 戻る確認：キャンセルを押下時処理 */
    public void OnClickCancelBackUIButton()
    {
        if (ConfirmBackUIPanel != null) { ConfirmBackUIPanel.SetActive(false); }
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
    /// 

    /** デッキエリアからカード削除時に、カード選択エリアの枚数表示から1枚マイナスする */
    private void minus1CountTextInSelectCardArea(int targetCardId)
    {
        if (cardIdToIndex.TryGetValue(targetCardId, out int foundIndex))
        {
            //Debug.Log($"Card ID {targetCardId} has index {foundIndex}");
            Transform cardItem = selectCardContent.GetChild(foundIndex);
            // CountText を取得
            Text countText = cardItem.Find("CountText")?.GetComponent<Text>();
            Transform countTextPanel = cardItem.Find("CountTextPanel");
            if (countText != null)
            {
                if (int.TryParse(countText.text, out int currentCount))
                {
                    int newCount = currentCount - 1;
                    if (newCount > 0)
                    {
                        countText.text = newCount.ToString();
                        if (countTextPanel != null) countTextPanel.gameObject.SetActive(true);
                    }
                    else
                    {
                        countText.text = "";
                        if (countTextPanel != null) countTextPanel.gameObject.SetActive(false);
                    }
                }
                else
                {
                    Debug.LogWarning("CountText is not a valid number");
                }
            }
            else
            {
                Debug.LogWarning("CountText not found in card item");
            }
        }
        else
        {
            Debug.LogWarning("Card ID not found in index dictionary");
        }
    }
    // cardId → currentIndex のマップ
    private Dictionary<int, int> cardIdToIndex = new Dictionary<int, int>();

    /// <summary>
    /// //////////////////////////////////////////////////////////////////////////////////////////////////
    /// </summary>
    /// 
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

    /** カード表示エリアの更新 */
    private IEnumerator RefreshSelectCardList()
    {
        // まずカード一覧を完全に削除
        foreach (Transform child in selectCardContent)
        {
            Destroy(child.gameObject);
        }
        cardIdToIndex.Clear();

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
            // カードタイプフィルター内容と合致しているかどうか（カードタイプフィルターが一つも選択されていない場合もtrue）
            //string cardType = cardEntity.cardType?.Trim().ToLower();
            //bool matchesCardType = activeTypeFilters.Count == 0 ||
            // (cardType != null && activeTypeFilters.Exists(f => cardType.Contains(f.Trim().ToLower())));
            string cardType = cardEntity.cardType;
            bool matchesCardType = activeTypeFilters.Count == 0 ||
             (cardType != null && activeTypeFilters.Exists(f => cardType.Contains(f)));

            // 色フィルター内容と合致しているかどうか（色フィルターが一つも選択されていない場合もtrue）
            //string cardColor = cardEntity.color?.Trim().ToLower();
            //bool matchesColor = activeColorFilters.Count == 0 ||
            //activeColorFilters.Exists(f => f.Trim().ToLower() == cardColor);
            //(cardColor != null && activeTypeFilters.Exists(f => cardColor.Contains(f.Trim().ToLower())));
            string cardColor = cardEntity.color;
            bool matchesColor = activeColorFilters.Count == 0 ||
            (cardColor != null && activeColorFilters.Exists(f => cardColor.Contains(f)));

            // コストフィルター内容と合致しているかどうか
            bool matchesCost = false;
            for (int i = 1; i <= 10; i++)
            {
                if (activeCostFilters[i] && cardEntity.cost == i)
                {
                    matchesCost = true;
                    break;
                }
            }
            // コストフィルターが1つもONでなければ全表示
            bool anyCostSelected = false;
            for (int i = 1; i <= 10; i++)
                if (activeCostFilters[i]) { anyCostSelected = true; break; }
            if (!anyCostSelected) matchesCost = true;


            // Extraフィルター
            bool matchesExtraFilter = true;
            if (checkFlag_inkwell && matchesExtraFilter) { matchesExtraFilter = (cardEntity.inkwellFlag > 0); }
            if (checkFlag_inkless && matchesExtraFilter) { matchesExtraFilter = (cardEntity.inkwellFlag == 0); }
            if (checkFlag_vanilla && matchesExtraFilter) { matchesExtraFilter = (cardEntity.vanillaFlag > 0); }

            if (checkFlag_bodyguard && matchesExtraFilter) { matchesExtraFilter = (cardEntity.bodyguardFlag > 0); }
            if (checkFlag_challenger && matchesExtraFilter) { matchesExtraFilter = (cardEntity.challengerFlag > 0); }
            if (checkFlag_evasive && matchesExtraFilter) { matchesExtraFilter = (cardEntity.evasiveFlag > 0); }
            if (checkFlag_reckless && matchesExtraFilter) { matchesExtraFilter = (cardEntity.recklessFlag > 0); }
            if (checkFlag_resist && matchesExtraFilter) { matchesExtraFilter = (cardEntity.resistFlag > 0); }
            if (checkFlag_rush && matchesExtraFilter) { matchesExtraFilter = (cardEntity.rushFlag > 0); }
            if (checkFlag_shift && matchesExtraFilter) { matchesExtraFilter = (cardEntity.shiftFlag > 0); }
            if (checkFlag_singer && matchesExtraFilter) { matchesExtraFilter = (cardEntity.singerFlag > 0); }
            if (checkFlag_singTogether && matchesExtraFilter) { matchesExtraFilter = (cardEntity.singTogetherFlag > 0); }
            if (checkFlag_support && matchesExtraFilter) { matchesExtraFilter = (cardEntity.supportFlag > 0); }
            if (checkFlag_ward && matchesExtraFilter) { matchesExtraFilter = (cardEntity.wardFlag > 0); }

            // 攻撃力フィルター内容と合致しているかどうか
            bool matchesStrength = false;
            for (int i = 0; i <= 10; i++)
            {
                if (activeStrengthFilters[i] && cardEntity.strength == i)
                {
                    matchesStrength = true;
                    break;
                }
            }
            // 攻撃力フィルターが1つもONでなければ全表示
            bool anyStrengthSelected = false;
            for (int i = 0; i <= 10; i++)
                if (activeStrengthFilters[i]) { anyStrengthSelected = true; break; }
            if (!anyStrengthSelected) matchesStrength = true;

            // 意思力フィルター内容と合致しているかどうか
            bool matchesWillpower = false;
            for (int i = 0; i <= 10; i++)
            {
                if (activeWillpowerFilters[i] && cardEntity.willpower == i)
                {
                    matchesWillpower = true;
                    break;
                }
            }
            // 意思力フィルターが1つもONでなければ全表示
            bool anyWillpowerSelected = false;
            for (int i = 0; i <= 10; i++)
                if (activeWillpowerFilters[i]) { anyWillpowerSelected = true; break; }
            if (!anyWillpowerSelected) matchesWillpower = true;

            // ロアフィルター内容と合致しているかどうか
            bool matchesLoreValue = false;
            for (int i = 0; i <= 10; i++)
            {
                if (activeLoreValueFilters[i] && cardEntity.loreValue == i)
                {
                    matchesLoreValue = true;
                    break;
                }
            }
            // ロアフィルターが1つもONでなければ全表示
            bool anyLoreValueSelected = false;
            for (int i = 0; i <= 10; i++)
                if (activeLoreValueFilters[i]) { anyLoreValueSelected = true; break; }
            if (!anyLoreValueSelected) matchesLoreValue = true;

            // レアリティフィルター内容と合致しているかどうか（レアリティフィルターが一つも選択されていない場合もtrue）
            string cardRarity = cardEntity.rarity;
            bool matchesRarity = activeRarityFilters.Count == 0 ||
             (cardRarity != null && activeRarityFilters.Exists(f => cardRarity.Contains(f)));

            // シリーズフィルター内容と合致しているかどうか
            string cardIdStr = cardEntity.cardId.ToString();
            string cardSeries = cardIdStr.Length > 0 ? cardIdStr.Substring(0, 1).Trim().ToLower() : "";
            bool matchesSeries = false;
            for (int i = 1; i <= 4; i++)
            {
                if (activeSeriesFilters[i] && int.Parse(cardSeries) == i)
                {
                    matchesSeries = true;
                    break;
                }
            }
            // シリーズフィルターが1つもONでなければ全表示
            bool anySeriesSelected = false;
            for (int i = 1; i <= 4; i++)
                if (activeSeriesFilters[i]) { anySeriesSelected = true; break; }
            if (!anySeriesSelected) matchesSeries = true;

            // 名前検索フィルター
            string searchText = searchNameInputField?.text?.Trim().ToLower();
            //Debug.LogWarning("検索文字列：" + searchText);
            bool matchesName = string.IsNullOrEmpty(searchText) ||
                               (cardEntity.name != null && cardEntity.name.ToLower().Contains(searchText)) ||
                               (cardEntity.versionName != null && cardEntity.versionName.ToLower().Contains(searchText)) ||
                               (cardEntity.classification != null && cardEntity.classification.ToLower().Contains(searchText));

            // 各フィルターに内容と合致している場合は表示対象とする
            if (matchesCardType && matchesColor && matchesCost && matchesExtraFilter && matchesStrength && matchesWillpower && matchesLoreValue &&
            matchesSeries & matchesRarity & matchesName)
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

        //Debug.LogWarning("フィルター後のカード数：" + filteredCardEntities.Count);
        if (filteredCardCountText != null)
        {
            filteredCardCountText.text = $"{filteredCardEntities.Count}";
        }
        if (filteredCardCountText2 != null)
        {
            filteredCardCountText2.text = $"{filteredCardEntities.Count}";
        }

        // 表示生成（フィルター条件に合致したカードリスト）

        int currentIndex = 0;
        int batchSize = 30;
        foreach (CardEntity cardEntity in filteredCardEntities)
        {
            int cardId = cardEntity.cardId;
            // 紐づけを追加
            cardIdToIndex[cardId] = currentIndex;
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
                editDeckFlag = true;
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

    /// <summary>
    /// //////////////////////////////////////////////////////////////////////////////////////////////////
    /// </summary>

    private List<string> cardTypes = new List<string> { "character", "action", "song", "item", "location" };
    private List<string> activeTypeFilters = new List<string>();
    private List<string> cardColors = new List<string> { "umber", "amethyst", "emerald", "ruby", "sapphire", "steel" };
    private List<string> activeColorFilters = new List<string>();
    private bool[] activeCostFilters = new bool[11];

    private List<string> extraFilters = new List<string> { "bodyguard", "challenger", "evasive", "reckless", "resist", "rush", "shift", "singer", "singTogether", "support", "ward" };
    private bool checkFlag_inkwell = false;
    private bool checkFlag_inkless = false;
    private bool checkFlag_vanilla = false;
    private bool checkFlag_bodyguard = false;
    private bool checkFlag_challenger = false;
    private bool checkFlag_evasive = false;
    private bool checkFlag_reckless = false;
    private bool checkFlag_resist = false;
    private bool checkFlag_rush = false;
    private bool checkFlag_shift = false;
    private bool checkFlag_singer = false;
    private bool checkFlag_singTogether = false;
    private bool checkFlag_support = false;
    private bool checkFlag_ward = false;
    private List<string> rarities = new List<string> { "1_common", "2_uncommon", "3_rare", "4_superrare", "5_legendary" };
    private List<string> activeRarityFilters = new List<string>();

    private bool[] activeStrengthFilters = new bool[11];
    private bool[] activeWillpowerFilters = new bool[11];
    private bool[] activeLoreValueFilters = new bool[11];
    private bool[] activeSeriesFilters = new bool[5];

    /** 各フィルターのToggleと押下時処理処理の紐づけ */
    private void LinkedFilterToggles()
    {
        // カードタイプの各Toggleに対して、それぞれ押下時処理の処理メソッドを紐づけする
        foreach (string cardType in cardTypes)
        {
            var cardTypeToggle = GameObject.Find($"TypeToggle_{cardType}")?.GetComponent<Toggle>();
            if (cardTypeToggle != null)
            {
                cardTypeToggle.onValueChanged.AddListener((isOn) => { OnCardTypeToggleChangedCore(cardType, isOn); });
            }
            else
            {
                Debug.LogWarning($"TypeToggle_{cardType} が見つかりません");
            }
        }

        // 色の各Toggleに対して、それぞれ押下時処理の処理メソッドを紐づけする
        foreach (string cardColor in cardColors)
        {
            var colorToggle = GameObject.Find($"ColorToggle_{cardColor}")?.GetComponent<Toggle>();
            if (colorToggle != null)
            {
                colorToggle.onValueChanged.AddListener((isOn) => { OnColorToggleChangedCore(cardColor, isOn); });
            }
            else
            {
                Debug.LogWarning($"ColorToggle_{cardColor} が見つかりません");
            }
        }

        // コストの各Toggleに対して、それぞれ押下時処理の処理メソッドを紐づけする
        for (int i = 1; i <= 10; i++)
        {
            var costToggle = GameObject.Find($"CostToggle_{i}")?.GetComponent<Toggle>();
            int capturedCost = i;
            if (costToggle != null)
            {
                costToggle.onValueChanged.AddListener((isOn) => { OnCostToggleChanged(capturedCost, isOn); });
            }
            else
            {
                Debug.LogWarning($"CostToggle_{i} が見つかりません");
            }
        }

        var extraToggle_inkwell = GameObject.Find("ExtraToggle_inkwell")?.GetComponent<Toggle>();
        extraToggle_inkwell.onValueChanged.AddListener((isOn) => { checkFlag_inkwell = !isOn; RefreshCardListFiltered(); });

        var extraToggle_inkless = GameObject.Find("ExtraToggle_inkless")?.GetComponent<Toggle>();
        extraToggle_inkless.onValueChanged.AddListener((isOn) => { checkFlag_inkless = !isOn; RefreshCardListFiltered(); });

        var extraToggle_vanilla = GameObject.Find("ExtraToggle_vanilla")?.GetComponent<Toggle>();
        extraToggle_vanilla.onValueChanged.AddListener((isOn) => { checkFlag_vanilla = !isOn; RefreshCardListFiltered(); });

        var extraToggle_bodyguard = GameObject.Find("ExtraToggle_bodyguard")?.GetComponent<Toggle>();
        extraToggle_bodyguard.onValueChanged.AddListener((isOn) => { checkFlag_bodyguard = !isOn; RefreshCardListFiltered(); });

        var extraToggle_challenger = GameObject.Find("ExtraToggle_challenger")?.GetComponent<Toggle>();
        extraToggle_challenger.onValueChanged.AddListener((isOn) => { checkFlag_challenger = !isOn; RefreshCardListFiltered(); });

        var extraToggle_evasive = GameObject.Find("ExtraToggle_evasive")?.GetComponent<Toggle>();
        extraToggle_evasive.onValueChanged.AddListener((isOn) => { checkFlag_evasive = !isOn; RefreshCardListFiltered(); });

        var extraToggle_reckless = GameObject.Find("ExtraToggle_reckless")?.GetComponent<Toggle>();
        extraToggle_reckless.onValueChanged.AddListener((isOn) => { checkFlag_reckless = !isOn; RefreshCardListFiltered(); });

        var extraToggle_resist = GameObject.Find("ExtraToggle_resist")?.GetComponent<Toggle>();
        extraToggle_resist.onValueChanged.AddListener((isOn) => { checkFlag_resist = !isOn; RefreshCardListFiltered(); });

        var extraToggle_rush = GameObject.Find("ExtraToggle_rush")?.GetComponent<Toggle>();
        extraToggle_rush.onValueChanged.AddListener((isOn) => { checkFlag_rush = !isOn; RefreshCardListFiltered(); });

        var extraToggle_shift = GameObject.Find("ExtraToggle_shift")?.GetComponent<Toggle>();
        extraToggle_shift.onValueChanged.AddListener((isOn) => { checkFlag_shift = !isOn; RefreshCardListFiltered(); });

        var extraToggle_singer = GameObject.Find("ExtraToggle_singer")?.GetComponent<Toggle>();
        extraToggle_singer.onValueChanged.AddListener((isOn) => { checkFlag_singer = !isOn; RefreshCardListFiltered(); });

        var extraToggle_singTogether = GameObject.Find("ExtraToggle_singTogether")?.GetComponent<Toggle>();
        extraToggle_singTogether.onValueChanged.AddListener((isOn) => { checkFlag_singTogether = !isOn; RefreshCardListFiltered(); });

        var extraToggle_support = GameObject.Find("ExtraToggle_support")?.GetComponent<Toggle>();
        extraToggle_support.onValueChanged.AddListener((isOn) => { checkFlag_support = !isOn; RefreshCardListFiltered(); });

        var extraToggle_ward = GameObject.Find("ExtraToggle_ward")?.GetComponent<Toggle>();
        extraToggle_ward.onValueChanged.AddListener((isOn) => { checkFlag_ward = !isOn; RefreshCardListFiltered(); });

        // 攻撃力の各Toggleに対して、それぞれ押下時処理の処理メソッドを紐づけする
        for (int i = 0; i <= 10; i++)
        {
            var strengthToggle = GameObject.Find($"StrengthToggle_{i}")?.GetComponent<Toggle>();
            int capturedStrength = i;
            if (strengthToggle != null)
            {
                strengthToggle.onValueChanged.AddListener((isOn) => { OnStrengthToggleChanged(capturedStrength, isOn); });
            }
            else
            {
                Debug.LogWarning($"StrengthToggle_{i} が見つかりません");
            }
        }
        // 意思力の各Toggleに対して、それぞれ押下時処理の処理メソッドを紐づけする
        for (int i = 0; i <= 10; i++)
        {
            var willpowerToggle = GameObject.Find($"WillpowerToggle_{i}")?.GetComponent<Toggle>();
            int capturedWillpower = i;
            if (willpowerToggle != null)
            {
                willpowerToggle.onValueChanged.AddListener((isOn) => { OnWillpowerToggleChanged(capturedWillpower, isOn); });
            }
            else
            {
                Debug.LogWarning($"WillpowerToggle_{i} が見つかりません");
            }
        }

        // ロア値の各Toggleに対して、それぞれ押下時処理の処理メソッドを紐づけする
        for (int i = 0; i <= 10; i++)
        {
            var loreValueToggle = GameObject.Find($"LoreValueToggle_{i}")?.GetComponent<Toggle>();
            int capturedLoreValue = i;
            if (loreValueToggle != null)
            {
                loreValueToggle.onValueChanged.AddListener((isOn) => { OnLoreValueToggleChanged(capturedLoreValue, isOn); });
            }
            else
            {
                Debug.LogWarning($"LoreValueToggle_{i} が見つかりません");
            }
        }

        // シリーズの各Toggleに対して、それぞれ押下時処理の処理メソッドを紐づけする
        for (int j = 1; j <= 4; j++)
        {
            var seriesToggle = GameObject.Find($"SeriesToggle_{j}")?.GetComponent<Toggle>();
            int cardSeries = j;
            if (seriesToggle != null)
            {
                seriesToggle.onValueChanged.AddListener((isOn) => { OnSeriesToggleChangedCore(cardSeries, isOn); });
            }
            else
            {
                Debug.LogWarning($"SeriesToggle_{j} が見つかりません");
            }
        }
        // レアリティの各Toggleに対して、それぞれ押下時処理の処理メソッドを紐づけする
        foreach (string rarity in rarities)
        {
            var rarityToggle = GameObject.Find($"RarityToggle_{rarity}")?.GetComponent<Toggle>();
            if (rarityToggle != null)
            {
                rarityToggle.onValueChanged.AddListener((isOn) => { OnRarityToggleChangedCore(rarity, isOn); });
            }
            else
            {
                Debug.LogWarning($"RarityToggle_{rarity} が見つかりません");
            }
        }
        // 入力が変更されたらカードリスト更新
        if (searchNameInputField != null)
        {
            searchNameInputField.onValueChanged.AddListener((_) => RefreshCardListFiltered());
        }
    }

    /** カードタイプフィルターを更新してカード選択エリアを再表示 */
    private void OnCardTypeToggleChangedCore(string cardType, bool isOn)
    {
        //string normalizedCardType = (cardType ?? "").Trim().ToLower();
        string normalizedCardType = cardType;

        if (!isOn)
        {
            if (!activeTypeFilters.Contains(normalizedCardType))
                activeTypeFilters.Add(normalizedCardType);
        }
        else
        {
            if (activeTypeFilters.Contains(normalizedCardType))
                activeTypeFilters.Remove(normalizedCardType);
        }
        //Debug.LogWarning("カードタイプフィルター更新：" + string.Join(", ", activeTypeFilters));
        RefreshCardListFiltered();
    }

    /** 色フィルターを更新してカード選択エリアを再表示 */
    private void OnColorToggleChangedCore(string color, bool isOn)
    {
        //string normalizedColor = (color ?? "").Trim().ToLower();
        string normalizedColor = color;

        if (!isOn)
        {
            if (!activeColorFilters.Contains(normalizedColor))
                activeColorFilters.Add(normalizedColor);
        }
        else
        {
            if (activeColorFilters.Contains(normalizedColor))
                activeColorFilters.Remove(normalizedColor);
        }
        //Debug.LogWarning("色フィルター更新：" + string.Join(", ", activeColorFilters));
        RefreshCardListFiltered();
    }

    /** コストフィルターを更新してカード選択エリアを再表示 */
    private void OnCostToggleChanged(int cost, bool isOn)
    {
        if (cost >= 1 && cost <= 10)
        {
            activeCostFilters[cost] = !isOn;
            //Debug.LogWarning("コストフィルター更新：" + string.Join(", ", activeCostFilters));
            RefreshCardListFiltered();
        }
    }

    /** 攻撃力フィルターを更新してカード選択エリアを再表示 */
    private void OnStrengthToggleChanged(int strength, bool isOn)
    {
        if (strength >= 0 && strength <= 10)
        {
            activeStrengthFilters[strength] = !isOn;
            //Debug.LogWarning("攻撃力フィルター更新：" + string.Join(", ", activeStrengthFilters));
            RefreshCardListFiltered();
        }
    }

    /** 意思力フィルターを更新してカード選択エリアを再表示 */
    private void OnWillpowerToggleChanged(int willpower, bool isOn)
    {
        if (willpower >= 0 && willpower <= 10)
        {
            activeWillpowerFilters[willpower] = !isOn;
            //Debug.LogWarning("意思力フィルター更新：" + string.Join(", ", activeWillpowerFilters));
            RefreshCardListFiltered();
        }
    }

    /** ロア値フィルターを更新してカード選択エリアを再表示 */
    private void OnLoreValueToggleChanged(int loreValue, bool isOn)
    {
        if (loreValue >= 0 && loreValue <= 10)
        {
            activeLoreValueFilters[loreValue] = !isOn;
            //Debug.LogWarning("ロア値フィルター更新：" + string.Join(", ", activeLoreValueFilters));
            RefreshCardListFiltered();
        }
    }


    /** レアリティフィルターを更新してカード選択エリアを再表示 */
    private void OnRarityToggleChangedCore(string rarity, bool isOn)
    {
        //string normalizedRarity = (rarity ?? "").Trim().ToLower();
        string normalizedRarity = rarity;

        if (!isOn)
        {
            if (!activeRarityFilters.Contains(normalizedRarity))
                activeRarityFilters.Add(normalizedRarity);
        }
        else
        {
            if (activeRarityFilters.Contains(normalizedRarity))
                activeRarityFilters.Remove(normalizedRarity);
        }
        //Debug.LogWarning("レアリティフィルター更新：" + string.Join(", ", activeRarityFilters));
        RefreshCardListFiltered();
    }

    /** シリーズフィルターを更新してカード選択エリアを再表示 */
    private void OnSeriesToggleChangedCore(int cardSeries, bool isOn)
    {
        string strCardSeries = cardSeries.ToString();

        if (cardSeries >= 1 && cardSeries <= 10)
        {
            activeSeriesFilters[cardSeries] = !isOn;
            //Debug.LogWarning("シリーズフィルター更新：" + string.Join(", ", activeSeriesFilters));
            RefreshCardListFiltered();
        }
    }

    /// <summary>
    /// //////////////////////////////////////////////////////////////////////////////////////////////////
    /// </summary>


    public void OnClickHideSelectCardCanvas()
    {
        if (deckCardSelectCanvas != null)
        {
            deckCardSelectCanvas.sortingOrder = -10;
        }
    }

    /** フィルター画面の表示 */
    public void ShowFilterCanvas()
    {
        if (filterCanvas != null)
        {
            filterCanvas.sortingOrder = 20;
        }
    }
    /** フィルター画面の非表示 */
    public void HideFilterCanvas()
    {
        if (filterCanvas != null)
            filterCanvas.sortingOrder = -20;
    }
}
