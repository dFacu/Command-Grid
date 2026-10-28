using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardExecutor : MonoBehaviour
{
    public static CardExecutor Instance;

    private void Awake()
    {
        Instance = this;
    }

    public bool PlayExecuter(CardSO card, TileClick tile, Unit target, PlayerResources owner)
    {
        if (card == null)
            return false;

        switch (card.effectType)
        {
            case EffectType.SpawnUnit:
                return SpawnUnit(card, tile, owner);
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









    public static void Play(CardSO card, PlayerResources owner, Vector2Int targetCell)
    {
        if (!owner.SpendPoints(card.cost)) return;

        // Obtener tiles afectados según el área

        switch (card.effectType)
        {
            case EffectType.SpawnUnit:
                /*foreach (var tile in affectedTiles)
                {
                    if (tile.IsFree())
                    {
                        // Usamos directamente el Vector3 que guarda el tile
                        BoardManager.Instance.SpawnUnit(card.prefab, tile.cellCoords, owner);
                        Debug.Log($"{card.cardName} invocó unidad en {tile.cellCoords}");
                        break; // solo una invocación
                    }
                }*/
                break;

            case EffectType.Trap:
                Vector2Int trapCell = BoardManager.Instance.FindStrategicTrapCell(owner);
                if (trapCell != Vector2Int.zero)
                {
                    TileClick tile = BoardManager.Instance.GetTile(trapCell);
                    tile.hasTrap = true;
                    tile.trapDamage = card.value;
                    Debug.Log($"{card.cardName} colocó trampa en {trapCell}");
                }
                break;

            case EffectType.Damage:
                Unit target = BoardManager.Instance.GetBestEnemyTarget(owner.ownerType);
                if (target != null)
                {
                    float distance = Vector3.Distance(owner.selectedUnit.transform.position, target.transform.position);
                    if (distance <= card.distance)
                    {
                        target.TakeDamage(card.value);
                        Debug.Log($"{card.cardName} infligió {card.value} de daño a {target.name}");
                    }
                }
                break;

            case EffectType.Heal:
                Unit ally = BoardManager.Instance.GetLowestHealthAlly(owner.ownerType);
                if (ally != null)
                {
                    ally.health += card.value;
                    Debug.Log($"{card.cardName} curó {card.value} a {ally.name}");
                }
                break;
            case EffectType.Buff:
               /* foreach (var tile in affectedTiles)
                {
                    if (tile.unitInCell != null)
                    {
                        Unit unit = tile.unitInCell.GetComponent<Unit>();
                        // if (unit != null && unit.HasElement == Element.Water)

                    }
                }*/
                break;
        }
    }
}

