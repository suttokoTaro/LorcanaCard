using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.EventSystems;

public class CardItemUITrial : MonoBehaviour
{
    [SerializeField] private Image iconImage;

    [SerializeField] private Image dammyIconImage;

    public void SetCard(CardEntity entity)
    {
        iconImage.sprite = entity.icon;
    }
    public void SetDammyCard()
    {
        iconImage = dammyIconImage;
    }


}
