
using System.Collections.Generic;
using UnityEngine;

public class AIAnalyzer
{
    public class AIAttackOption
    {
        public Unit sourceUnit;
        public Unit target;
        public int score;
        public int expectedDamage;
    }

    public AIAttackOption FindBestDamagePlay(
        CardSO card,
        PlayerResources aiOwner,
        PlayerResources playerOwner)
    {
        if (card == null || aiOwner == null || playerOwner == null)
            return null;

        AIAttackOption bestOption = null;

        foreach (GameObject allyObject in aiOwner.units)
        {
            if (allyObject == null)
                continue;

            Unit ally = allyObject.GetComponent<Unit>();

            if (ally == null || ally.isPetrified)
                continue;

            if (ally.owner != aiOwner)
                continue;

            // Comprobar compatibilidad elemental.
            if (ally.HasElement != Element.All &&
                ally.HasElement != card.elements)
                continue;

            foreach (GameObject enemyObject in playerOwner.units)
            {
                if (enemyObject == null)
                    continue;

                Unit enemy = enemyObject.GetComponent<Unit>();

                if (enemy == null || enemy.owner != playerOwner)
                    continue;

                // Comprobar alcance.
                float distance = Vector3.Distance(
                    ally.transform.position,
                    enemy.transform.position
                );

                if (distance > card.distance)
                    continue;

                // Calcular daño incluyendo bonus de zonas.
                int damage = card.value +
                    EffectZone.GetTotalAttackBonus(ally);

                // Evaluar esta combinación.
                int score = 30;

                score += damage * 3;

                // Premiar especialmente una eliminación.
                if (enemy.health <= damage)
                {
                    score += 100;
                }

                // Priorizar enemigos con poca vida.
                score += Mathf.Max(0, 20 - enemy.health);

                // Preferir objetivos más cercanos.
                score -= Mathf.RoundToInt(distance * 2);

                if (bestOption == null || score > bestOption.score)
                {
                    bestOption = new AIAttackOption
                    {
                        sourceUnit = ally,
                        target = enemy,
                        score = score,
                        expectedDamage = damage
                    };
                }
            }
        }

        return bestOption;
    }

    public class AIHealOption
    {
        public Unit sourceUnit;
        public Unit target;
        public int score;
        public int actualHeal;
    }

    public AIHealOption FindBestHealPlay(
    CardSO card,
    PlayerResources aiOwner)
    {
        if (card == null || aiOwner == null)
            return null;

        AIHealOption bestOption = null;

        foreach (GameObject sourceObject in aiOwner.units)
        {
            if (sourceObject == null) continue;

            Unit source = sourceObject.GetComponent<Unit>();

            if (source == null || source.isPetrified)
                continue;

            foreach (GameObject targetObject in aiOwner.units)
            {
                if (targetObject == null) continue;

                Unit target = targetObject.GetComponent<Unit>();

                if (target == null)
                    continue;

                // No tiene sentido curar una unidad con vida completa.
                int missingHealth = target.maxHealth - target.health;

                if (missingHealth <= 0)
                    continue;

                // Comprobar alcance.
                float distance = Vector3.Distance(
                    source.transform.position,
                    target.transform.position
                );

                if (distance > card.distance)
                    continue;

                // Calcular curación real, sin superar la vida máxima.
                int actualHeal = Mathf.Min(
                    card.value,
                    missingHealth
                );

                float missingRatio =
                    (float)missingHealth / target.maxHealth;

                // Evaluación estratégica.
                int score = 0;

                score += actualHeal * 4;
                score += Mathf.RoundToInt(missingRatio * 40);
                score -= Mathf.RoundToInt(distance * 2);

                if (bestOption == null || score > bestOption.score)
                {
                    bestOption = new AIHealOption
                    {
                        sourceUnit = source,
                        target = target,
                        score = score,
                        actualHeal = actualHeal
                    };
                }
            }
        }

        return bestOption;
    }

