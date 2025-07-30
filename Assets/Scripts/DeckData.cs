using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DeckData
{
    public string deckId;
    public string deckName;
    public int color1;
    public int color2;
    public int leaderCardId;
    public string createdAt; // ISO 8601形式などで保存
    public string updatedAt;

    public List<int> cardIDs = new List<int>();
}

