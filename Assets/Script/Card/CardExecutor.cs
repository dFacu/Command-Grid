using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.UIElements;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PushResult
{
    public bool moved;
}
public class CardExecutor : MonoBehaviour
{
    public static CardExecutor Instance;

    private void Awake()
    {
        Instance = this;
    }

    public bool PlayExecuter(CardSO card, TileClick tile, Unit target, PlayerResources owner, PlayerResources targetOwner, Unit sourceUnit)
    {
        if (card == null)
            return false;

        switch (card.effectType)
        {
            case EffectType.SpawnUnit:
                return SpawnUnit(card, tile, owner);
            case EffectType.Petrify:
                return PetrifyEnemies(card, owner, targetOwner);
            case EffectType.Damage:
                return DamageEnemy(card, target, owner, sourceUnit);
            case EffectType.Heal:
                return HealUnit(card, target, owner, sourceUnit);
            case EffectType.Trap:
                return PlaceTrap(card, tile, owner, sourceUnit);
            case EffectType.Terrain:
                return PlaceZone(card, tile, owner, sourceUnit);
            case EffectType.DamageArea:
                return DamageArea(card, tile, owner, targetOwner, sourceUnit);
            case EffectType.Shield:
                return AddShield(card, target, owner, sourceUnit);
            case EffectType.Knockback:
                return ApplyKnockback( card, tile, owner, targetOwner, sourceUnit );



        }

        return false;
    }


    private bool SpawnUnit(CardSO card, TileClick tile, PlayerResources owner)
    {
        if (tile == null)
            return false;

        if (card.prefab == null)
            return false;
        if (owner == null)
            return false;

        bool creada = GridControler.Instance.AddObjectToGrid(card.prefab, tile.cellCoords, owner);

        if (!creada)
        {
            Debug.Log("No se pudo invocar la unidad en esa casilla.");
            return false;
        }

        PlayCardEffect(card, tile.transform.position);

        Debug.Log($"Unidad invocada en {tile.cellCoords}");

        return true;

    }

    private bool PetrifyEnemies(CardSO card, PlayerResources owner, PlayerResources targets)
    {
        if (card == null || owner == null)
            return false;

        List<Unit> units = new List<Unit>();
        foreach (GameObject unit in targets.units)
        {
            units.Add(unit.GetComponent<Unit>());
        }

        bool affectedAny = false;

        foreach (Unit unit in units)
        {
            if (unit == null)
                continue;

            // Afectar únicamente a las unidades rivales.
            if (unit.owner.ownerType == owner.ownerType)
                continue;

            unit.Petrify(card.value);
            affectedAny = true;
        }

        Vector3 newVector3 = new Vector3(4,4,4);
        PlayCardEffect(card, newVector3);

        if (affectedAny)
        {
            Debug.Log(
                $"{card.name}: los enemigos fueron petrificados " +
                $"por {card.value} turnos."
            );
        }

        return affectedAny;
    }

    private bool DamageEnemy(CardSO card, Unit target, PlayerResources owner, Unit sourceUnit)
    {
        if (card == null || target == null || owner == null)
            return false;

        if (sourceUnit == null)
        {
            Debug.Log("Debés seleccionar una unidad para usar esta carta.");
            return false;
        }

        // Una unidad petrificada no puede usar cartas desde ella.
        if (sourceUnit.isPetrified)
        {
            Debug.Log("Una unidad petrificada no puede usar cartas.");
            return false;
        }

        // Comprobar que el elemento de la unidad permita usar la carta.
        if (sourceUnit.HasElement != card.elements && sourceUnit.HasElement != Element.All)
        {
            Debug.Log(
                $"{sourceUnit.name} no puede usar {card.cardName}. " +
                $"Elemento requerido: {card.elements}."
            );
            return false;
        }

        // No permitir atacar unidades aliadas.
        if (target.owner == null)
            return false;

        if (target.owner.ownerType == owner.ownerType)
        {
            Debug.Log("No podés atacar una unidad aliada.");
            return false;
        }

        // Calcular la distancia desde quien lanza la carta hasta el objetivo.
        float distance = Vector3.Distance(
            sourceUnit.transform.position,
            target.transform.position
        );

        if (distance > card.distance)
        {
            Debug.Log(
                $"El objetivo está fuera de alcance. " +
                $"Distancia: {distance:F1}, alcance: {card.distance}."
            );
            return false;
        }

        // Calcular bonus de ataque de las zonas activas.
        int damageTotal = CalculateDamage(card, sourceUnit);

        // Aplicar daño.
        target.TakeDamage(damageTotal);

        Vector3 newVector3 = new Vector3(target.transform.position.x, target.transform.position.y, target.transform.position.z - 0.2f);
        PlayCardEffect(card, newVector3);


        Debug.Log(
            $"Daño base: {card.value} | " +
            $"Daño total: {damageTotal}"
        );

        return true;
    }


