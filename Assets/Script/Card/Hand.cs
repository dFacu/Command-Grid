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

    public void PlayCard(int index, PlayerResources owner)
    {
        if (index < 0 || index >= cardsInHand.Count) return;

        CardSO card = cardsInHand[index];
        CardExecutor.Play(card, owner);
        cardsInHand.RemoveAt(index);
    }
}
