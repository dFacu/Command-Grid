using UnityEngine;

[CreateAssetMenu(fileName = "NewUnitCard", menuName = "Card/Unit")]
public class UnitCardSO : Card
{
    public GameObject unitPrefab;   // prefab de la unidad a instanciar
    public int health = 1;
    public int attack = 1;

    public override void Play(CardContext ctx)
    {
        if (!ctx.targetCell.HasValue)
        {
            Debug.Log("Selecciona una celda para desplegar la unidad.");
            return;
        }

        if (ctx.owner.SpendPoints(cost))
        {
            Debug.Log($"{cardName} desplegado en {ctx.targetCell.Value} por {ctx.owner.ownerName}");
           // ctx.boardManager.SpawnUnit(unitPrefab, ctx.targetCell.Value, ctx.owner);
        }
    }
}