    public class AIShieldOption
    {
        public Unit sourceUnit;
        public Unit target;
        public int score;
    }

    public AIShieldOption FindBestShieldPlay(
    CardSO card,
    PlayerResources aiOwner,
    PlayerResources playerOwner)
    {
        if (card == null || aiOwner == null || playerOwner == null)
            return null;

        AIShieldOption bestOption = null;

        foreach (GameObject sourceObject in aiOwner.units)
        {
            if (sourceObject == null) continue;

            Unit source = sourceObject.GetComponent<Unit>();

            if (source == null || source.isPetrified)
                continue;

            // Comprobar compatibilidad elemental.
            if (source.HasElement != Element.All &&
                source.HasElement != card.elements)
                continue;

            foreach (GameObject targetObject in aiOwner.units)
            {
                if (targetObject == null) continue;

                Unit target = targetObject.GetComponent<Unit>();

                if (target == null)
                    continue;

                // Comprobar alcance entre quien aplica el escudo
                // y quien lo recibe.
                float distance = Vector3.Distance(
                    source.transform.position,
                    target.transform.position
                );

                if (distance > card.distance)
                    continue;

                // Calcular qué tan vulnerable está la unidad.
                float missingHealthRatio =
                    (float)(target.maxHealth - target.health)
                    / target.maxHealth;

                // Cuanto más escudo tenga, menor será la prioridad.
                float existingShieldRatio =
                    (float)target.shield / target.maxHealth;

                // Buscar enemigos cercanos.
                float nearestEnemyDistance = float.MaxValue;

                foreach (GameObject enemyObject in playerOwner.units)
                {
                    if (enemyObject == null) continue;

                    Unit enemy = enemyObject.GetComponent<Unit>();

                    if (enemy == null)
                        continue;

                    float enemyDistance = Vector3.Distance(
                        target.transform.position,
                        enemy.transform.position
                    );

                    if (enemyDistance < nearestEnemyDistance)
                        nearestEnemyDistance = enemyDistance;
                }

                int score = 0;

                // Prioridad por vida faltante.
                score += Mathf.RoundToInt(missingHealthRatio * 50);

                // Prioridad por cercanía al enemigo.
                if (nearestEnemyDistance <= 1.5f)
                    score += 35;
                else if (nearestEnemyDistance <= 3f)
                    score += 20;
                else if (nearestEnemyDistance <= 5f)
                    score += 10;

                // El escudo existente reduce la prioridad.
                score -= Mathf.RoundToInt(existingShieldRatio * 40);

                // Penalizar ligeramente la distancia de aplicación.
                score -= Mathf.RoundToInt(distance * 2);

                // No desperdiciar la carta sobre una unidad
                // que ya está muy protegida.
                if (target.shield >= card.value * 2)
                    score -= 40;

                if (bestOption == null || score > bestOption.score)
                {
                    bestOption = new AIShieldOption
                    {
                        sourceUnit = source,
                        target = target,
                        score = score
                    };
                }
            }
        }

        return bestOption;
    }

    public class AIAreaOption
    {
        public Unit sourceUnit;
        public TileClick targetTile;
        public int score;
        public int enemiesHit;
        public int enemiesKilled;
    }

