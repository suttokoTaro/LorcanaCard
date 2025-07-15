using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.EventSystems;
using System.Linq;

public class CardListUUI : MonoBehaviour
{
    [SerializeField] public Transform cardListContent;
    [SerializeField] private SwipeDetector swipeDetector;
    public GameObject cardItemPrefab;

    [SerializeField] private Canvas filterCanvas;
    [SerializeField] private GameObject zoomCanvas;
    [SerializeField] private Image zoomImage;
    [SerializeField] private Text filteredCardCountText;

    private List<string> cardColors = new List<string> { "umber", "amethyst", "emerald", "ruby", "sapphire", "steel" };
    private List<string> activeColorFilters = new List<string>();
    private bool[] activeCostFilters = new bool[11]; // index 1〜10を使用


    void Start()
    {
        // カード表示エリアの初期表示
        RefreshCardList();

        LinkedFilterToggles();


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

        Debug.LogWarning("全カード数：" + allCardEntities.Length);

        // フィルター条件に合致したカードリスト
        List<CardEntity> filteredCardEntities = new List<CardEntity>();

        foreach (CardEntity cardEntity in allCardEntities)
        {
            // カードentityに登録されている色
            string cardColor = cardEntity.color?.Trim().ToLower();

            // 色フィルター内容と合致しているかどうか（色フィルターが一つも選択されていない場合もtrue）
            bool matchesColor = activeColorFilters.Count == 0 ||
            activeColorFilters.Exists(f => f.Trim().ToLower() == cardColor);

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
            // フィルターが1つもONでなければ全表示
            bool anyCostSelected = false;
            for (int i = 1; i <= 10; i++)
                if (activeCostFilters[i]) { anyCostSelected = true; break; }
            if (!anyCostSelected) matchesCost = true;


            // 各フィルターに内容と合致している場合は表示対象とする
            if (matchesColor && matchesCost)
            {
                filteredCardEntities.Add(cardEntity);
            }
        }

        Debug.LogWarning("フィルター後のカード数：" + filteredCardEntities.Count);
        if (filteredCardCountText != null)
        {
            filteredCardCountText.text = $"表示枚数：{filteredCardEntities.Count}枚";
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

        int currentIndex = 0;
        // 表示生成（フィルター条件に合致したカードリスト）
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
                //Debug.LogWarning("拡大表示したカードのindex：" + index);
                // 拡大表示
                ShowZoom(iconImage.sprite, index);
            });
            currentIndex = currentIndex + 1;
        }
    }

    /** 各フィルターのToggleとチェック時処理の紐づけ */
    private void LinkedFilterToggles()
    {
        // フィルターエリアの各色Toggleに対して、それぞれチェック時の処理メソッドを紐づけする
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

        /**
        umberToggle.onValueChanged.AddListener((isOn) => { OnColorToggleChangedCore("umber", isOn); });
        amethystToggle.onValueChanged.AddListener((isOn) => { OnColorToggleChangedCore("amethyst", isOn); });
        emeraldToggle.onValueChanged.AddListener((isOn) => { OnColorToggleChangedCore("emerald", isOn); });
        rubyToggle.onValueChanged.AddListener((isOn) => { OnColorToggleChangedCore("ruby", isOn); });
        sapphireToggle.onValueChanged.AddListener((isOn) => { OnColorToggleChangedCore("sapphire", isOn); });
        steelToggle.onValueChanged.AddListener((isOn) => { OnColorToggleChangedCore("steel", isOn); });
        */

        // フィルターエリアの1～9のToggleに対して、それぞれチェック時の処理メソッドを紐づけする
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

    }

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
        //Debug.LogWarning("色フィルター更新：" + string.Join(", ", activeColorFilters));

        RefreshCardList();
    }

    /** コストフィルターを更新してカード選択エリアを再表示 */
    private void OnCostToggleChanged(int cost, bool isOn)
    {
        if (cost >= 1 && cost <= 10)
        {
            activeCostFilters[cost] = isOn;
            RefreshCardList();
        }
    }


    /** フィルター画面の表示 */
    public void ShowFilterCanvas()
    {
        if (filterCanvas != null)
        {
            //filterCanvas.SetActive(true);
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
            //filterCanvas.SetActive(false);
            filterCanvas.sortingOrder = -10;
    }
    private void ShowZoom(Sprite sprite, Text index)
    {
        if (zoomCanvas != null && zoomImage != null)
        {
            swipeDetector.SetCurrentIndex(int.Parse(index.text));
            zoomImage.sprite = sprite;
            zoomCanvas.SetActive(true);
        }
    }
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
