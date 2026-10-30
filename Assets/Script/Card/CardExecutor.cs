using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class CardExecutor : MonoBehaviour
{
    public static CardExecutor Instance;

    private void Awake()
    {
        Instance = this;
    }

    public bool PlayExecuter(CardSO card, TileClick tile, Unit target, PlayerResources owner, PlayerResources targetOwner)
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
                return DamageEnemy(card, target, owner);
            case EffectType.Heal:
                return HealUnit(card, target, owner);
            case EffectType.Trap:
                return PlaceTrap(card, tile, owner);
            case EffectType.Terrain:
                return PlaceZone(card, tile, owner);



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

        if (affectedAny)
        {
            Debug.Log(
                $"{card.name}: los enemigos fueron petrificados " +
                $"por {card.value} turnos."
            );
        }

        return affectedAny;
    }

    private bool DamageEnemy(CardSO card, Unit target, PlayerResources owner)
    {
        if (card == null || target == null || owner == null)
            return false;

        // La unidad seleccionada es quien lanza la carta.
        Unit sourceUnit = owner.selectedUnit;

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
        if (sourceUnit.HasElement != card.elements)
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
        int bonusAtaque = EffectZone.GetTotalAttackBonus(sourceUnit);

        int damageTotal = card.value + bonusAtaque;

        // Aplicar daño.
        target.TakeDamage(damageTotal);

        Debug.Log(
            $"Daño base: {card.value} | " +
            $"Bonus de zona: {bonusAtaque} | " +
            $"Daño total: {damageTotal}"
        );

        return true;
    }

    private bool HealUnit(CardSO card, Unit target, PlayerResources owner)
    {
        if (card == null || target == null || owner == null)
            return false;

        // Unidad que utiliza la carta.
        Unit sourceUnit = owner.selectedUnit;

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

        // Validar el elemento de quien lanza la carta.
       /* if (sourceUnit.element != card.HasElement)
        {
            Debug.Log(
                $"{sourceUnit.name} no puede usar {card.cardName}. " +
                $"Elemento requerido: {card.HasElement}."
            );
            return false;
        }*/

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

        // Aplicar curación.
        target.Heal(card.value);

        Debug.Log(
            $"{sourceUnit.name} usó {card.cardName} para curar a " +
            $"{target.name} por {card.value} puntos."
        );

        return true;
    }

    private bool PlaceTrap(CardSO card, TileClick tile, PlayerResources owner)
    {
        if (card == null || tile == null || owner == null)
            return false;

        if (tile.hasTrap)
        {
            Debug.Log("Esta casilla ya tiene una trampa.");
            return false;
        }

        // Validar la unidad que lanza la carta.
        Unit sourceUnit = owner.selectedUnit;

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

    private bool PlaceZone( CardSO card, TileClick tile, PlayerResources owner)
    {
        if (card == null || tile == null || owner == null)
            return false;

        Unit sourceUnit = owner.selectedUnit;

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

        if (sourceUnit.HasElement != card.elements)
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

}

