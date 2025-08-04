using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// フィールドにアタッチするクラス
public class DropPlace : MonoBehaviour, IDropHandler
{
    [SerializeField] private BattleUI battleUI;
    public void OnDrop(PointerEventData eventData) // ドロップされた時に行う処理
    {
        CardMovement cardMove = eventData.pointerDrag.GetComponent<CardMovement>(); // ドラッグしてきた情報からCardMovementを取得
        if (cardMove != null) // もしカードがあれば、
        {
            var beforeArea = cardMove.cardParent;
            Debug.Log("移動前の場所：" + beforeArea.name.ToLower());
            if (beforeArea.name.ToLower().Contains("playerdeck"))
            {
                //battleUI.RemoveTopDeckMenuCard(beforeArea);
            }

            // 移動処理
            cardMove.cardParent = this.transform;

            var afterArea = cardMove.cardParent;
            Debug.Log("移動後の場所：" + afterArea.name.ToLower());

            // 表裏を切り替える（カードに CardController がある前提）
            var cardCtrl = eventData.pointerDrag.GetComponent<CardController>();
            if (cardCtrl != null)
            {
                bool isFront = ShouldBeFront(this.transform);

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

            string areaName = this.transform.name.ToLower();
            if (areaName.Contains("location"))
            {
                float rotationZ = -90f;
                cardCtrl.transform.localRotation = Quaternion.Euler(0f, 0f, rotationZ);
            }

            // ドロップ後にデッキ枚数更新
            // if (BattleUI.Instance != null)
            // {
            //     BattleUI.Instance.UpdateDeckCountText();
            // }
            battleUI.UpdateDeckCountText();
        }
    }
    private bool ShouldBeFront(Transform area)
    {
        string areaName = area.name.ToLower();
        return areaName.Contains("hand") || areaName.Contains("field") || areaName.Contains("trash") || areaName.Contains("item") || areaName.Contains("location");
    }
}