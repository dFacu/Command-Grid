// AbilityCardSO.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewAbilityCard", menuName = "Card/Ability")]
public class AbilityCardSO : Card
{
    public int power = 1;

    public override void Play(CardContext ctx)
    {
        if (ctx.owner.SpendPoints(cost))
        {
            Debug.Log($"{cardName} activada por {ctx.owner.ownerName}, poder {power}");
            // Lógica de habilidad: daño, curación, buff, etc. Usar ctx.targetCell o ctx.targetUnit
        }
    }
}
