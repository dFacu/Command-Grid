using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GUILayout;

public enum ZoneEffectType
{
    None,
    AttackBonus,
    Heal,
    ReduceMovement,
    AttackReduction
}
public enum CardType 
{ 
    Unit,
    Ability,
    Tactic
}
public enum EffectType 
{
    SpawnUnit,
    Petrify,
    Heal,
    Damage,
    Trap,
    Buff,
    Debuff,
    Terrain
}
public enum Element
{
    None,
    Fire,
    Water,
    Air
}

public enum AreaShape
{
    Single,
    Cross,
    Square,
    Circle
}

[CreateAssetMenu(fileName = "NewCard", menuName = "Card/Base")]
public class CardSO : ScriptableObject
{
    public string cardName;

    public CardType type;

    public Sprite image;

    public int cost;

    [TextArea(4,4)] public string description;

    public EffectType effectType;
    public Element elements;
    public Element targetElement;

    public int value;       // daño, curación, buff
    public int distance;    // alcance
    public int duration;    // turnos que dura el efecto

    public AreaShape areaShape;
    public int areaSize;

    [Header("Efectos de zona")]
    public ZoneEffectType effectForFavoredElement;
    public Element favoredElement;

    public ZoneEffectType effectForOtherElements;
    public Element excludedElement;



    public GameObject prefab; // para unidades





    public string rarity;     // común, rara, épica


}