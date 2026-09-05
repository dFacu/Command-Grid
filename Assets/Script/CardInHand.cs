// CardInHand.cs
using UnityEngine;

public class CardInHand
{
    public Card cardSO;         // referencia al ScriptableObject
    public bool isExhausted;
    public int remainingCharges;

    public CardInHand(Card so)
    {
        cardSO = so;
        isExhausted = false;
        remainingCharges = 1;
    }

    public void Play(CardContext ctx)
    {
        if (isExhausted) return;
        cardSO.Play(ctx);
        // actualizar estado runtime si corresponde
        isExhausted = true;
        remainingCharges = Mathf.Max(0, remainingCharges - 1);
    }
}
