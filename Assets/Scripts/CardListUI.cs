using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.EventSystems;
using System.Linq;

public class CardListUI : MonoBehaviour
{
    [SerializeField] public Transform cardListContent;
    [SerializeField] private SwipeDetector swipeDetector;
    public GameObject cardItemPrefab;

    [SerializeField] private Canvas filterCanvas;
    [SerializeField] private GameObject zoomCanvas;
    [SerializeField] private Image zoomImage;
    [SerializeField] private Text filteredCardCountText;
    [SerializeField] private InputField searchNameInputField;

    private List<string> cardTypes = new List<string> { "character", "action", "song", "item", "location" };
    private List<string> activeTypeFilters = new List<string>();
    private List<string> cardColors = new List<string> { "umber", "amethyst", "emerald", "ruby", "sapphire", "steel" };
    private List<string> activeColorFilters = new List<string>();
    private bool[] activeCostFilters = new bool[11]; // index 1〜10を使用
    private bool[] activeSeriesFilters = new bool[11]; // index 1〜10を使用

    private List<string> extraFilters = new List<string> { "bodyguard", "challenger", "evasive", "reckless", "resist", "rush", "shift", "singer", "singTogether", "support", "ward" };

    private List<string> rarities = new List<string> { "1_common", "2_uncommon", "3_rare", "4_superrare", "5_legendary" };
    private List<string> activeRarityFilters = new List<string>();


    void Start()
    {
        // カード表示エリアの初期表示
        RefreshCardList();

        // 各フィルターのToggleと押下時処理の紐づけ
        LinkedFilterToggles();

        // 入力が変更されたらカードリスト更新
        if (searchNameInputField != null)
        {
            searchNameInputField.onValueChanged.AddListener((_) => RefreshCardList());
        }
    }

    /** カード表示エリアの更新 */
    private void RefreshCardList()
    {
        // まずカード一覧を完全に削除
        foreach (Transform child in cardListContent)
        {
            Destroy(child.gameObject);
        }

        // 全カード読み込み
        CardEntity[] allCardEntities = Resources.LoadAll<CardEntity>("CardEntityList");
        // Debug.LogWarning("全カード数：" + allCardEntities.Length);

        // フィルター条件に合致したカードリスト
        List<CardEntity> filteredCardEntities = new List<CardEntity>();

        foreach (CardEntity cardEntity in allCardEntities)
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

            // シリーズフィルター内容と合致しているかどうか
            string cardIdStr = cardEntity.cardId.ToString();
            string cardSeries = cardIdStr.Length > 0 ? cardIdStr.Substring(0, 1).Trim().ToLower() : "";
            bool matchesSeries = false;
            for (int i = 1; i <= 10; i++)
            {
                if (activeSeriesFilters[i] && int.Parse(cardSeries) == i)
                {
                    matchesSeries = true;
                    break;
                }
            }
            // シリーズフィルターが1つもONでなければ全表示
            bool anySeriesSelected = false;
            for (int i = 1; i <= 10; i++)
                if (activeSeriesFilters[i]) { anySeriesSelected = true; break; }
            if (!anySeriesSelected) matchesSeries = true;

            // レアリティフィルター内容と合致しているかどうか（レアリティフィルターが一つも選択されていない場合もtrue）
            string cardRarity = cardEntity.rarity;
            bool matchesRarity = activeRarityFilters.Count == 0 ||
             (cardRarity != null && activeRarityFilters.Exists(f => cardRarity.Contains(f)));

            // 名前検索フィルター
            string searchText = searchNameInputField?.text?.Trim().ToLower();
            Debug.LogWarning("検索文字列：" + searchText);
            bool matchesName = string.IsNullOrEmpty(searchText) ||
                               (cardEntity.name != null && cardEntity.name.ToLower().Contains(searchText)) ||
                               (cardEntity.versionName != null && cardEntity.versionName.ToLower().Contains(searchText));

            // 各フィルターに内容と合致している場合は表示対象とする
            if (matchesCardType && matchesColor && matchesCost && matchesSeries & matchesRarity & matchesName)
            {
                filteredCardEntities.Add(cardEntity);
            }
        }

