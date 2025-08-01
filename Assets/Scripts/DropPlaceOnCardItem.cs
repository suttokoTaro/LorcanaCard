
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// フィールドにアタッチするクラス
public class DropPlaceOnCardItem : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData) // ドロップされた時に行う処理
    {
        CardMovement cardMove = eventData.pointerDrag.GetComponent<CardMovement>(); // ドラッグしてきた情報からCardMovementを取得
        if (cardMove != null) // もしカードがあれば、
        {
            // 移動処理
            if (!this.transform.parent.name.ToLower().Contains("location"))
            {
                cardMove.cardParent = this.transform.parent;
            }

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
            if (BattleUI.Instance != null)
            {
                BattleUI.Instance.UpdateDeckCountText();
            }
        }
    }
    private bool ShouldBeFront(Transform area)
    {
        string areaName = area.name.ToLower();
        return areaName.Contains("hand") || areaName.Contains("field") || areaName.Contains("trash") || areaName.Contains("item") || areaName.Contains("location");
    }
}