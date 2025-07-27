using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ColorEntity", menuName = "Create ColorEntity")]

public class ColorEntity : ScriptableObject
{
    // カードのユニークID
    public int colorId;

    // カードの収録シリーズ
    public new string colorName;

    // カードの表画像
    public Sprite colorIcon;
}
