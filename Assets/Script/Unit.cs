// Unit.cs (ejemplo simple)
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

public class Unit : MonoBehaviour
{
    public PlayerResources owner;

    public Element HasElement;
    public int health = 5;
    public int attack = 2;
    public int movementRange = 1;   // cuántas celdas puede moverse por turno

    public GameObject marker;

    public bool hasMovedThisTurn;


    public Vector2Int currentCell;

    // Lista de efectos activos
   // private List<StatusEffect> activeEffects = new List<StatusEffect>();


    public void Initialize(PlayerResources ownerRef) {  owner = ownerRef; }

    public void MoveTo(Vector2Int cell)
    {
        Vector3 pos = new Vector3(cell.x, 0.5f, cell.y);

        transform.position = pos;
        currentCell = cell;
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
        // currentCell y target.currentCell deberían ser Vector2Int
        int dist = Mathf.Abs(this.currentCell.x - target.currentCell.x) +
                   Mathf.Abs(this.currentCell.y - target.currentCell.y);

        // Usamos distancia Manhattan (más común en grids)
        return dist <= 1;
    }   

    public void SetSelected(bool isSelected)
    {
        if (marker != null)
            marker.SetActive(isSelected);
    }

}

