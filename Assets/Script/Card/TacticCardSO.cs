// TacticCardSO.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewTacticCard", menuName = "Card/Tactic")]
public class TacticCardSO : Card
{
    public override void Play(CardContext ctx)
    {
        if (ctx.owner.SpendPoints(cost))
        {
            Debug.Log($"{cardName} táctica usada por {ctx.owner.ownerName}");
            // Efecto táctico: modificar estado global, robar carta, etc.
        }
    }
}
