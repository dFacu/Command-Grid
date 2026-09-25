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
                Debug.Log($"{card.cardName} activó modo invocación de unidad.");
                break;
            case EffectType.Trap:
                 Debug.Log($"{card.cardName} activó modo selección de tile.");
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

