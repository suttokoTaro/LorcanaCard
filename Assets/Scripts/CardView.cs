using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardView : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text countText;
    [SerializeField] Image iconImage;

    public void Show(CardModel cardModel)
    {
        if (iconImage != null)
            iconImage.sprite = cardModel.icon;

        if (nameText != null)
            nameText.text = $"Card ID: {cardModel.cardId}";
    }

    public void SetCount(int count)
    {
        if (countText != null)
            countText.text = $"×{count}";
    }

    public void HideCount()
    {
        if (countText != null)
            countText.text = "";
    }
}