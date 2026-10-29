// Unit.cs (ejemplo simple)
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

public class Unit : MonoBehaviour
{
    public PlayerResources owner;

    public Element HasElement;
    public int maxHealth = 15;
    public int health;
    public int attack = 2;
    public int movementRange = 1;   // cuántas celdas puede moverse por turno

    public GameObject marker;

    public bool hasMovedThisTurn;


    public Vector2Int currentCell;

    public bool isPetrified = false;
    public int petrifiedTurns = 0;

    private void Start()
    {
        health = maxHealth;
    }

    public void Initialize(PlayerResources ownerRef) {  owner = ownerRef; }

    public void MoveTo(Vector2Int cell)
    {
        int availableMovement = EffectZone.GetMovementRange(this);

        int distance = Mathf.Abs(cell.x - currentCell.x)
                     + Mathf.Abs(cell.y - currentCell.y);

        if (distance > availableMovement)
        {
            Debug.Log("No podés moverte tan lejos.");
            return;
        }

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

    public void Petrify(int turns)
    {
        isPetrified = true;
        petrifiedTurns = Mathf.Max(petrifiedTurns, turns);

        Debug.Log($"{name} fue convertido en piedra por {petrifiedTurns} turnos.");
    }

    public void ReducePetrification()
    {
        if (!isPetrified)
            return;

        petrifiedTurns--;

        if (petrifiedTurns <= 0)
        {
            petrifiedTurns = 0;
            isPetrified = false;

            Debug.Log($"{name} ya no está petrificado.");
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0)
            return;

        health = Mathf.Min(health + amount, maxHealth);

        Debug.Log($"{name} ahora tiene {health}/{maxHealth} de vida.");
    }
}

