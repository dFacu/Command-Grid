// HandManager.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HandManager : MonoBehaviour
{
    public Transform handPanel;
    public GameObject cardButtonPrefab;
    public PlayerResources owner; // asignar desde Inspector
    public TurnManager turnManager;
    public BoardManager boardManager;

    public List<Card> startingCardSO; // arrastrar assets en Inspector

    private List<CardInHand> hand = new List<CardInHand>();

    void Start()
    {
        foreach (var so in startingCardSO)
            hand.Add(new CardInHand(so));

        RenderHand();
    }

    void RenderHand()
    {
        foreach (Transform t in handPanel) Destroy(t.gameObject);

        foreach (var cardInHand in hand)
        {
            GameObject btn = Instantiate(cardButtonPrefab, handPanel);
            Text txt = btn.GetComponentInChildren<Text>();
            txt.text = $"{cardInHand.cardSO.cardName} ({cardInHand.cardSO.cost} PC)";
            Button b = btn.GetComponent<Button>();
            b.onClick.AddListener(() => OnCardClicked(cardInHand));
        }
    }

    void OnCardClicked(CardInHand cardInHand)
    {
        // Por defecto jugamos sin target; si la carta necesita target, abrir selector de celda
        CardContext ctx = new CardContext
        {
            owner = owner,
            targetCell = null,
            targetUnit = null,
            turnManager = turnManager,
            boardManager = boardManager
        };

        // Si la carta es UnitCardSO, podrías abrir un selector de celda y luego llamar Play con targetCell
        cardInHand.Play(ctx);
        RenderHand();
    }
}
