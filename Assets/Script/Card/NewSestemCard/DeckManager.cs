using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckManager : MonoBehaviour
{
    public List<CardSO> allCards = new List<CardSO>();

    public int startingHandSize = 6;

    public int maxHandSize = 12;

    public int currentHandSize;

    private HandManager handManager;

    private DrawPileManager drawPileManager;

    private bool startBattleRun = true;


    void Start()
    {
        //Load all card assets from the Resources folder
        CardSO[] cards = Resources.LoadAll<CardSO>("Cards");

        //Add the loaded cards to the allCards list
        allCards.AddRange(cards);
    }

    private void Awake()
    {
        if (drawPileManager == null)
        {
            drawPileManager = FindObjectOfType<DrawPileManager>();
        }
        if (handManager == null)
        {
            handManager = FindObjectOfType<HandManager>();
        }
    }

    private void Update()
    {
        if (startBattleRun)
        {
            BattleSetup();
        }
    }

    public void BattleSetup()
    {
        handManager.BattleSetup(maxHandSize);
        drawPileManager.MakeDrawPile(allCards);
        drawPileManager.BattleSetup(startingHandSize, maxHandSize);
        startBattleRun = false;
    }
}
