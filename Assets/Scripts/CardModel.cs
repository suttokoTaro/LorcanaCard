using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CardModel
{
    // カードのユニークID
    public int cardId;
    // カードの表画像
    public Sprite icon;
    // カードの裏画像
    public Sprite backIcon;
    // カード名
    public String name;
    // カード色
    public String color;
    // カードコスト
    public int cost;
    // カード耐久力
    public int endurePoint;
    // カード攻撃力 
    public int power;


    public CardModel(int cardID)
    {
        CardEntity cardEntity = Resources.Load<CardEntity>("CardEntityList/Card_" + cardID);

        cardId = cardEntity.cardId;
        icon = cardEntity.icon;
        backIcon = cardEntity.backIcon;
        name = cardEntity.name;
        color = cardEntity.color;
        cost = cardEntity.cost;
        endurePoint = cardEntity.endurePoint;
        power = cardEntity.power;

    }
}