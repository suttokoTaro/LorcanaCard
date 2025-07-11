using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.EventSystems;

public class DeckBuilderUI : MonoBehaviour
{
    [SerializeField] private InputField deckNameInputField;
    [SerializeField] private Toggle umberToggle, amethystToggle, emeraldToggle, rubyToggle, sapphireToggle, steelToggle;
    [SerializeField] private Text deckCountText;


    public Text deckNameText;
    public Transform deckCardListContent; // デッキ内カード表示用
    public Transform allCardListContent;  // 全カード一覧表示用
    public GameObject cardItemPrefab;
    public Button saveButton;
    public Button backButton;

    private DeckData currentDeck;
    private List<string> activeColorFilters = new List<string>();
    private bool[] activeCostFilters = new bool[11]; // index 1〜10を使用
    private string originalDeckId;


    void Start()
    {
        // デッキリスト画面で選択したデッキ情報が取得できない場合エラーを返す
        currentDeck = SelectedDeckData.selectedDeck;
        if (currentDeck == null)
        {
            Debug.LogError("選択したデッキ情報が取得できません。");
            return;
        }

        // 取得したデッキ情報のデッキIDが取得できない場合は、新しいデッキIDを補完する
        if (string.IsNullOrEmpty(currentDeck.deckId))
        {
            currentDeck.deckId = System.Guid.NewGuid().ToString();
        }
        originalDeckId = currentDeck.deckId;

        // デッキ名入力エリアが存在する場合、取得したデッキ情報のデッキ名を設定し、編集可能とする
        if (deckNameInputField != null)
        {
            deckNameInputField.text = currentDeck.deckName;
            deckNameInputField.onEndEdit.AddListener(OnDeckNameChanged);
        }

        // デッキリストエリアの表示の更新
        RefreshDeckCardList();
        // カード選択エリアの表示の更新
        RefreshSelectableCardList();

        saveButton.onClick.AddListener(OnSaveDeck);
        backButton.onClick.AddListener(() => SceneManager.LoadScene("DeckListScene"));
        // 他の初期化のあとに
        if (deckNameInputField != null && currentDeck != null)
        {
            deckNameInputField.text = currentDeck.deckName;

            // 入力変更時に保存する
            deckNameInputField.onEndEdit.AddListener(OnDeckNameChanged);
        }

        // フィルターエリアの各色Toggleに対して、それぞれチェック時の処理メソッドを紐づけする
        umberToggle.onValueChanged.AddListener((isOn) =>
 {
     OnColorToggleChangedCore("umber", isOn);
 });
        amethystToggle.onValueChanged.AddListener((isOn) =>
{
    OnColorToggleChangedCore("amethyst", isOn);
});
        emeraldToggle.onValueChanged.AddListener((isOn) =>
{
    OnColorToggleChangedCore("emerald", isOn);
});
        rubyToggle.onValueChanged.AddListener((isOn) =>
{
    OnColorToggleChangedCore("ruby", isOn);
});
        sapphireToggle.onValueChanged.AddListener((isOn) =>
{
    OnColorToggleChangedCore("sapphire", isOn);
});
        steelToggle.onValueChanged.AddListener((isOn) =>
{
    OnColorToggleChangedCore("steel", isOn);
});

        // フィルターエリアの1～9のToggleに対して、それぞれチェック時の処理メソッドを紐づけする
        for (int i = 1; i <= 9; i++)
        {
            var toggle = GameObject.Find($"CostToggle_{i}")?.GetComponent<Toggle>();
            int capturedCost = i;
            if (toggle != null)
            {
                toggle.onValueChanged.AddListener((isOn) =>
                {
                    OnCostToggleChanged(capturedCost, isOn);
                });
            }
            else
            {
                Debug.LogWarning($"CostToggle_{i} が見つかりません");
            }

        }
    }

    /** デッキリストエリアの表示の更新 */
    void RefreshDeckCardList()
    {
        // まず一度デッキリストエリアの既存のオブジェクトをすべて削除
        foreach (Transform child in deckCardListContent)
            Destroy(child.gameObject);

        // カードIDごとの枚数の算出
        var cardCountDict = new Dictionary<int, int>();
        foreach (int id in currentDeck.cardIDs)
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

            // テキスト
            var nameText = item.transform.Find("nameText")?.GetComponent<Text>();
            var countText = item.transform.Find("CountText")?.GetComponent<Text>();
            if (nameText != null) nameText.text = $"Card ID: {entity.cardId}";
            if (countText != null) countText.text = $"×{count}";

            // 押下時に1枚削除する
            item.GetComponent<Button>().onClick.AddListener(() =>
            {
                currentDeck.cardIDs.Remove(entity.cardId); // 1枚削除
                RefreshDeckCardList();
            });
        }

