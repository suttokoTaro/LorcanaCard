using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.EventSystems;

public class DeckBuilderUI : MonoBehaviour
{
    [SerializeField] private InputField deckNameInputField;
    [SerializeField] private Toggle umberToggle;
    [SerializeField] private Toggle amethystToggle;
    [SerializeField] private Toggle emeraldToggle;
    [SerializeField] private Toggle rubyToggle;
    [SerializeField] private Toggle sapphireToggle;
    [SerializeField] private Toggle steelToggle;

    public Text deckNameText;

    public Transform deckCardListContent; // デッキ内カード表示用
    public Transform allCardListContent;  // 全カード一覧表示用

    public GameObject cardItemPrefab;

    public Button saveButton;
    public Button backButton;

    private DeckData currentDeck;
    private List<string> activeColorFilters = new List<string>();

    private string originalDeckId;

    void Start()
    {
        currentDeck = SelectedDeckData.selectedDeck;

        if (currentDeck == null)
        {
            Debug.LogError("currentDeck が null");
            return;
        }
        if (string.IsNullOrEmpty(currentDeck.deckId))
        {
            currentDeck.deckId = System.Guid.NewGuid().ToString(); // 古いデッキにIDを補完
        }
        originalDeckId = currentDeck.deckId;

        if (deckNameInputField != null)
        {
            deckNameInputField.text = currentDeck.deckName;
            deckNameInputField.onEndEdit.AddListener(OnDeckNameChanged);
        }


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

        RefreshDeckCardList();
        GenerateAllCardList();

        saveButton.onClick.AddListener(OnSaveDeck);
        backButton.onClick.AddListener(() => SceneManager.LoadScene("DeckListScene"));
        // 他の初期化のあとに
        if (deckNameInputField != null && currentDeck != null)
        {
            deckNameInputField.text = currentDeck.deckName;

            // 入力変更時に保存する
            deckNameInputField.onEndEdit.AddListener(OnDeckNameChanged);
        }


    }

    void RefreshDeckCardList()
    {
        foreach (Transform child in deckCardListContent)
            Destroy(child.gameObject);

        var cardCountDict = new Dictionary<int, int>();
        foreach (int id in currentDeck.cardIDs)
        {
            if (!cardCountDict.ContainsKey(id))
                cardCountDict[id] = 0;
            cardCountDict[id]++;
        }

        // 🔽 カード情報と枚数をまとめたリストを作成
        List<(CardEntity entity, int count)> cardList = new List<(CardEntity, int)>();
        foreach (var pair in cardCountDict)
        {
            var entity = Resources.Load<CardEntity>($"CardEntityList/Card_{pair.Key}");
            if (entity != null)
                cardList.Add((entity, pair.Value));
        }

        // 🔽 ソート：コスト昇順 → ID昇順
        cardList.Sort((a, b) =>
        {
            int costCompare = a.entity.cost.CompareTo(b.entity.cost);
            if (costCompare != 0) return costCompare;
            return a.entity.cardId.CompareTo(b.entity.cardId);
        });

        // 🔽 UI生成
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

            item.GetComponent<Button>().onClick.AddListener(() =>
            {
                currentDeck.cardIDs.Remove(entity.cardId); // 1枚削除
                RefreshDeckCardList();
            });
        }
    }

    void GenerateAllCardList()
    {
        // まずカード一覧を完全に削除
        foreach (Transform child in allCardListContent)
        {
            Destroy(child.gameObject);
        }

        // 全カード読み込み
        CardEntity[] allEntities = Resources.LoadAll<CardEntity>("CardEntityList");

        // フィルター：色で絞り込み
        List<CardEntity> filtered = new List<CardEntity>();
        foreach (CardEntity entity in allEntities)
        {
            Debug.Log($"[ColorCheck] cardId: {entity.cardId}, color: '{entity.color}'");

            string cardColor = entity.color?.Trim().ToLower();
            bool matchesFilter = activeColorFilters.Count == 0 ||
                                 activeColorFilters.Exists(f => f.Trim().ToLower() == cardColor);

            if (matchesFilter)
            {
                Debug.Log($"→ 表示: {entity.cardId}");
                filtered.Add(entity);
            }
        }

        // ソート：cardId 昇順
        filtered.Sort((a, b) => a.cardId.CompareTo(b.cardId));

        // 表示生成（フィルター後のカードだけ！）
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
                HideZoom();
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

        Debug.Log("active filters: " + string.Join(",", activeColorFilters));
        GenerateAllCardList();
    }

    private void OnDeckNameChanged(string newName)
    {
        if (currentDeck != null)
        {
            currentDeck.deckName = newName;
            OnSaveDeck(); // ← ここを DeckManager.SaveDeck(currentDeck) ではなくこれに変更
            Debug.Log($"デッキ名変更＆保存: {newName}");
        }
    }

    void OnSaveDeck()
    {
        DeckDataList list = DeckStorage.LoadDecks();

        // 🔁 deckName ではなく deckId で判定
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
        yield return new WaitForSeconds(0.4f);

        if (zoomCanvas != null && zoomImage != null)
        {
            zoomImage.sprite = sprite;
            zoomCanvas.SetActive(true);
        }
    }

    private void HideZoom()
    {
        if (zoomCanvas != null)
            zoomCanvas.SetActive(false);
    }
}
