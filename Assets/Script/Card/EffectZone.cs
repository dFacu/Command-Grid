
using System.Collections.Generic;
using UnityEngine;

public class EffectZone : MonoBehaviour
{
    [Header("Configuración")]
    public CardSO cardData;
    public int turnsRemaining;

    private TileClick centerTile;
    private bool initialized;


    [Header("Visualización de la zona")]
    public Color zoneColor = new Color(0.2f, 0.6f, 1f, 0.4f);

    private Renderer[] affectedRenderers;
    private MaterialPropertyBlock propertyBlock;

    private Dictionary<Renderer, Color> originalColors =
    new Dictionary<Renderer, Color>();

    public void Initialize(CardSO card, TileClick tile)
    {
        if (card == null || tile == null)
            return;

        cardData = card;
        centerTile = tile;
        turnsRemaining = card.duration;
        initialized = true;

        PaintZone();

        Debug.Log($"Zona {card.cardName} creada.");
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
        if (unit == null || cardData == null)
            return 0;

        // La unidad debe estar dentro de la zona.
        if (!ContainsUnit(unit))
            return 0;

        // Solo beneficia al elemento configurado.
        if (unit.HasElement != cardData.favoredElement)
            return 0;

        // Verificamos que la zona tenga bonus de ataque.
        if (cardData.effectForFavoredElement != ZoneEffectType.AttackBonus)
            return 0;

        return cardData.value;
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
            RestoreZoneColors();

            Debug.Log($"La zona {cardData.cardName} desapareció.");

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

    // Comprueba si una casilla está dentro del área de la zona.
    public bool ContainsCell(Vector2Int cell)
    {
        if (!initialized || centerTile == null || cardData == null)
            return false;

        int radius = Mathf.Max(0, (cardData.areaSize - 1) / 2);

        int dx = Mathf.Abs(cell.x - centerTile.cellCoords.x);
        int dy = Mathf.Abs(cell.y - centerTile.cellCoords.y);

        return dx <= radius && dy <= radius;
    }


    // Comprueba si una unidad recibe penalización de movimiento
    // al intentar llegar a una casilla.
    public static bool IsInsideMovementPenalty(Unit unit, Vector2Int destination)
    {
        if (unit == null)
            return false;

        EffectZone[] zones =
            FindObjectsByType<EffectZone>(FindObjectsSortMode.None);

        foreach (EffectZone zone in zones)
        {
            if (zone == null || zone.cardData == null)
                continue;

            // La unidad debe ser de un elemento distinto al favorecido.
            if (unit.HasElement == zone.cardData.favoredElement)
                continue;

            // La zona debe tener configurada la reducción de movimiento.
            if (zone.cardData.effectForOtherElements !=
                ZoneEffectType.ReduceMovement)
                continue;

            if (zone.ContainsCell(destination))
                return true;
        }

        return false;
    }

    public static int GetTotalAttackBonus(Unit unit)
    {
        if (unit == null)
            return 0;

        int totalBonus = 0;

        EffectZone[] zones =
            FindObjectsByType<EffectZone>(FindObjectsSortMode.None);

        foreach (EffectZone zone in zones)
        {
            if (zone == null)
                continue;

            totalBonus += zone.GetAttackBonus(unit);
        }

        return totalBonus;
    }

    private void PaintZone()
    {
        if (centerTile == null)
            return;

        TileClick[] cells =
            FindObjectsByType<TileClick>(FindObjectsSortMode.None);

        propertyBlock = new MaterialPropertyBlock();

        foreach (TileClick cell in cells)
        {
            if (cell == null || !ContainsCell(cell.cellCoords))
                continue;

            Renderer cellRenderer = cell.GetComponent<Renderer>();

            if (cellRenderer == null)
                continue;

            // Guardar el color original.
            if (!originalColors.ContainsKey(cellRenderer))
            {
                Color originalColor = cellRenderer.material.HasProperty("_BaseColor")
                    ? cellRenderer.material.GetColor("_BaseColor")
                    : cellRenderer.material.color;

                originalColors.Add(cellRenderer, originalColor);
            }

            cellRenderer.GetPropertyBlock(propertyBlock);

            propertyBlock.SetColor("_Color", zoneColor);
            propertyBlock.SetColor("_BaseColor", zoneColor);

            cellRenderer.SetPropertyBlock(propertyBlock);
        }
    }

    private void RestoreZoneColors()
    {
        foreach (var item in originalColors)
        {
            if (item.Key == null)
                continue;

            item.Key.SetPropertyBlock(null);
        }

        originalColors.Clear();
    }
}
