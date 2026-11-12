using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.XR;
using static AIAnalyzer;
using static UnityEngine.UI.GridLayoutGroup;

public class IAEnemy : MonoBehaviour
{

    public static IAEnemy Instance;
    public AIAnalyzer aiAnalyzer;

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
        aiAnalyzer = new AIAnalyzer();
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

                AIAnalyzer.AISpawnOption spawn =
                    aiAnalyzer.FindBestSpawnPlay(
                        card,
                        Manager.Instance.enemy,
                        Manager.Instance.player
                    );

                if (spawn == null)
                {
                    Debug.LogWarning("IA: No encontró una posición para invocar.");
                    yield break;
                }

                tile = spawn.tile;

                Debug.Log(
                    $"IA: Invocará {card.cardName} en {tile.cellCoords}. " +
                    $"Score: {spawn.score}"
                );

                break;

            case EffectType.Damage:

                AIAnalyzer.AIAttackOption attack =
                    aiAnalyzer.FindBestDamagePlay(
                        card,
                        Manager.Instance.enemy,
                        Manager.Instance.player
                    );

                if (attack == null)
                {
                    Debug.LogWarning("IA: No encontró un ataque conveniente.");
                    yield break;
                }

                sourceUnit = attack.sourceUnit;
                target = attack.target;

                Debug.Log(
                    $"IA eligió atacar con {sourceUnit.name} " +
                    $"a {target.name}. " +
                    $"Daño esperado: {attack.expectedDamage} | " +
                    $"Puntuación: {attack.score}"
                );

                break;

                if (target == null || sourceUnit == null)
                {
                    Debug.LogWarning("IA: No encontró un objetivo para atacar.");
                    yield break;
                }

                Debug.Log($"IA: {sourceUnit.name} ataca a {target.name}");

                break;

            case EffectType.Heal:

                AIAnalyzer.AIHealOption heal =
                    aiAnalyzer.FindBestHealPlay(
                        card,
                        Manager.Instance.enemy
                    );

                if (heal == null)
                {
                    Debug.LogWarning("IA: No encontró una unidad para curar.");
                    yield break;
                }

                target = heal.target;
                sourceUnit = heal.sourceUnit;

                Debug.Log(
                    $"IA: {sourceUnit.name} cura a {target.name} " +
                    $"por {heal.actualHeal} puntos. Score: {heal.score}"
                );

                break;

            case EffectType.Shield:

                AIAnalyzer.AIShieldOption shield =
                    aiAnalyzer.FindBestShieldPlay(
                        card,
                        Manager.Instance.enemy,
                        Manager.Instance.player
                    );

                if (shield == null)
                {
                    Debug.LogWarning("IA: No encontró una unidad para proteger.");
                    yield break;
                }

                target = shield.target;
                sourceUnit = shield.sourceUnit;

                Debug.Log(
                    $"IA: {sourceUnit.name} aplica escudo a {target.name}. " +
                    $"Escudo actual: {target.shield} | Score: {shield.score}"
                );

                break;

            case EffectType.DamageArea:

                AIAnalyzer.AIAreaOption area =
                    aiAnalyzer.FindBestDamageAreaPlay(
                        card,
                        Manager.Instance.enemy,
                        Manager.Instance.player
                    );

                if (area == null)
                {
                    Debug.LogWarning("IA: No encontró un área válida para atacar.");
                    yield break;
                }

                tile = area.targetTile;
                sourceUnit = area.sourceUnit;

                Debug.Log(
                    $"IA: {sourceUnit.name} lanza un ataque de área en " +
                    $"{tile.cellCoords}. Enemigos alcanzados: {area.enemiesHit} | " +
                    $"Eliminaciones posibles: {area.enemiesKilled} | " +
                    $"Score: {area.score}"
                );

                break;

            case EffectType.Petrify:

                AIAnalyzer.AIPetrifyOption petrify =
                    aiAnalyzer.FindBestPetrifyPlay(
                        card,
                        Manager.Instance.enemy,
                        Manager.Instance.player
                    );

                if (petrify == null)
                {
                    Debug.LogWarning("IA: No hay enemigos para petrificar.");
                    yield break;
                }

                Debug.Log(
                    $"IA: Utilizará Petrify sobre {petrify.enemiesAffected} enemigos. " +
                    $"Score: {petrify.score}"
                );

                break;

