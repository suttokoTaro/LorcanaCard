using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckCardListInBattleUIPrefab : MonoBehaviour
{

    public bool isFront;
    public CardEntity cardEntity;

    public Button deckPanel;
    public Image imageIcon;
    public Button handButton;
    public Button trushButton;
    public Button bottomButton;


    public void createDeckCardFront(int cardId)
    {
        isFront = true;
        cardEntity = Resources.Load<CardEntity>($"CardEntityList/Card_{cardId}");
        imageIcon.sprite = cardEntity.icon;
    }

    public void changeFrontAndBack()
    {
        if (isFront)
        {
            imageIcon.sprite = cardEntity.backIcon;
            handButton.interactable = false;
            trushButton.interactable = false;
            bottomButton.interactable = false;
        }
        if (!isFront)
        {
            imageIcon.sprite = cardEntity.icon;
            handButton.interactable = true;
            trushButton.interactable = true;
            bottomButton.interactable = true;
        }
        isFront = !isFront;
    }
}