    private bool HealUnit(CardSO card, Unit target, PlayerResources owner, Unit sourceUnit)
    {
        if (card == null || target == null || owner == null)
            return false;

        if (sourceUnit == null)
        {
            Debug.Log("Debés seleccionar una unidad para usar esta carta.");
            return false;
        }

        // Una unidad petrificada no puede utilizar cartas.
        if (sourceUnit.isPetrified)
        {
            Debug.Log("Una unidad petrificada no puede usar cartas.");
            return false;
        }

        // La curación solo puede afectar unidades aliadas.
        if (target.owner == null ||
            target.owner.ownerType != owner.ownerType)
        {
            Debug.Log("Solo podés curar unidades aliadas.");
            return false;
        }

        // Validar el alcance desde la unidad que lanza la carta.
        float distance = Vector3.Distance(
            sourceUnit.transform.position,
            target.transform.position
        );

        if (distance > card.distance)
        {
            Debug.Log(
                $"La unidad está fuera del alcance de curación. " +
                $"Distancia: {distance:F1}, alcance: {card.distance}."
            );
            return false;
        }

        // Calcular la curación.
        int healingAmount = card.value;

        // Pasiva de Kalyar: Corriente Vital.
        if (sourceUnit.hasKalyarPassive &&
            EffectZone.IsUnitInsideWaterZone(sourceUnit))
        {
            healingAmount += 1;

            Debug.Log("¡Corriente Vital! Kalyar potencia la curación +1.");
        }

        // Aplicar curación.
        target.Heal(healingAmount);

        Vector3 newVector3 = new Vector3(
            target.transform.position.x,
            target.transform.position.y - 0.2f,
            target.transform.position.z
        );

        PlayCardEffect(card, newVector3);

        Debug.Log(
            $"{sourceUnit.name} usó {card.cardName} para curar a " +
            $"{target.name} por {healingAmount} puntos."
        );

        return true;
    }


    private bool PlaceTrap(CardSO card, TileClick tile, PlayerResources owner, Unit sourceUnit)
    {
        if (card == null || tile == null || owner == null)
            return false;

        if (tile.hasTrap)
        {
            Debug.Log("Esta casilla ya tiene una trampa.");
            return false;
        }

        if (sourceUnit == null)
        {
            Debug.Log("Debés seleccionar una unidad para colocar la trampa.");
            return false;
        }

        if (sourceUnit.isPetrified)
        {
            Debug.Log("Una unidad petrificada no puede usar cartas.");
            return false;
        }

        /*if (sourceUnit.element != card.HasElement)
        {
            Debug.Log($"{sourceUnit.name} no puede usar {card.cardName}.");
            return false;
        }*/

        // Comprobar el alcance desde la unidad seleccionada.
        float distance = Vector3.Distance(
            sourceUnit.transform.position,
            tile.transform.position
        );

        if (distance > card.distance)
        {
            Debug.Log("La casilla está fuera del alcance de la carta.");
            return false;
        }

        // Colocar la trampa.
        tile.hasTrap = true;
        tile.trapDamage = card.value;

        Renderer cellRenderer = tile.GetComponent<Renderer>();

        if (cellRenderer != null)
            cellRenderer.material.color = new Color(0.2f, 0.8f, 1f);

        Debug.Log(
            $"Trampa colocada en {tile.name}. Daño: {tile.trapDamage}"
        );

        return true;
    }

