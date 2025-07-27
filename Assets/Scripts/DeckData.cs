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

    public List<int> cardIDs = new List<int>();
}

