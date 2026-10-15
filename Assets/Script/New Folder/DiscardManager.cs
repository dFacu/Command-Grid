using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class DiscardManager : MonoBehaviour
{
    [SerializeField] public List<CardSO> discardCards = new List<CardSO>();
    public TextMeshProUGUI discardCount;
    public int discardCardsCount;

    private void UpdateDiscardCount()
    {
        discardCount.text = discardCards.Count.ToString();
        discardCardsCount = discardCards.Count;
    }

    public void AddToDiscard(CardSO card)
    {
        if (card != null)
        {
            discardCards.Add(card);
            UpdateDiscardCount();
        }
    }

    public CardSO PullFromDiscard()
    {
        if(discardCards.Count > 0)
        {
            CardSO cardToReturn = discardCards[discardCards.Count - 1];
            discardCards.RemoveAt(discardCards.Count - 1);
            UpdateDiscardCount();
            return cardToReturn;
        }
        else
        {
            return null;
        }
    }

    public List<CardSO> PullAllFromDiscard()
    {
        if(discardCards.Count > 0 )
        {
            List<CardSO> cardsToReturn = new List<CardSO>(discardCards);
            discardCards.Clear();
            UpdateDiscardCount();
            return cardsToReturn;
        }
        else
        {
            return new List<CardSO>();
        }
    
    }
}
