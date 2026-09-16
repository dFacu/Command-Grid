// Unit.cs (ejemplo simple)
using UnityEngine;

public class Unit : MonoBehaviour
{
    public PlayerResources owner;
    public int health = 5;
    public int attack = 2;
    public int movementRange = 1;   // cuántas celdas puede moverse por turno


    public bool hasMovedThisTurn;


    public Vector3 currentCell;

    public void Initialize(PlayerResources ownerRef) {  owner = ownerRef; }

    public void MoveTo(Vector3 cell)
    {
        Vector3 pos = new Vector3(cell.x, 0.5f, cell.z);

        transform.position = pos;
        currentCell = cell;
    }

    public void Attack(Unit target)
    {
        if (target == null) return;

        target.health -= attack;
        Debug.Log($"{name} atacó a {target.name}, vida restante: {target.health}");

        if (target.health <= 0)
        {
           // BoardManager.Instance.RemoveUnitAt(target.GetCurrentCell());
            Destroy(target.gameObject);
            Debug.Log($"{target.name} fue derrotado!");
        }

        hasMovedThisTurn = true;
    }

    public void TakeDamage(int dmg)
    {
        health -= dmg;
        if (health <= 0)
        {
            Debug.Log($"{gameObject.name} ha sido derrotado");
           // BoardManager.Instance.RemoveUnitAt(currentCell);
            Destroy(gameObject);
        }
    }

    public bool IsInAttackRange(Unit target)
    {
        Vector3Int unitCell = Vector3Int.RoundToInt(this.currentCell - new Vector3(0, 0.5f, 0));
        Vector3Int targetCell = Vector3Int.RoundToInt(target.currentCell - new Vector3(0, 0.5f, 0));

        float dist = Vector2Int.Distance(
            new Vector2Int(unitCell.x, unitCell.z),
            new Vector2Int(targetCell.x, targetCell.z)
        );
        return dist <= 1;
    }

}

