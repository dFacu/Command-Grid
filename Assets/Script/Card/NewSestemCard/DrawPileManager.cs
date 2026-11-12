using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Playables;
using SinuousProductions;
using Utility = SinuousProductions.Utility;

public class DrawPileManager : MonoBehaviour
{
    public List<CardSO> drawPile = new List<CardSO>();

    public int startingHandSize = 6;

    private int currentIndex = 0;
    public int maxHandSize;
    public int currentHandSize;
    private HandManager handManager;
    private DiscardManager discardManager;
    public TextMeshProUGUI drawPileCounter;
    void Start()
    {
        handManager = FindObjectOfType<HandManager>();
    }

    void Update()
    {
        if (handManager != null)
        {
            currentHandSize = handManager.cardsInHand.Count;
        }
    }

    public void MakeDrawPile(List<CardSO> cardsToAdd)
    {
        drawPile.Clear();

        drawPile.AddRange(cardsToAdd);

        drawPile.RemoveAll(card => card == null);

        Utility.Shuffle(drawPile);

        UpdateDrawPileCount();

        Debug.Log($"Mazo del jugador mezclado: {drawPile.Count} cartas.");
    }

    public void BattleSetup(int numberOfCardsToDraw, int setMaxHandSize)
    {
        maxHandSize = setMaxHandSize;
        for (int i = 0; i < numberOfCardsToDraw; i++)
        {
            DrawCard(handManager);
        }
    }
    public void DrawCard(HandManager handManager)
    {
        if (drawPile.Count == 0)
        {
            RefillDeckFromDiscard();
        }

        if (drawPile.Count == 0)
            return;

        currentHandSize = handManager.cardsInHand.Count;

        if (currentHandSize >= maxHandSize)
            return;

        CardSO nextCard = drawPile[0];

        handManager.AddCardToHand(nextCard);

        drawPile.RemoveAt(0);

        currentHandSize = handManager.cardsInHand.Count;

        UpdateDrawPileCount();
    }

    private void RefillDeckFromDiscard()
    {
        if (discardManager == null)
        {
            discardManager = FindObjectOfType<DiscardManager>();
        }
    }
    private void UpdateDrawPileCount()
    {
        drawPileCounter.text = drawPile.Count.ToString();
    }
}
