using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public List<CardSO> allCards = new List<CardSO>();

    public int startingHandSize = 6;

    private int currentIndex = 0;
    public int maxHandSize;
    public int currentHandSize;
    private HandManager handManager;

    void Start()
    {
        //Load all card assets from the Resources folder
        CardSO[] cards = Resources.LoadAll<CardSO>("Cards");

        //Add the loaded cards to the allCards list
        allCards.AddRange(cards);

        handManager = FindObjectOfType<HandManager>();
        maxHandSize = handManager.maxHandSize;
        for (int i = 0; i < startingHandSize; i++)
        {
            Debug.Log($"Drawing Card");
            DrawCard(handManager);
        }
    }

    void Update()
    {
        if (handManager != null)
        {
            currentHandSize = handManager.cardsInHand.Count;
        }
    }

    public void DrawCard(HandManager handManager)
    {
        if (allCards.Count == 0)
            return;

        if (currentHandSize < maxHandSize)
        {
            CardSO nextCard = allCards[currentIndex];
            handManager.AddCardToHand(nextCard);
            currentIndex = (currentIndex + 1) % allCards.Count;
        }
    }
}
