using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public List<CardSO> allCards = new List<CardSO>();

    private int currentIndex = 0;

    private void Start()
    {
        CardSO[] cards = Resources.LoadAll<CardSO>("Card");

        allCards.AddRange(cards);
    }
    public void DrawCard(HandManager handManager)
    {
        if(allCards.Count == 0)
            return;

        CardSO nexCard = allCards[currentIndex];
        handManager.AddCardToHand(nexCard);
        currentIndex = (currentIndex + 1) % allCards.Count;
    }
}
