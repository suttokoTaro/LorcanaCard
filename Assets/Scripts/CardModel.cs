using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CardModel
{
    public int cardId;
    public Sprite icon;
    public String name;
    public String color;
    public int cost;
    public int endurePoint;
    public int power;


    public CardModel(int cardID)
    {
        CardEntity cardEntity = Resources.Load<CardEntity>("CardEntityList/Card_" + cardID);

        cardId = cardEntity.cardId;
        icon = cardEntity.icon;
        name = cardEntity.name;
        color = cardEntity.color;
        cost = cardEntity.cost;
        endurePoint = cardEntity.endurePoint;
        power = cardEntity.power;
        
    }
}