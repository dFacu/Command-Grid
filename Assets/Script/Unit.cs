// Unit.cs (ejemplo simple)
using UnityEngine;

public class Unit : MonoBehaviour
{
    public PlayerResources owner;
    public int health = 5;
    public int attack = 2;
    public int movementRange = 1;   // cuántas celdas puede moverse por turno

    private Vector2Int currentCell;

    public void Initialize(PlayerResources ownerRef, Vector2Int startCell)
    {
        owner = ownerRef;
        currentCell = startCell;
    }

    public void MoveTo(Vector2Int targetCell)
    {
        // Validar rango de movimiento
        int distance = Mathf.Abs(targetCell.x - currentCell.x) + Mathf.Abs(targetCell.y - currentCell.y);
        if (distance <= movementRange && BoardManager.Instance.IsCellFree(targetCell))
        {
            // Liberar celda actual
            BoardManager.Instance.RemoveUnitAt(currentCell);

            // Actualizar posición
            transform.position = BoardManager.Instance.CellToWorld(targetCell);
            currentCell = targetCell;

            // Registrar nueva celda ocupada
            BoardManager.Instance.RegisterUnitAt(targetCell, gameObject);
        }
        else
        {
            Debug.Log("Movimiento inválido");
        }
    }

    public void TakeDamage(int dmg)
    {
        health -= dmg;
        if (health <= 0)
        {
            Debug.Log($"{gameObject.name} ha sido derrotado");
            BoardManager.Instance.RemoveUnitAt(currentCell);
            Destroy(gameObject);
        }
    }
}