    private bool PlaceZone( CardSO card, TileClick tile, PlayerResources owner, Unit sourceUnit)
    {
        if (card == null || tile == null || owner == null)
            return false;

        if (sourceUnit == null)
        {
            Debug.Log("Seleccioná una unidad para lanzar la carta.");
            return false;
        }

        if (sourceUnit.isPetrified)
        {
            Debug.Log("Una unidad petrificada no puede usar cartas.");
            return false;
        }

        if (sourceUnit.HasElement != card.elements && sourceUnit.HasElement != Element.All)
        {
            Debug.Log("Esta unidad no puede utilizar esa carta.");
            return false;
        }

        float distance = Vector3.Distance(
            sourceUnit.transform.position,
            tile.transform.position
        );

        if (distance > card.distance)
        {
            Debug.Log("La casilla está fuera del alcance.");
            return false;
        }

        if (card.prefab == null)
        {
            Debug.LogError(
                $"La carta {card.cardName} no tiene un prefab asignado."
            );
            return false;
        }

        Vector3 spawnPosition = tile.transform.position;

        GameObject zoneObject = Instantiate(
            card.prefab,
            spawnPosition,
            Quaternion.identity
        );

        PlayCardEffect(card, tile.transform.position);

        EffectZone zone = zoneObject.GetComponent<EffectZone>();

        if (zone == null)
        {
            Debug.LogError(
                "El prefab de la zona no tiene el componente EffectZone."
            );

            Destroy(zoneObject);
            return false;
        }

        zone.Initialize(card, tile);

        Debug.Log($"Se colocó la zona {card.cardName} en {tile.name}.");

        return true;
    }

    private int GetZoneAttackBonus(Unit attacker)
    {
        if (attacker == null)
            return 0;

        int totalBonus = 0;

        EffectZone[] zones =
            FindObjectsByType<EffectZone>(FindObjectsSortMode.None);

        foreach (EffectZone zone in zones)
        {
            if (zone == null)
                continue;

            totalBonus += zone.GetAttackBonus(attacker);
        }

        return totalBonus;
    }

    private bool DamageArea( CardSO card,  TileClick tile,  PlayerResources owner,   PlayerResources targetOwner,    Unit sourceUnit)
    {
        if (card == null || tile == null ||
            owner == null || targetOwner == null)
        {

            return false;

        }

        if (sourceUnit == null)
            return false;

        if (sourceUnit.isPetrified)
        {
            Debug.Log("Una unidad petrificada no puede usar cartas.");
            return false;
        }

        if (sourceUnit.HasElement != card.elements &&
            sourceUnit.HasElement != Element.All)
        {
            Debug.Log("Esta unidad no puede utilizar esa carta.");
            return false;
        }

        // Validar alcance desde el lanzador hasta el centro del área.
        float distance = Vector3.Distance(
            sourceUnit.transform.position,
            tile.transform.position
        );

        if (distance > card.distance)
        {
            Debug.Log("El área seleccionada está fuera del alcance.");
            return false;
        }

        Vector2Int center = tile.cellCoords;

        int affectedUnits = 0;
        int damageTotal = CalculateDamage(card, sourceUnit);

        foreach (GameObject obj in targetOwner.units)
        {
            if (obj == null)
                continue;

            Unit enemy = obj.GetComponent<Unit>();

            if (enemy == null)
                continue;

            Vector2Int position = enemy.currentCell;

            int dx = Mathf.Abs(position.x - center.x);
            int dy = Mathf.Abs(position.y - center.y);

            bool insideArea = false;

            switch (card.areaShape)
            {
                case AreaShape.Single:
                    insideArea = dx == 0 && dy == 0;
                    break;

                case AreaShape.Cross:
                    insideArea = (dx == 0 || dy == 0) &&
                                 dx + dy <= card.areaSize;
                    break;

                case AreaShape.Square:
                    insideArea = dx <= card.areaSize &&
                                 dy <= card.areaSize;
                    break;

                case AreaShape.Circle:
                    insideArea = (dx * dx + dy * dy) <=
                                 card.areaSize * card.areaSize;
                    break;
            }

            if (!insideArea)
                continue;

            enemy.TakeDamage(damageTotal);
            affectedUnits++;

            Debug.Log(
                $"{enemy.name} recibió {damageTotal} de daño."
            );
        }

        if (affectedUnits == 0)
        {
            Debug.Log("No hay enemigos dentro del área.");
            return false;
        }

        PlayCardEffect(card, tile.transform.position);

        Debug.Log(
            $"{card.cardName}: afectó a {affectedUnits} enemigos."
        );

        return true;
    }

