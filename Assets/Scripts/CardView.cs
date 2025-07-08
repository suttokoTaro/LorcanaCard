using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardView : MonoBehaviour
{
    // カード画像表示用
    [SerializeField] Image iconImage;

    // カード枚数表示用テキスト（デッキ編集画面にて使用想定）
    [SerializeField] Text countText;

    [SerializeField] private Text damageText;
    [SerializeField] private GameObject damagePanel; // 表示パネル（非表示可）

    /**カードの表画像の表示 */
    public void ShowIcon(CardModel cardModel)
    {
        if (iconImage != null)
            iconImage.sprite = cardModel.icon;
    }

    /**カードの裏画像の表示 */
    public void ShowBackIcon(CardModel cardModel)
    {
        if (iconImage != null)
            iconImage.sprite = cardModel.backIcon;
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

    /** ダメージポイントの更新 */
    public void UpdateDamage(int amount)
    {
        damageText.text = amount.ToString();
        damagePanel.SetActive(true);
    }
    /** ダメージパネルの表示操作 */
    public void SetDamagePanelVisible(bool visible)
    {
        if (damagePanel != null)
            damagePanel.SetActive(visible);
    }
}