using UnityEngine;

public static class CardExecutor
{
    public static void Play(CardSO card, PlayerResources owner)
    {
        if (!owner.SpendPoints(card.cost)) return;

        switch (card.effectType)
        {

        }
    }
}

