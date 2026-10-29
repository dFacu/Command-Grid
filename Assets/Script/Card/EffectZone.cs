
using UnityEngine;

public class EffectZone : MonoBehaviour
{
    [Header("Configuración")]
    public CardSO cardData;
    public int turnsRemaining;

    private TileClick centerTile;
    private bool initialized;

    public void Initialize(CardSO card, TileClick tile)
    {
        if (card == null || tile == null)
            return;

        cardData = card;
        centerTile = tile;
        turnsRemaining = card.duration;
        initialized = true;

        Debug.Log(
            $"Zona {card.cardName} creada en {tile.name}. " +
            $"Duración: {turnsRemaining} turnos."
        );
    }

    // Comprueba si una unidad está dentro de la zona.
    public bool ContainsUnit(Unit unit)
    {
        if (!initialized || unit == null || centerTile == null)
            return false;

        TileClick[] cells =
            FindObjectsByType<TileClick>(FindObjectsSortMode.None);

        int radius = Mathf.Max(0, (cardData.areaSize - 1) / 2);

        foreach (TileClick cell in cells)
        {
            if (cell == null || cell.unitInCell != unit.gameObject)
                continue;

            int dx = Mathf.Abs(
                cell.cellCoords.x - centerTile.cellCoords.x
            );

            int dy = Mathf.Abs(
                cell.cellCoords.y - centerTile.cellCoords.y
            );

            // Área cuadrada: por ejemplo, 3x3 casillas.
            return dx <= radius && dy <= radius;
        }

        return false;
    }

    // Bonus de ataque para las unidades del elemento favorecido.
    public int GetAttackBonus(Unit unit)
    {
        if (!ContainsUnit(unit) || cardData == null)
            return 0;

        if (cardData.favoredElement == unit.HasElement &&
            cardData.effectForFavoredElement ==
            ZoneEffectType.AttackBonus)
        {
            return cardData.value;
        }

        return 0;
    }

    // Multiplicador de movimiento para los demás elementos.
    public float GetMovementMultiplier(Unit unit)
    {
        if (unit == null || cardData == null)
            return 1f;

        // La unidad debe estar dentro de esta zona.
        if (!ContainsUnit(unit))
            return 1f;

        // Las unidades de Agua no reciben penalización.
        if (unit.HasElement == Element.Water)
            return 1f;

        // Los demás elementos tienen la mitad de movimiento.
        return 0.5f;
    }

    public void EndTurn()
    {
        if (!initialized)
            return;

        turnsRemaining--;

        if (turnsRemaining <= 0)
        {
            Debug.Log($"La zona {cardData.cardName} terminó.");
            Destroy(gameObject);
        }
    }

    public static float GetMovementMultiplierFromZones(Unit unit)
    {
        if (unit == null)
            return 1f;

        EffectZone[] zones =
            FindObjectsByType<EffectZone>(FindObjectsSortMode.None);

        float multiplier = 1f;

        foreach (EffectZone zone in zones)
        {
            if (zone == null)
                continue;

            // Si una zona reduce el movimiento, aplicamos la penalización.
            multiplier = Mathf.Min(
                multiplier,
                zone.GetMovementMultiplier(unit)
            );
        }

        return multiplier;
    }

    public static int GetMovementRange(Unit unit)
    {
        if (unit == null)
            return 0;

        float multiplier = GetMovementMultiplierFromZones(unit);

        return Mathf.Max(
            0,
            Mathf.FloorToInt(unit.movementRange * multiplier)
        );
    }
}
