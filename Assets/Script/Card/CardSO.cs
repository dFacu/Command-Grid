using UnityEngine;

public enum CardType { Unit, Ability, Tactic }
public enum EffectType { SpawnUnit, Heal, Damage, Trap }

[CreateAssetMenu(fileName = "NewCard", menuName = "Card/Base")]
public class CardSO : ScriptableObject
{
    public string cardName;
    public CardType type;
    public int cost;
    [TextArea] public string description;

    public EffectType effectType;
    public int value; // daño, curación, etc.
    public GameObject prefab; // para unidades
}