using UnityEngine;

public enum CardType { Unit, Ability, Tactic}

[CreateAssetMenu(fileName = "NewCard", menuName = "Card")]
public class Card : ScriptableObject
{
    public string cardName;
    public CardType type;
    public int cost;
    [TextArea] public string description;

    // Método virtual que recibe contexto (owner, target cell, etc.)
    public virtual void Play(CardContext ctx)
    {
        if (ctx.owner.SpendPoints(cost))
        {
            Debug.Log($"{cardName} jugada por {ctx.owner.ownerName} (tipo: {type})");
            // Efecto por defecto: override en SO concretos o manejar por tipo
        }
    }
}

