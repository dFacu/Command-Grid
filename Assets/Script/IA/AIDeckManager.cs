using System.Collections.Generic;
using UnityEngine;

public class AIDeckManager : MonoBehaviour
{
    public List<CardSO> allCards = new List<CardSO>();


    public List<CardSO> deck = new List<CardSO>();
    public List<CardSO> discard = new List<CardSO>();

    public int startingHandSize = 6;

    public int maxHandSize = 12;

    public int currentHandSize;

    private IAEnemy handManager;

    private bool startBattleRun = true;

    private void Start()
    {
        //Load all card assets from the Resources folder
        CardSO[] cards = Resources.LoadAll<CardSO>("Cards");

        //Add the loaded cards to the allCards list
        allCards.AddRange(cards);

        ShuffleDeck();
    }


    public void ShuffleDeck()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            CardSO tmp = deck[i];
            int r = Random.Range(i, deck.Count);
            deck[i] = deck[r];
            deck[r] = tmp;
        }
    }

    public CardSO DrawCard()
    {
        if(deck.Count == 0)
        {
            if(discard.Count == 0) return null;
            deck.AddRange(discard);
            discard.Clear();
            ShuffleDeck();
        }

        CardSO top = deck[0];
        deck.RemoveAt(0);
        return top;
    }

    public void Discard(CardSO card) { if (card != null) discard.Add(card); }
}
