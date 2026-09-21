using Unity.VisualScripting;
using UnityEngine;

public static class CardExecutor
{

    public static void Play(CardSO card, PlayerResources owner)
    {
        if (!owner.SpendPoints(card.cost)) return;

        switch (card.effectType)
        {
            case EffectType.SpawnUnit:
                owner.isPlacingSpawn = true;              // modo selección de tile
                owner.cardPendingSpawn = card;            // guardamos la carta
                Debug.Log($"{card.cardName} activó modo invocación de unidad.");
                break;
            case EffectType.Trap:
                owner.isPlacingTrap = true;
                owner.cardPendingTrap = card; // guardamos la carta que disparó el modo
                Debug.Log($"{card.cardName} activó modo selección de tile.");

                /*owner.isPlacingTrap = true; // Modo selec Tile
                owner.PlaceTrap(owner.selectedTile, card);
               */
                break;
            case EffectType.Damage:
                /*if (owner.SelectedUnit != null)
                {
                    owner.SelectedUnit.TakeDamage(card.value);
                    Debug.Log($"{card.cardName} infligió {card.value} de daño.");
                }*/
                break;
        }

        
    }
}

