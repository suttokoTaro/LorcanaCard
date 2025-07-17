using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardEntity", menuName = "Create CardEntity")]

public class CardEntity : ScriptableObject
{
    // カードのユニークID
    public int cardId;

    // カードの収録シリーズ
    public new string setSeries;

    // カードの収録シリーズ
    public new string idInSetSeries;

    // カードの表画像
    public Sprite icon;

    // カードの裏画像
    public Sprite backIcon;

    // カード名
    public new string name;

    // カード名（バージョン名)
    public new string versionName;

    // カード色
    public new string color;

    // カードコスト
    public int cost;

    // カード種類
    public new string cardType;

    // カードクラス（特徴）
    public new string classification;

    // カード耐久力
    public int willpower;

    // カード攻撃力 
    public int strength;

    // カードロア値
    public int loreValue;

    // カードのレアリティ（灰丸= Common 白本= Uncommon 銅△= Rare 銀◇= Super Rare 金五= Legendary）
    public new string rarity;

    // カードのインクに使えるかどうか（0:インクなし、1:インクあり）
    public int inkwellFlag;

    // バニラ（0:バニラ以外、1:バニラ）
    public int vanillaFlag;

    // 護衛
    public int bodyguardFlag;

    // 果敢
    public int challengerFlag;

    // 回避
    public int evasiveFlag;

    // 暴勇
    public int recklessFlag;

    // 耐久
    public int resistFlag;

    // 突進
    public int rushFlag;

    // 変身
    public int shiftFlag;

    // 歌声
    public int singerFlag;

    // 合唱
    public int singTogetherFlag;

    // 支援
    public int supportFlag;

    // 魔除
    public int wardFlag;

}