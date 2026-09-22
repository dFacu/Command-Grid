using System.Collections.Generic;
using UnityEngine;

public class HandManager : MonoBehaviour
{
    public DeckManager deckManager;
    public GameObject cardPrefab;
    public Transform handTransform;

    public float fanSpread = 7f;
    public float cardSpacing = 100f;
    public float vercticalSpacing = 10f;

    public List<GameObject> cardsInHand = new List<GameObject>();

    private void Start()
    {
    }

    private void Update()
    {
        UpdateHandVisual();
    }

    public void AddCardToHand(CardSO cardData)
    {
        GameObject newCard = Instantiate(cardPrefab, handTransform.position, Quaternion.identity, handTransform);
        cardsInHand.Add(newCard);

        newCard.GetComponent<CardUI>().LoadCard(cardData);

        UpdateHandVisual();

    }

    private void UpdateHandVisual()
    {
        int cardCount = cardsInHand.Count;

        if(cardCount == 1)
        {
            cardsInHand[0].transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            cardsInHand[0].transform.localPosition = new Vector3(0f, 0f, 0f);
            return;
        }


        for (int i = 0; i < cardCount; i++)
        {
            float rotationAngle = (fanSpread * (i - (cardCount - 1) / 2f));
            cardsInHand[i].transform.localRotation = Quaternion.Euler(0f, 0f, rotationAngle);

            float horizontalOffset = (cardSpacing * (i - (cardCount - 1) / 2f));
            float normalizedPosition = (2f * i / (cardCount - 1) - 1f);

            float verticalOffset = vercticalSpacing * (1 - normalizedPosition * normalizedPosition);
            cardsInHand[i].transform.localPosition = new Vector3(horizontalOffset, verticalOffset, 0f);
        }
    }
}
