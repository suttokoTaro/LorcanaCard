using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CardController : MonoBehaviour, IPointerClickHandler, IDropHandler
{

    [SerializeField] private Button plusButton;
    [SerializeField] private Button minusButton;
    public CardView view; // カードの見た目の処理
    public CardModel model; // カードのデータを処理
    public bool isTapped = false; // 縦か横かの状態保持

    private void Awake()
    {
        view = GetComponent<CardView>();
    }

    /** カードの生成（表画像表示） */
    public void CreateCardAndViewIcon(int cardID)
    {
        model = new CardModel(cardID);
        view.ShowIcon(model);
    }

    /** カードの生成（表画像表示） */
    public void CreateCardAndViewBackIcon(int cardID)
    {
        model = new CardModel(cardID);
        view.ShowBackIcon(model);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 現在の親が「FieldArea」かどうかを確認
        string areaName = transform.parent.name.ToLower();
        if (areaName.Contains("field") || areaName.Contains("ink") || areaName.Contains("item"))
        {
            ToggleTap();
        }
    }
    private void ToggleTap()
    {
        isTapped = !isTapped;
        float rotationZ = isTapped ? -90f : 0f;
        transform.localRotation = Quaternion.Euler(0f, 0f, rotationZ);
    }

    private void Start()
    {
        plusButton.onClick.AddListener(() => ModifyDamage(+1));
        minusButton.onClick.AddListener(() => ModifyDamage(-1));
        UpdateDamageButtonVisibility(); // 初期状態チェック
    }
    private void Update()
    {
        // 親が変わったときに自動判定
        UpdateDamageButtonVisibility();
    }
    private void UpdateDamageButtonVisibility()
    {
        string parentName = transform.parent?.name.ToLower();
        bool isInField = parentName != null && (parentName.Contains("field") || parentName.Contains("location"));

        //plusButton.gameObject.SetActive(isInField);
        //minusButton.gameObject.SetActive(isInField);
        view.SetDamagePanelVisible(isInField);
    }

    public void ModifyDamage(int delta)
    {
        model.AddDamage(delta);
        view.UpdateDamage(model.damage);
    }

    private BattleUI battleUI;
    public void Initialize(BattleUI ui)
    {
        battleUI = ui;
    }


    public void OnDrop(PointerEventData eventData) // ドロップされた時に行う処理
    {
        CardMovement cardMove = eventData.pointerDrag.GetComponent<CardMovement>(); // ドラッグしてきた情報からCardMovementを取得
        if (cardMove != null) // もしカードがあれば、
        {
            var beforeArea = cardMove.cardParent;
            //Debug.Log("移動前の場所：" + beforeArea.name.ToLower());

            // 移動処理
            if (this.transform.parent.name.ToLower().Contains("location")) { return; }

            cardMove.cardParent = this.transform.parent;

            var afterArea = cardMove.cardParent;
            //Debug.Log("移動後の場所：" + afterArea.name.ToLower());

            // 表裏を切り替える（カードに CardController がある前提）
            var cardCtrl = eventData.pointerDrag.GetComponent<CardController>();
            if (cardCtrl != null)
            {
                bool isFront = ShouldBeFront(this.transform.parent);

                if (isFront)
                {
                    cardCtrl.view.ShowIcon(cardCtrl.model);
                }
                if (!isFront)
                {
                    cardCtrl.view.ShowBackIcon(cardCtrl.model);
                }
            }
            cardCtrl.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

            // ドロップ後にデッキ枚数更新
            battleUI.UpdateDeckCountText();
        }
    }
    private bool ShouldBeFront(Transform area)
    {
        string areaName = area.name.ToLower();
        return areaName.Contains("hand") || areaName.Contains("field") || areaName.Contains("trash") || areaName.Contains("item") || areaName.Contains("location");
    }

}