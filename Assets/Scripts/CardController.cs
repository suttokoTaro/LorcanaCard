using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardController : MonoBehaviour, IPointerClickHandler
{
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
        if (areaName.Contains("field") || areaName.Contains("ink"))
        {
            ToggleTap();
        }
    }
    private void ToggleTap()
    {
        isTapped = !isTapped;
        float rotationZ = isTapped ? 90f : 0f;
        transform.localRotation = Quaternion.Euler(0f, 0f, rotationZ);
    }
}