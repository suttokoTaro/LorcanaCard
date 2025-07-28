using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeckCardSelectUI : MonoBehaviour
{
    public Transform deckCardListContent;
    void Start()
    {

    }
    /** 戻るボタン押下時の処理 */
    public void OnClickBackButton()
    {
        SceneManager.LoadScene("DeckDetailScene");
    }
}
