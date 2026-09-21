using System.Collections.Generic;
using UnityEngine;

public class Deck 
{ 
    private List<CardSO> cards = new List<CardSO>();

    public Deck (List<CardSO> initialCard)
    {
        cards = new List<CardSO>(initialCard);
    }

    public CardSO DrawCard()
    {
        if(cards.Count == 0) return null;

        CardSO draw = cards[0];
        cards.RemoveAt(0);
        return draw;
    }

}