            case EffectType.Terrain:
                {
                    AITerrainOption terrain = aiAnalyzer.FindBestTerrainPlay(
                        card,
                        Manager.Instance.enemy,
                        Manager.Instance.player
                    );

                    if (terrain == null)
                        yield break;

                    tile = terrain.targetTile;
                    sourceUnit = terrain.sourceUnit;

                    Debug.Log(
                        $"IA: colocando terreno en {tile.cellCoords}. " +
                        $"Aliados beneficiados: {terrain.alliesBenefited}. " +
                        $"Enemigos afectados: {terrain.enemiesAffected}."
                    );

                    break;
                }
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
            AIAnalyzer.AISpawnOption spawn =
                aiAnalyzer.FindBestSpawnPlay(
                    card,
                    Manager.Instance.enemy,
                    Manager.Instance.player
                );

            if (spawn == null)
                return false;
        }

        if (card.effectType == EffectType.Damage)
        {
            AIAnalyzer.AIAttackOption attack =
                aiAnalyzer.FindBestDamagePlay(
                    card,
                    Manager.Instance.enemy,
                    Manager.Instance.player
                );

            if (attack == null)
                return false;
        }

        if (card.effectType == EffectType.Heal)
        {
            AIAnalyzer.AIHealOption heal =
                aiAnalyzer.FindBestHealPlay(
                    card,
                    Manager.Instance.enemy
                );

            if (heal == null)
                return false;
        }

        if (card.effectType == EffectType.Shield)
        {
            AIAnalyzer.AIShieldOption shield =
                aiAnalyzer.FindBestShieldPlay(
                    card,
                    Manager.Instance.enemy,
                    Manager.Instance.player
                );

            if (shield == null)
                return false;
        }

        if (card.effectType == EffectType.DamageArea)
        {
            AIAnalyzer.AIAreaOption area =
                aiAnalyzer.FindBestDamageAreaPlay(
                    card,
                    Manager.Instance.enemy,
                    Manager.Instance.player
                );

            if (area == null)
                return false;
        }

        if (card.effectType == EffectType.Petrify)
        {
            AIAnalyzer.AIPetrifyOption petrify =
                aiAnalyzer.FindBestPetrifyPlay(
                    card,
                    Manager.Instance.enemy,
                    Manager.Instance.player
                );

            if (petrify == null)
                return false;
        }

        if (card.effectType == EffectType.Terrain)
        {
            AIAnalyzer.AITerrainOption Terrain = 
                aiAnalyzer.FindBestTerrainPlay(
                    card,
                    Manager.Instance.enemy,
                    Manager.Instance.player
                    );

            if (Terrain == null)
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

                AIAnalyzer.AIAttackOption attack =
                    aiAnalyzer.FindBestDamagePlay(
                        card,
                        Manager.Instance.enemy,
                        Manager.Instance.player
                    );

                if (attack == null)
                    return int.MinValue;

                score += attack.score;

                break;

            case EffectType.SpawnUnit:

                AIAnalyzer.AISpawnOption spawn =
                    aiAnalyzer.FindBestSpawnPlay(
                        card,
                        Manager.Instance.enemy,
                        Manager.Instance.player
                    );

                if (spawn == null)
                    return int.MinValue;

                score += 25 + spawn.score;

                break;

            case EffectType.Heal:

                AIAnalyzer.AIHealOption heal =
                    aiAnalyzer.FindBestHealPlay(
                        card,
                        Manager.Instance.enemy
                    );

                if (heal == null)
                    return int.MinValue;

                score += heal.score;

                break;

            case EffectType.Shield:

                AIAnalyzer.AIShieldOption shield =
                    aiAnalyzer.FindBestShieldPlay(
                        card,
                        Manager.Instance.enemy,
                        Manager.Instance.player
                    );

                if (shield == null)
                    return int.MinValue;

                score += shield.score;

                break;

            case EffectType.Trap:
                score += 20;
                break;

            case EffectType.DamageArea:

                AIAnalyzer.AIAreaOption area =
                    aiAnalyzer.FindBestDamageAreaPlay(
                        card,
                        Manager.Instance.enemy,
                        Manager.Instance.player
                    );

                if (area == null)
                    return int.MinValue;

                score += area.score;

                break;

            case EffectType.Petrify:

                AIAnalyzer.AIPetrifyOption petrify =
                    aiAnalyzer.FindBestPetrifyPlay(
                        card,
                        Manager.Instance.enemy,
                        Manager.Instance.player
                    );

                if (petrify == null)
                    return int.MinValue;

                score += petrify.score;

                break;

            case EffectType.Terrain:
                {
                    AITerrainOption terrain = aiAnalyzer.FindBestTerrainPlay(
                        card,
                        Manager.Instance.enemy,
                        Manager.Instance.player
                    );

                    if (terrain != null)
                        score += 20 + terrain.score;

                    break;
                }
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