        // デッキ枚数表示値の更新
        if (deckCountText != null)
        {
            deckCountText.text = $"現在：{currentDeck.cardIDs.Count}枚";
        }
    }

    /** カード選択エリアの表示の更新 */
    void RefreshSelectableCardList()
    {
        // まずカード一覧を完全に削除
        foreach (Transform child in allCardListContent)
        {
            Destroy(child.gameObject);
        }

        // 全カード読み込み
        CardEntity[] allEntities = Resources.LoadAll<CardEntity>("CardEntityList");

        // フィルター条件に合致したカードリスト
        List<CardEntity> filtered = new List<CardEntity>();

        foreach (CardEntity entity in allEntities)
        {
            string cardColor = entity.color?.Trim().ToLower();
            bool matchesColor = activeColorFilters.Count == 0 ||
            activeColorFilters.Exists(f => f.Trim().ToLower() == cardColor);

            bool matchesCost = false;
            for (int i = 1; i <= 10; i++)
            {
                if (activeCostFilters[i] && entity.cost == i)
                {
                    matchesCost = true;
                    break;
                }
            }
            // フィルターが1つもONでなければ全表示
            bool anyCostSelected = false;
            for (int i = 1; i <= 10; i++)
                if (activeCostFilters[i]) { anyCostSelected = true; break; }
            if (!anyCostSelected) matchesCost = true;

            if (matchesColor && matchesCost)
            {
                filtered.Add(entity);
            }
        }

        // ソート：cardId 昇順
        filtered.Sort((a, b) => a.cardId.CompareTo(b.cardId));

        // 表示生成（フィルター条件に合致したカードリスト）
        foreach (CardEntity entity in filtered)
        {
            int cardId = entity.cardId;
            GameObject item = Instantiate(cardItemPrefab, allCardListContent);

            EventTrigger trigger = item.AddComponent<EventTrigger>();

            // 長押し開始
            var down = new EventTrigger.Entry();
            down.eventID = EventTriggerType.PointerDown;
            down.callback.AddListener((eventData) =>
            {
                StartCoroutine(ShowZoom(entity.icon));
            });
            trigger.triggers.Add(down);

            // 離したとき
            var up = new EventTrigger.Entry();
            up.eventID = EventTriggerType.PointerUp;
            up.callback.AddListener((eventData) =>
            {
                StopAllCoroutines();
                //HideZoom();
            });
            trigger.triggers.Add(up);

            // 画像表示
            Image iconImage = item.transform.Find("Image")?.GetComponent<Image>();
            if (iconImage != null)
                iconImage.sprite = entity.icon;

            // テキスト表示
            Text nameText = item.transform.Find("nameText")?.GetComponent<Text>();
            Text countText = item.transform.Find("CountText")?.GetComponent<Text>();

            if (nameText != null) nameText.text = $"Card ID: {cardId}";
            if (countText != null) countText.text = ""; // 下部エリアなので枚数表示なし

            // ボタンに追加処理
            item.GetComponent<Button>().onClick.AddListener(() =>
            {
                currentDeck.cardIDs.Add(cardId);
                RefreshDeckCardList();
            });
        }
    }

    public void OnUmberToggleChanged(bool isOn) => OnColorToggleChangedCore("umber", isOn);
    public void OnAmethystToggleChanged(bool isOn) => OnColorToggleChangedCore("amethyst", isOn);
    public void OnEmeraldToggleChanged(bool isOn) => OnColorToggleChangedCore("emerald", isOn);
    public void OnRubyToggleChanged(bool isOn) => OnColorToggleChangedCore("ruby", isOn);
    public void OnSapphireToggleChanged(bool isOn) => OnColorToggleChangedCore("sapphire", isOn);
    public void OnSteelToggleChanged(bool isOn) => OnColorToggleChangedCore("steel", isOn);

    /** 色フィルターを更新してカード選択エリアを再表示 */
    private void OnColorToggleChangedCore(string color, bool isOn)
    {
        string normalizedColor = (color ?? "").Trim().ToLower();

        if (isOn)
        {
            if (!activeColorFilters.Contains(normalizedColor))
                activeColorFilters.Add(normalizedColor);
        }
        else
        {
            if (activeColorFilters.Contains(normalizedColor))
                activeColorFilters.Remove(normalizedColor);
        }

        RefreshSelectableCardList();
    }

    /** コストフィルターを更新してカード選択エリアを再表示 */
    public void OnCostToggleChanged(int cost, bool isOn)
    {
        if (cost >= 1 && cost <= 10)
        {
            activeCostFilters[cost] = isOn;
            RefreshSelectableCardList(); // 再描画
        }
    }

    /** デッキ名の変更 */
    private void OnDeckNameChanged(string newName)
    {
        if (currentDeck != null)
        {
            currentDeck.deckName = newName;
            OnSaveDeck();
            Debug.Log($"デッキ名変更＆保存: {newName}");
        }
    }

    /** デッキの保存 */
    void OnSaveDeck()
    {
        DeckDataList list = DeckStorage.LoadDecks();
        int index = list.decks.FindIndex(d => d.deckId == currentDeck.deckId);
        if (index >= 0)
        {
            list.decks[index] = currentDeck;
        }
        else
        {
            list.decks.Add(currentDeck);
        }
        DeckStorage.SaveDecks(list);
        Debug.Log("デッキ保存完了");
    }

    [SerializeField] private GameObject zoomCanvas;
    [SerializeField] private Image zoomImage;

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
}
