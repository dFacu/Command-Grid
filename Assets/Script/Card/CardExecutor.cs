using UnityEngine;

public static class CardExecutor
{
    public static void Play(CardSO card, PlayerResources owner)
    {
        if (!owner.SpendPoints(card.cost)) return;

        switch (card.effectType)
        {
            case EffectType.SpawnUnit:
                if (card.prefab != null)
                {
                    owner.SpawnUnit(card.prefab,Vector3.one,owner);
                    Debug.Log($"{card.cardName} invocó una unidad.");
                }
                break;
            case EffectType.Trap:
                owner.isPlacingTrap = true; // Modo selec Tile
                owner.PlaceTrap(owner.selectedTile, card);
               
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