        Debug.LogWarning("フィルター後のカード数：" + filteredCardEntities.Count);
        if (filteredCardCountText != null)
        {
            filteredCardCountText.text = $"{filteredCardEntities.Count}";
        }

        // ソート：cardId 昇順
        //filteredCardEntities.Sort((a, b) => a.cardId.CompareTo(b.cardId));
        // ソート：色 → コスト → カードID 昇順
        filteredCardEntities = filteredCardEntities
            .OrderBy(c => (c.color ?? "").Trim().ToLower()) // 色
            .ThenBy(c => c.cost)                            // コスト
            .ThenBy(c => c.cardId)                          // カードID
            .ToList();

        // スワイプビューアに渡す
        swipeDetector.SetCardList(filteredCardEntities);

        // 表示生成（フィルター条件に合致したカードリスト）
        int currentIndex = 0;
        foreach (CardEntity cardEntity in filteredCardEntities)
        {
            int cardId = cardEntity.cardId;
            GameObject item = Instantiate(cardItemPrefab, cardListContent);

            // 画像表示
            Image iconImage = item.transform.Find("Image")?.GetComponent<Image>();
            if (iconImage != null) iconImage.sprite = cardEntity.icon;

            Text countText = item.transform.Find("CountText")?.GetComponent<Text>();
            Transform countTextPanel = item.transform.Find("CountTextPanel");
            if (countText != null) countText.text = "";
            if (countTextPanel != null) countTextPanel.gameObject.SetActive(false);

            Text index = item.transform.Find("Index")?.GetComponent<Text>();
            index.text = currentIndex.ToString(); ;

            // ボタンに追加処理
            item.GetComponent<Button>().onClick.AddListener(() =>
            {
                // 拡大表示
                ShowZoom(iconImage.sprite, index);
            });
            currentIndex = currentIndex + 1;
        }
    }

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
                // 暫定対処：フィルター初期値を全選択とする
                activeTypeFilters.Add(cardType);
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
                //colorToggle.isOn = false;
                // 暫定対処：フィルター初期値を全選択とする
                activeColorFilters.Add(cardColor);
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
                //costToggle.isOn = false;
                // 暫定対処：フィルター初期値を全選択とする
                activeCostFilters[i] = true;
            }
            else
            {
                Debug.LogWarning($"CostToggle_{i} が見つかりません");
            }
        }

        // Extraの各Toggleに対して、それぞれ押下時処理の処理メソッドを紐づけする
        foreach (string extraFilter in extraFilters)
        {
            var extraToggle = GameObject.Find($"ExtraToggle_{extraFilter}")?.GetComponent<Toggle>();
            if (extraToggle != null)
            {
                extraToggle.onValueChanged.AddListener((isOn) => { OnExtraToggleChangedCore(extraFilter, isOn); });
            }
            else
            {
                Debug.LogWarning($"ExtraToggle_{extraFilter} が見つかりません");
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
                //seriesToggle.isOn = false;
                // 暫定対処：フィルター初期値を全選択とする
                activeSeriesFilters[j] = true;
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
                // 暫定対処：フィルター初期値を全選択とする
                activeRarityFilters.Add(rarity);
            }
            else
            {
                Debug.LogWarning($"RarityToggle_{rarity} が見つかりません");
            }
        }


    }

    /** カードタイプフィルターを更新してカード選択エリアを再表示 */
    private void OnCardTypeToggleChangedCore(string cardType, bool isOn)
    {
        //string normalizedCardType = (cardType ?? "").Trim().ToLower();
        string normalizedCardType = cardType;

        if (isOn)
        {
            if (!activeTypeFilters.Contains(normalizedCardType))
                activeTypeFilters.Add(normalizedCardType);
        }
        else
        {
            if (activeTypeFilters.Contains(normalizedCardType))
                activeTypeFilters.Remove(normalizedCardType);
        }
        Debug.LogWarning("カードタイプフィルター更新：" + string.Join(", ", activeTypeFilters));
        RefreshCardList();
    }

    /** 色フィルターを更新してカード選択エリアを再表示 */
    private void OnColorToggleChangedCore(string color, bool isOn)
    {
        //string normalizedColor = (color ?? "").Trim().ToLower();
        string normalizedColor = color;

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
        Debug.LogWarning("色フィルター更新：" + string.Join(", ", activeColorFilters));
        RefreshCardList();
    }

    /** コストフィルターを更新してカード選択エリアを再表示 */
    private void OnCostToggleChanged(int cost, bool isOn)
    {
        if (cost >= 1 && cost <= 10)
        {
            activeCostFilters[cost] = isOn;
            Debug.LogWarning("コストフィルター更新：" + string.Join(", ", activeCostFilters));
            RefreshCardList();
        }
    }

    private void OnExtraToggleChangedCore(string extraFilter, bool isOn)
    {
        Debug.LogWarning("Extraフィルター紐づき確認：" + extraFilter);
    }

    /** シリーズフィルターを更新してカード選択エリアを再表示 */
    private void OnSeriesToggleChangedCore(int cardSeries, bool isOn)
    {
        string strCardSeries = cardSeries.ToString();

        if (cardSeries >= 1 && cardSeries <= 10)
        {
            activeSeriesFilters[cardSeries] = isOn;
            Debug.LogWarning("シリーズフィルター更新：" + string.Join(", ", activeSeriesFilters));
            RefreshCardList();
        }
    }

    /** レアリティフィルターを更新してカード選択エリアを再表示 */
    private void OnRarityToggleChangedCore(string rarity, bool isOn)
    {
        //string normalizedRarity = (rarity ?? "").Trim().ToLower();
        string normalizedRarity = rarity;

        if (isOn)
        {
            if (!activeRarityFilters.Contains(normalizedRarity))
                activeRarityFilters.Add(normalizedRarity);
        }
        else
        {
            if (activeRarityFilters.Contains(normalizedRarity))
                activeRarityFilters.Remove(normalizedRarity);
        }
        Debug.LogWarning("レアリティフィルター更新：" + string.Join(", ", activeRarityFilters));
        RefreshCardList();
    }


    /** フィルター画面の表示 */
    public void ShowFilterCanvas()
    {
        if (filterCanvas != null)
        {
            filterCanvas.sortingOrder = 10;
        }
    }

    /** すべてのフィルターをクリアする */
    public void DeselectAllTogglesInScene()
    {
        Toggle[] allToggles = GameObject.FindObjectsOfType<Toggle>(true); // 非アクティブも含む
        foreach (Toggle toggle in allToggles)
        {
            toggle.isOn = false;
        }
    }

    /** フィルター画面の非表示 */
    public void HideFilterCanvas()
    {
        if (filterCanvas != null)
            filterCanvas.sortingOrder = -10;
    }

    /** カードの拡大表示 */
    private void ShowZoom(Sprite sprite, Text index)
    {
        if (zoomCanvas != null && zoomImage != null)
        {
            swipeDetector.SetCurrentIndex(int.Parse(index.text));
            zoomImage.sprite = sprite;
            zoomCanvas.SetActive(true);
        }
    }

    /** カードの拡大表示の非表示 */
    public void HideZoom()
    {
        if (zoomCanvas != null)
        {
            zoomCanvas.SetActive(false);
            //Debug.LogWarning("Zoom非表示");
        }
    }

    /** 戻るボタン押下時処理：メインメニュー画面に戻る */
    public void OnClickBackToMainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
}
