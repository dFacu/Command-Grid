using UnityEngine;

public enum CardType { Unit, Ability, Tactic}


[System.Serializable]
public class Card
{
    public string cardName;
    public CardType type;
    public int cost;

    public Card(string cardName, CardType type, int cost)
    {
        this.cardName = cardName;
        this.type = type;
        this.cost = cost;
    }

    public void Play(CommandPoints pointsSystem)
    {
        if (pointsSystem.SpendPoints(cost))
        {
            Debug.Log($"{cardName} jugada (tipo: {type})");
        }
    }
}
