using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.XR;
using static UnityEngine.UI.GridLayoutGroup;

public class IAEnemy : MonoBehaviour
{

    public static IAEnemy Instance;

    public List<CardSO> hand = new List<CardSO>();
    public AIDeckManager deckManager;
    public int maxHandSize = 6;
    public int currentMana = 0;
    public int maxMana = 3;
    public float playDelay = 0.5f;


    void Awake()
    {
        Instance = this;
    }

    public void InitPlayer(List<CardSO> startingDeck)
    {
        // Copiar el mazo inicial
        deckManager.deck = new List<CardSO>(startingDeck);

        // Barajar
        deckManager.ShuffleDeck();

        // Limpiar mano
        hand.Clear();

        // Robar cartas iniciales (ejemplo: 4)
        for (int i = 0; i < 4; i++)
        {
            CardSO card = deckManager.DrawCard();
            if (card != null)
                hand.Add(card);
        }

        Debug.Log("IA inicializada con mazo aleatorio y mano inicial.");
    }

    public void PlayTurn()
    {
        currentMana = Mathf.Min(maxMana, currentMana + 1);

        if (hand.Count < maxHandSize)
        {
            CardSO drawn = deckManager.DrawCard();
            if (drawn != null) hand.Add(drawn);
        }

        StartCoroutine(PlayTurnRoutine());
    }

    private IEnumerator PlayCardRoutine(CardSO card)
    {
        TileClick tile = null;
        Unit target = null;

        switch (card.effectType)
        {
            case EffectType.SpawnUnit:
                tile = BoardManager.Instance.FindFreeTile(TurnManager.Instance.enemy);

                if (tile == null)
                {
                    Debug.Log("La IA no encontró una casilla libre para invocar.");
                    yield break;
                }
                break;
                /*
            case EffectType.Trap:
                tile = BoardManager.Instance.FindTrapTile(Manager.Instance.enemy);
                if (tile == null)
                    yield break;
                break;

            case EffectType.Damage:
                target = BoardManager.Instance.FindBestEnemy(
                    Owner.AI,
                    card.distance
                );*/

                if (target == null)
                    yield break;
                break;
        }

        bool success = CardExecutor.Instance.PlayExecuter(
            card,
            tile,
            target,
            TurnManager.Instance.enemy
        );

        if (success)
        {
            currentMana -= card.cost;

            deckManager.Discard(card);
            hand.Remove(card);

            Debug.Log($"La IA jugó la carta: {card.name}");
        }

        yield return new WaitForSeconds(playDelay);
    }

    private IEnumerator PlayTurnRoutine()
    {
        yield return new WaitForSeconds(0.3f);

        bool playedSomething = true;
        int safety = 0;

        while (playedSomething && safety < 10)
        {
            playedSomething = false;
            CardSO best = null;
            int bestScore = int.MinValue;

            foreach (CardSO card in hand)
            {
                if (!CanPlayCard(card)) continue;
                int score = EvaluateCard(card);
                if (score > bestScore)
                {
                    bestScore = score;
                    best = card;
                }
            }

            if (best != null)
            {
                yield return StartCoroutine(PlayCardRoutine(best));
                playedSomething = true;
            }

            safety++;
            yield return new WaitForSeconds(playDelay);
        }

        // Cuando termina realmente su turno → avisa al TurnManager
        TurnManager.Instance.EndEnemyTurn();
    }



    private bool CanPlayCard(CardSO card)
    {
        if (card.cost > currentMana) return false;
        if (card.effectType == EffectType.SpawnUnit && !BoardManager.Instance.HasBoardSpace(Owner.AI)) return false;
        return true;
    }
    private int EvaluateCard(CardSO card)
    {
        int score = 0;

        switch (card.effectType)
        {
            case EffectType.Damage:
                if (BoardManager.Instance.EnemyInRange(Owner.AI)) score += 30;
                score += card.value;
                break;

            case EffectType.SpawnUnit:
                if (BoardManager.Instance.HasBoardSpace(Owner.AI)) score += 25;
                score += card.value;
                break;

            case EffectType.Heal:
                if (BoardManager.Instance.CommanderHealthBelow(Owner.AI, 0.6f)) score += 35;
                score += card.value;
                break;

            case EffectType.Trap:
                score += 20;
                break;
        }

        if (card.cost > currentMana) score -= 1000;

        return score;
    }

}
