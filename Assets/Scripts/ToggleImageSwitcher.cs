using UnityEngine;
using UnityEngine.UI;

public class ToggleImageSwitcher : MonoBehaviour
{
    [SerializeField] public Toggle toggle;          // 対象のToggle
    [SerializeField] public Image iconImage;        // 切り替え対象のImage
    [SerializeField] public Sprite normalSprite;    // チェック前の画像
    [SerializeField] public Sprite checkedSprite;   // チェック時の画像

    void Start()
    {
        if (toggle != null)
        {
            //toggle.isOn = false; // ← 初期状態を非選択に設定
            toggle.onValueChanged.AddListener(OnToggleChanged);

            OnToggleChanged(toggle.isOn); // 初期状態の反映

            OnToggleChanged(false);
            // // 初期画像を手動でセット（false の場合）
            // if (iconImage != null && normalSprite != null)
            // {
            //     iconImage.sprite = normalSprite;
            // }
        }
    }

    void OnToggleChanged(bool isOn)
    {
        if (iconImage != null)
        {
            iconImage.sprite = isOn ? checkedSprite : normalSprite;
        }
    }
}
