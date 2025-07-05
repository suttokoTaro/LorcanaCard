using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardEntity", menuName = "Create CardEntity")]

public class CardEntity : ScriptableObject
{
    // カードのユニークID
    public int cardId;

    // カード画像
    public Sprite icon;

    // カード名
    public new string name;

    // カード色
    public new string color;

    // カードコスト
    public int cost;

    // カード耐久力
    public int endurePoint;

    // カード攻撃力 
    public int power;
    

}