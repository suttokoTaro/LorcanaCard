using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardView : MonoBehaviour
{
    // カード名表示用テキスト（不要想定）
    [SerializeField] Text nameText;
    // カード枚数表示用テキスト（デッキ編集画面にて使用想定）
    [SerializeField] Text countText;
    // カードの表画像（デッキ編集画面、マリガン画面、対戦画面で使用想定）
    [SerializeField] Image iconImage;

    public void Show(CardModel cardModel)
    {
        if (iconImage != null)
            iconImage.sprite = cardModel.icon;

        if (nameText != null)
            nameText.text = $"Card ID: {cardModel.cardId}";
    }

    /** カード枚数のセット（デッキ編集画面で使用する） */
    public void SetCount(int count)
    {
        if (countText != null)
            countText.text = $"×{count}";
    }

    /** カード枚数の非表示（デッキ編集画面で使用する） */
    public void HideCount()
    {
        if (countText != null)
            countText.text = "";
    }
}