    public AIAreaOption FindBestDamageAreaPlay(
    CardSO card,
    PlayerResources aiOwner,
    PlayerResources playerOwner)
    {
        if (card == null || aiOwner == null || playerOwner == null)
            return null;

        if (GridControler.Instance == null)
            return null;

        AIAreaOption bestOption = null;

        int width = GridControler.Instance.width;
        int height = GridControler.Instance.height;

        foreach (GameObject sourceObject in aiOwner.units)
        {
            if (sourceObject == null) continue;

            Unit source = sourceObject.GetComponent<Unit>();

            if (source == null || source.isPetrified)
                continue;

            // Compatibilidad elemental.
            if (source.HasElement != Element.All &&
                source.HasElement != card.elements)
                continue;

            // Analizar todas las casillas del tablero como centro del área.
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector2Int center = new Vector2Int(x, y);

                    TileClick tile = BoardManager.Instance.GetTile(center);

                    if (tile == null)
                        continue;

                    // Comprobar distancia entre la unidad y el centro del área.
                    float distance = Vector3.Distance(
                        source.transform.position,
                        tile.transform.position
                    );

                    if (distance > card.distance)
                        continue;

                    // Obtener las casillas afectadas usando tu sistema actual.
                    List<TileClick> affectedTiles =
                        BoardManager.Instance.GetAreaTiles(
                            center,
                            card.areaShape,
                            card.areaSize
                        );

                    if (affectedTiles == null || affectedTiles.Count == 0)
                        continue;

                    int enemiesHit = 0;
                    int enemiesKilled = 0;

                    foreach (GameObject enemyObject in playerOwner.units)
                    {
                        if (enemyObject == null) continue;

                        Unit enemy = enemyObject.GetComponent<Unit>();

                        if (enemy == null)
                            continue;

                        // Comprobar si el enemigo está dentro del área.
                        bool insideArea = false;

                        foreach (TileClick affectedTile in affectedTiles)
                        {
                            if (affectedTile != null &&
                                affectedTile.cellCoords == enemy.currentCell)
                            {
                                insideArea = true;
                                break;
                            }
                        }

                        if (!insideArea)
                            continue;

                        enemiesHit++;

                        if (enemy.health <= card.value)
                            enemiesKilled++;
                    }

                    // No utilizar la carta si no alcanza a ningún enemigo.
                    if (enemiesHit == 0)
                        continue;

                    int score = 0;

                    // Premiar la cantidad de enemigos alcanzados.
                    score += enemiesHit * 50;

                    // Premiar especialmente las eliminaciones.
                    score += enemiesKilled * 100;

                    // Priorizar ligeramente los centros más cercanos.
                    score -= Mathf.RoundToInt(distance * 2);

                    if (bestOption == null || score > bestOption.score)
                    {
                        bestOption = new AIAreaOption
                        {
                            sourceUnit = source,
                            targetTile = tile,
                            score = score,
                            enemiesHit = enemiesHit,
                            enemiesKilled = enemiesKilled
                        };
                    }
                }
            }
        }

        return bestOption;
    }

    public class AISpawnOption
    {
        public TileClick tile;
        public int score;
    }

    public AISpawnOption FindBestSpawnPlay(
    CardSO card,
    PlayerResources aiOwner,
    PlayerResources playerOwner)
    {
        if (card == null || card.prefab == null ||
            aiOwner == null || playerOwner == null ||
            GridControler.Instance == null)
            return null;

        // Comprobar que el prefab tenga el componente Unit.
        Unit prefabUnit = card.prefab.GetComponent<Unit>();

        if (prefabUnit == null)
        {
            Debug.LogWarning(
                $"IA: El prefab de {card.cardName} no tiene componente Unit."
            );
            return null;
        }

        AISpawnOption bestOption = null;

        GridControler grid = GridControler.Instance;

        for (int x = 0; x < grid.width; x++)
        {
            for (int y = 0; y < grid.height; y++)
            {
                TileClick tile = grid.gridCells[x, y].GetComponent<TileClick>();

                if (tile == null || tile.occupied || tile.hasTrap)
                    continue;

                Vector2Int candidateCell = tile.cellCoords;

                int nearestEnemyDistance = int.MaxValue;
                int nearestAllyDistance = int.MaxValue;

                // Buscar el enemigo más cercano a esta posición.
                foreach (GameObject enemyObject in playerOwner.units)
                {
                    if (enemyObject == null) continue;

                    Unit enemy = enemyObject.GetComponent<Unit>();

                    if (enemy == null) continue;

                    int distance =
                        Mathf.Abs(candidateCell.x - enemy.currentCell.x) +
                        Mathf.Abs(candidateCell.y - enemy.currentCell.y);

                    nearestEnemyDistance =
                        Mathf.Min(nearestEnemyDistance, distance);
                }

                // Buscar el aliado más cercano.
                foreach (GameObject allyObject in aiOwner.units)
                {
                    if (allyObject == null) continue;

                    Unit ally = allyObject.GetComponent<Unit>();

                    if (ally == null) continue;

                    int distance =
                        Mathf.Abs(candidateCell.x - ally.currentCell.x) +
                        Mathf.Abs(candidateCell.y - ally.currentCell.y);

                    nearestAllyDistance =
                        Mathf.Min(nearestAllyDistance, distance);
                }

                int score = 0;

                // 1. Preferir posiciones cercanas a los enemigos.
                if (nearestEnemyDistance != int.MaxValue)
                {
                    score += Mathf.Max(
                        0,
                        30 - nearestEnemyDistance * 4
                    );
                }

                // 2. Mantener cierta cercanía con los aliados,
                // evitando colocar unidades demasiado aisladas.
                if (nearestAllyDistance != int.MaxValue)
                {
                    score += Mathf.Max(
                        0,
                        15 - nearestAllyDistance * 3
                    );
                }

                // 3. Si la unidad tiene mucha vida, puede ocupar
                // posiciones más avanzadas.
                if (nearestEnemyDistance <= 2 &&
                    prefabUnit.maxHealth >= 15)
                {
                    score += 10;
                }

                if (bestOption == null || score > bestOption.score)
                {
                    bestOption = new AISpawnOption
                    {
                        tile = tile,
                        score = score
                    };
                }
            }
        }

        return bestOption;
    }

    public class AIPetrifyOption
    {
        public int score;
        public int enemiesAffected;
        public List<Unit> targets = new List<Unit>();
    }

    public AIPetrifyOption FindBestPetrifyPlay(
    CardSO card,
    PlayerResources aiOwner,
    PlayerResources playerOwner)
    {
        if (card == null || aiOwner == null || playerOwner == null)
            return null;

        AIPetrifyOption option = new AIPetrifyOption();

        foreach (GameObject enemyObject in playerOwner.units)
        {
            if (enemyObject == null) continue;

            Unit enemy = enemyObject.GetComponent<Unit>();

            if (enemy == null || enemy.isPetrified)
                continue;

            float nearestAllyDistance = float.MaxValue;

            foreach (GameObject allyObject in aiOwner.units)
            {
                if (allyObject == null) continue;

                Unit ally = allyObject.GetComponent<Unit>();

                if (ally == null)
                    continue;

                float distance = Vector3.Distance(
                    enemy.transform.position,
                    ally.transform.position
                );

                nearestAllyDistance = Mathf.Min(
                    nearestAllyDistance,
                    distance
                );
            }

            int score = 0;

            // Priorizar enemigos que están cerca de nuestros aliados.
            if (nearestAllyDistance <= 1.5f)
                score += 35;
            else if (nearestAllyDistance <= 3f)
                score += 25;
            else if (nearestAllyDistance <= 5f)
                score += 15;

            // Una unidad con mucha vida puede seguir siendo
            // una amenaza durante más tiempo.
            float healthRatio = (float)enemy.health / enemy.maxHealth;
            score += Mathf.RoundToInt(healthRatio * 20);

            // Un enemigo con escudo puede ser más difícil de eliminar,
            // por lo que inmovilizarlo puede ser conveniente.
            if (enemy.shield > 0)
                score += 5;

            option.score += score;
            option.targets.Add(enemy);
        }

        option.enemiesAffected = option.targets.Count;

        if (option.enemiesAffected == 0)
            return null;

        // Premiar las cartas que afectan a varios enemigos.
        option.score += option.enemiesAffected * 30;

        return option;
    }

    public class AITerrainOption
    {
        public Unit sourceUnit;
        public TileClick targetTile;
        public int score;
        public int alliesBenefited;
        public int enemiesAffected;
    }

    public AITerrainOption FindBestTerrainPlay(
    CardSO card,
    PlayerResources aiOwner,
    PlayerResources playerOwner)
    {
        if (card == null || card.prefab == null ||
            aiOwner == null || playerOwner == null ||
            GridControler.Instance == null)
            return null;

        AITerrainOption bestOption = null;

        GridControler grid = GridControler.Instance;

        // Recorremos las unidades aliadas que pueden lanzar la carta.
        foreach (GameObject sourceObject in aiOwner.units)
        {
            if (sourceObject == null)
                continue;

            Unit source = sourceObject.GetComponent<Unit>();

            if (source == null || source.isPetrified)
                continue;

            if (source.HasElement != Element.All &&
                source.HasElement != card.elements)
                continue;

            // Recorremos todas las casillas del tablero.
            for (int x = 0; x < grid.width; x++)
            {
                for (int y = 0; y < grid.height; y++)
                {
                    TileClick tile = grid.gridCells[x, y].GetComponent<TileClick>();

                    if (tile == null)
                        continue;

                    float distance = Vector3.Distance(
                        source.transform.position,
                        tile.transform.position
                    );

                    if (distance > card.distance)
                        continue;

                    int score = 0;
                    int alliesBenefited = 0;
                    int enemiesAffected = 0;

                    // Calculamos el radio de la zona.
                    int radius = Mathf.Max(0, (card.areaSize - 1) / 2);

                    // Evaluamos las unidades aliadas.
                    foreach (GameObject allyObject in aiOwner.units)
                    {
                        if (allyObject == null)
                            continue;

                        Unit ally = allyObject.GetComponent<Unit>();

                        if (ally == null)
                            continue;

                        int dx = Mathf.Abs(ally.currentCell.x - tile.cellCoords.x);
                        int dy = Mathf.Abs(ally.currentCell.y - tile.cellCoords.y);

                        if (dx > radius || dy > radius)
                            continue;

                        // La zona beneficia al elemento favorecido.
                        if (card.effectForFavoredElement == ZoneEffectType.AttackBonus &&
                            ally.HasElement == card.favoredElement)
                        {
                            score += 40;
                            alliesBenefited++;
                        }

                        // La zona perjudica el movimiento de los demás elementos.
                        if (card.effectForOtherElements == ZoneEffectType.ReduceMovement &&
                            ally.HasElement != card.favoredElement)
                        {
                            score -= 25;
                        }
                    }

                    // Evaluamos las unidades enemigas.
                    foreach (GameObject enemyObject in playerOwner.units)
                    {
                        if (enemyObject == null)
                            continue;

                        Unit enemy = enemyObject.GetComponent<Unit>();

                        if (enemy == null)
                            continue;

                        int dx = Mathf.Abs(enemy.currentCell.x - tile.cellCoords.x);
                        int dy = Mathf.Abs(enemy.currentCell.y - tile.cellCoords.y);

                        if (dx > radius || dy > radius)
                            continue;

                        // Evitamos beneficiar a los enemigos.
                        if (card.effectForFavoredElement == ZoneEffectType.AttackBonus &&
                            enemy.HasElement == card.favoredElement)
                        {
                            score -= 40;
                        }

                        // Reducir el movimiento enemigo es beneficioso.
                        if (card.effectForOtherElements == ZoneEffectType.ReduceMovement &&
                            enemy.HasElement != card.favoredElement)
                        {
                            score += 35;
                            enemiesAffected++;
                        }
                    }

                    // Penalizamos ligeramente las zonas muy alejadas de la fuente.
                    score -= Mathf.RoundToInt(distance * 2);

                    if (bestOption == null || score > bestOption.score)
                    {
                        bestOption = new AITerrainOption
                        {
                            sourceUnit = source,
                            targetTile = tile,
                            score = score,
                            alliesBenefited = alliesBenefited,
                            enemiesAffected = enemiesAffected
                        };
                    }
                }
            }
        }

        // Si no existe ningún beneficio estratégico, no usamos la carta.
        if (bestOption == null || bestOption.score <= 0)
            return null;

        return bestOption;
    }
}