    private bool AddShield( CardSO card,  Unit target,  PlayerResources owner,  Unit sourceUnit)
    {
        if (card == null || target == null ||
            owner == null || sourceUnit == null)
            return false;

        if (sourceUnit.isPetrified)
            return false;

        // Solo permitir escudar unidades aliadas.
        if (target.owner == null ||
            target.owner.ownerType != owner.ownerType)
            return false;

        // Validar elemento de quien lanza la carta.
        if (sourceUnit.HasElement != card.elements &&
            sourceUnit.HasElement != Element.All)
            return false;

        // Validar alcance.
        float distance = Vector3.Distance(
            sourceUnit.transform.position,
            target.transform.position
        );

        if (distance > card.distance)
            return false;

        target.AddShield(card.value);

        PlayCardEffect(card, target.transform.position);

        return true;
    }

    private void PlayCardEffect(CardSO card, Vector3 position)
    {
        if (card == null || card.particlePrefab == null)
            return;

        GameObject effect = Instantiate(
            card.particlePrefab,
            position,
            Quaternion.identity
        );

        Destroy(effect, 5f);
    }


    private int CalculateDamage(CardSO card, Unit sourceUnit)
    {
        int bonus = EffectZone.GetTotalAttackBonus(sourceUnit);

        // Pasiva del Heraldo: Afinidad Ígnea.
        if (sourceUnit != null &&
            sourceUnit.hasHeraldoPassive &&
            card.elements == Element.Fire)
        {
            bonus += 1;

            Debug.Log("¡Afinidad Ígnea! El Heraldo aumenta su daño en +1.");
        }

        return card.value + bonus;
    }


    private bool ApplyKnockback(
        CardSO card,
        TileClick tile,
        PlayerResources owner,
        PlayerResources targetOwner,
        Unit sourceUnit)
    {
        if (card == null || tile == null ||
            owner == null || targetOwner == null ||
            sourceUnit == null)
            return false;

        if (sourceUnit.isPetrified)
            return false;

        if (sourceUnit.HasElement != card.elements &&
            sourceUnit.HasElement != Element.All)
            return false;

        float distance = Vector3.Distance(
            sourceUnit.transform.position,
            tile.transform.position
        );

        if (distance > card.distance)
            return false;

        Vector2Int center = tile.cellCoords;

        List<Unit> allUnits = new List<Unit>();

        foreach (GameObject obj in owner.units.Concat(targetOwner.units))
        {
            if (obj == null) continue;

            Unit unit = obj.GetComponent<Unit>();

            if (unit != null && !allUnits.Contains(unit))
                allUnits.Add(unit);
        }

        // Seleccionamos los personajes dentro del área.
        List<Unit> affectedUnits = new List<Unit>();

        foreach (Unit unit in allUnits)
        {
            int dx = Mathf.Abs(unit.currentCell.x - center.x);
            int dy = Mathf.Abs(unit.currentCell.y - center.y);

            int distanceFromCenter = dx + dy;

            if (distanceFromCenter <= card.areaSize &&
                distanceFromCenter > 0)
            {
                affectedUnits.Add(unit);
            }
        }

        if (affectedUnits.Count == 0)
            return false;

        // Reproducir el efecto en el centro de la explosión.
        PlayCardEffect(card, tile.transform.position);

        // Ejecutar el empuje de manera animada.
        StartCoroutine(
            KnockbackRoutine(card, affectedUnits, allUnits, center)
        );

        return true;
    }

