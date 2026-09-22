using System.Collections.Generic;
using UnityEngine;

public enum CardType 
{ 
    Unit,
    Ability,
    Tactic
}
public enum EffectType 
{ 
    SpawnUnit,
    Heal,
    Damage,
    Trap
}

public enum Element
{
    None,
    Fire,
    Water,
    Air
}

[CreateAssetMenu(fileName = "NewCard", menuName = "Card/Base")]
public class CardSO : ScriptableObject
{
    public string cardName;

    public CardType type;

    public int cost;

    [TextArea] public string description;

    public EffectType effectType;

    public List<Element> elements;

    public int value; // daño, curación, etc.
    public GameObject prefab; // para unidades
}