using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance;
    public Hand hand;

    public string usuarioName;

    public List<CardSO> DeckCargado;


    private void Awake()
    {
        Instance = this;
    }

    public CardSO DrawCard()
    {
        if (DeckCargado.Count == 0) return null;

        CardSO draw = DeckCargado[0];
        DeckCargado.RemoveAt(0);

        GameObject cardObj = Instantiate(hand.prefabCard, hand.handPanel);
        cardObj.GetComponent<CardUI>().LoadCard(draw);

        Button btn = cardObj.GetComponent<Button>();
        btn.onClick.AddListener(() =>
        {
            CardExecutor.Play(draw, PlayerResources.Instance);

            // Eliminar de la mano
            hand.cardsInHand.Remove(draw);

            // Eliminar de la UI
            Destroy(cardObj);
        });
            return draw;
    }
    public void InitPlayer(List<CardSO> startingDeck)
    {
        //Roba x cartas iniciales
        for (int i = 0; i < 4; i++)
        {
            CardSO card = DrawCard();
            if (card != null)
                hand.AddCard(card);
        }
    }
}