    private IEnumerator KnockbackRoutine(
    CardSO card,
    List<Unit> affectedUnits,
    List<Unit> allUnits,
    Vector2Int center)
    {
        // Primero empujamos a los personajes más cercanos al centro.
        affectedUnits = affectedUnits
            .OrderBy(unit =>
                Mathf.Abs(unit.currentCell.x - center.x) +
                Mathf.Abs(unit.currentCell.y - center.y))
            .ToList();

        foreach (Unit unit in affectedUnits)
        {
            if (unit == null)
                continue;

            int dx = unit.currentCell.x - center.x;
            int dy = unit.currentCell.y - center.y;

            int distanceFromCenter = Mathf.Abs(dx) + Mathf.Abs(dy);

            int push = Mathf.Max(
                1,
                card.value - distanceFromCenter + 1
            );

            Vector2Int direction;

            if (Mathf.Abs(dx) >= Mathf.Abs(dy))
            {
                direction = new Vector2Int(
                    dx == 0 ? 0 : (int)Mathf.Sign(dx),
                    0
                );
            }
            else
            {
                direction = new Vector2Int(
                    0,
                    dy == 0 ? 0 : (int)Mathf.Sign(dy)
                );
            }

            // Cada unidad tiene su propio límite de empuje.
            for (int i = 0; i < push; i++)
            {
                if (unit == null)
                    break;

                PushResult result = new PushResult();

                yield return StartCoroutine(
                    TryPushUnit(
                        unit,
                        direction,
                        allUnits,
                        new HashSet<Unit>(),
                        result,
                        card
                    )
                );

                if (!result.moved)
                    break;
            }
        }

        Debug.Log("La onda expansiva terminó.");
    }

    private IEnumerator TryPushUnit(
    Unit unit,
    Vector2Int direction,
    List<Unit> allUnits,
    HashSet<Unit> processing,
    PushResult result,
    CardSO card)
    {
        if (unit == null || processing.Contains(unit))
        {
            result.moved = false;
            yield break;
        }

        processing.Add(unit);

        Vector2Int nextCell = unit.currentCell + direction;

        // Comprobar que exista la casilla.
        TileClick destination = FindTile(nextCell);

        if (destination == null)
        {
            // No existe casilla: intentar desplazamiento lateral.
            yield return StartCoroutine(
                TryLateralMove(
                    unit, direction, allUnits, result, card
                )
            );

            processing.Remove(unit);
            yield break;
        }

        // Buscar si hay otro personaje en esa casilla.
        Unit blockingUnit = allUnits.FirstOrDefault(other =>
            other != null &&
            other != unit &&
            other.currentCell == nextCell
        );

        if (blockingUnit != null)
        {
            // Intentar empujar al personaje que bloquea.
            PushResult chainResult = new PushResult();

            yield return StartCoroutine(
                TryPushUnit(
                    blockingUnit,
                    direction,
                    allUnits,
                    processing,
                    chainResult,
                    card
                )
            );

            if (chainResult.moved)
            {
                yield return StartCoroutine(
                    unit.ForcedMoveTo(
                        nextCell,
                        0.2f
                    )
                );

                result.moved = true;
            }
            else
            {
                // Si no pudo empujarlo, buscar una salida lateral.
                yield return StartCoroutine(
                    TryLateralMove(
                        unit, direction, allUnits, result, card
                    )
                );
            }
        }
        else
        {
            // Casilla libre: mover normalmente.
            yield return StartCoroutine(
                unit.ForcedMoveTo(
                    nextCell,
                    0.2f
                )
            );

            result.moved = true;
        }

        if (!result.moved && unit != null)
        {
            unit.TakeDamage(1);
        }

        processing.Remove(unit);
    }

    private IEnumerator TryLateralMove(
    Unit unit,
    Vector2Int direction,
    List<Unit> allUnits,
    PushResult result,
    CardSO card)
    {
        // Direcciones perpendiculares al empuje.
        Vector2Int sideA = new Vector2Int(
            -direction.y,
            direction.x
        );

        Vector2Int sideB = new Vector2Int(
            direction.y,
            -direction.x
        );

        Vector2Int[] lateralDirections = { sideA, sideB };

        foreach (Vector2Int side in lateralDirections)
        {
            Vector2Int lateralCell = unit.currentCell + side;

            if (FindTile(lateralCell) == null)
                continue;

            bool occupiedByUnit = allUnits.Any(other =>
                other != null &&
                other != unit &&
                other.currentCell == lateralCell
            );

            if (occupiedByUnit)
                continue;

            yield return StartCoroutine(
                unit.ForcedMoveTo(
                    lateralCell,
                    0.2f
                )
            );

            result.moved = true;
            yield break;
        }

        result.moved = false;
    }

    private TileClick FindTile(Vector2Int cell)
    {
        TileClick[] tiles = FindObjectsByType<TileClick>(
            FindObjectsSortMode.None
        );

        foreach (TileClick tile in tiles)
        {
            if (tile != null && tile.cellCoords == cell)
                return tile;
        }

        return null;
    }
}



