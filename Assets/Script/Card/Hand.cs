using UnityEngine;
using System.Collections.Generic;

public class Hand : MonoBehaviour
{
    public List<CardSO> cardsInHand = new List<CardSO>();
    public int maxHandSize = 4;

    public GameObject prefabCard;
    public Transform handPanel;
    
    public void AddCard(CardSO card)
    {
        if (cardsInHand.Count < maxHandSize)
            cardsInHand.Add(card);
    }
}
