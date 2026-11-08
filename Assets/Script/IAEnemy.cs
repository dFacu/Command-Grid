using System.Collections;
using System.Collections.Generic;
using TMPro;
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


    public TextMeshProUGUI textPoint;
    public TextMeshProUGUI textLifeComander;

    private bool lastCardPlayed;

    void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        textLifeComander.text = Manager.Instance.enemy.units[0].GetComponent<Unit>().health.ToString();
        textPoint.text = Manager.Instance.enemy.currentPoints.ToString();
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
        lastCardPlayed = false;

        TileClick tile = null;
        Unit target = null;
        Unit sourceUnit = null;

        switch (card.effectType)
        {
            case EffectType.SpawnUnit:

                tile = BoardManager.Instance.FindFreeTile(
                    Manager.Instance.enemy
                );

                if (tile == null)
                {
                    Debug.LogWarning("IA: No encontró una casilla para invocar.");
                    yield break;
                }

                Debug.Log($"IA: Invocando {card.cardName} en {tile.cellCoords}");

                break;

            case EffectType.Damage:

                target = BoardManager.Instance.FindBestEnemy(
                    Owner.AI,
                    card.distance,
                    card,
                    out sourceUnit
                );

                if (target == null || sourceUnit == null)
                {
                    Debug.LogWarning("IA: No encontró un objetivo para atacar.");
                    yield break;
                }

                Debug.Log($"IA: {sourceUnit.name} ataca a {target.name}");

                break;
        }

        bool success = CardExecutor.Instance.PlayExecuter(
            card,
            tile,
            target,
            Manager.Instance.enemy,
            Manager.Instance.player,
            sourceUnit
        );

        if (success)
        {
            currentMana -= card.cost;
            deckManager.Discard(card);
            hand.Remove(card);

            lastCardPlayed = true;

            Debug.Log($"IA ejecutó correctamente: {card.cardName}");
        }

        yield return new WaitForSeconds(playDelay);
    }

    private IEnumerator PlayTurnRoutine()
    {
        yield return new WaitForSeconds(0.3f);

        // Primero mover las unidades.
        yield return StartCoroutine(MoveUnitsRoutine());

        // Después comenzar a jugar cartas.

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
                Debug.Log(
                    $"IA eligió: {best.cardName} | " +
                    $"Efecto: {best.effectType} | " +
                    $"Coste: {best.cost} | Maná: {currentMana}"
                );

                yield return StartCoroutine(PlayCardRoutine(best));
                playedSomething = lastCardPlayed;
            }

            safety++;
            yield return new WaitForSeconds(playDelay);
        }

        // Cuando termina realmente su turno → avisa al TurnManager
        TurnManager.Instance.EndEnemyTurn();
    }



    private bool CanPlayCard(CardSO card)
    {
        if (card.cost > currentMana)
            return false;

        if (card.effectType == EffectType.SpawnUnit)
        {
            if (BoardManager.Instance.FindFreeTile(Manager.Instance.enemy) == null)
                return false;
        }

        if (card.effectType == EffectType.Damage)
        {
            Unit source;

            Unit target = BoardManager.Instance.FindBestEnemy(
                Owner.AI,
                card.distance,
                card,
                out source
            );

            if (target == null || source == null)
                return false;
        }

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

    private IEnumerator MoveUnitsRoutine()
    {
        PlayerResources enemy = TurnManager.Instance.enemy;
        PlayerResources player = TurnManager.Instance.player;


        foreach (GameObject obj in enemy.units)
        {
            if (obj == null)
                continue;

            Unit unit = obj.GetComponent<Unit>();

            if (unit == null || unit.isPetrified)
                continue;

            Unit closestEnemy = null;
            float closestDistance = float.MaxValue;

            // Buscar el enemigo más cercano.
            foreach (GameObject targetObj in player.units)
            {
                if (targetObj == null)
                    continue;

                Unit target = targetObj.GetComponent<Unit>();

                if (target == null)
                    continue;

                float distance = Vector2.Distance(
                    unit.currentCell,
                    target.currentCell
                );

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = target;
                }
            }

            if (closestEnemy == null)
                continue;

            // Buscar una casilla libre que acerque a la unidad.
            TileClick bestTile = null;
            float bestDistance = closestDistance;

            TileClick[] cells =
                FindObjectsByType<TileClick>(FindObjectsSortMode.None);

            foreach (TileClick cell in cells)
            {
                if (cell == null || cell.occupied)
                    continue;

                int movement = unit.movementRange;

                if (EffectZone.IsInsideMovementPenalty(unit, cell.cellCoords))
                    movement = Mathf.CeilToInt(movement * 0.5f);

                int distanceFromUnit =
                    Mathf.Abs(cell.cellCoords.x - unit.currentCell.x) +
                    Mathf.Abs(cell.cellCoords.y - unit.currentCell.y);

                if (distanceFromUnit == 0 || distanceFromUnit > movement)
                    continue;

                float distanceToEnemy = Vector2.Distance(
                    cell.cellCoords,
                    closestEnemy.currentCell
                );

                if (distanceToEnemy < bestDistance)
                {
                    bestDistance = distanceToEnemy;
                    bestTile = cell;
                }
            }

            if (bestTile != null)
            {
                unit.MoveTo(bestTile.cellCoords);

                yield return new WaitForSeconds(0.3f);
            }
        }
    }

    private Unit GetSourceUnitForCard(CardSO card)
    {
        PlayerResources enemy = TurnManager.Instance.enemy;

        foreach (GameObject obj in enemy.units)
        {
            if (obj == null) continue;

            Unit unit = obj.GetComponent<Unit>();

            if (unit == null || unit.isPetrified)
                continue;

            if (unit.HasElement != Element.All &&
                unit.HasElement != card.elements)
                continue;

            return unit;
        }

        return null;
    }
}
