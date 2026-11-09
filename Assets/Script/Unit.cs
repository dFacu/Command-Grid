// Unit.cs (ejemplo simple)
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Timeline;

public class Unit : MonoBehaviour
{
    public PlayerResources owner;

    public Element HasElement;
    public int maxHealth = 15;
    public int health;
    public int movementRange = 1;   // cuántas celdas puede moverse por turno

    public GameObject marker;

    public bool hasMovedThisTurn;


    public Vector2Int currentCell;

    public bool isPetrified = false;
    public int petrifiedTurns = 0;

    public int shield = 0;

    private void Start()
    {
        health = maxHealth;
    }

    public void Initialize(PlayerResources ownerRef) {  owner = ownerRef; }

    public void MoveTo(Vector2Int cell)
    {
        int distance = Mathf.Abs(cell.x - currentCell.x)
                     + Mathf.Abs(cell.y - currentCell.y);

        int availableMovement = movementRange;

        if (EffectZone.IsInsideMovementPenalty(this, cell))
        {
            availableMovement = Mathf.FloorToInt(movementRange * 0.5f);
        }

        if (distance > availableMovement)
        {
            Debug.Log("No podés moverte tan lejos.");
            return;
        }

        Vector3 pos = new Vector3(cell.x, 0.5f, cell.y);

        transform.position = pos;
        currentCell = cell;
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

    public void AddShield(int amount)
    {
        if (amount <= 0)
            return;

        shield += amount;

        Debug.Log($"{name} recibió {amount} de escudo. Escudo actual: {shield}");
    }

    public IEnumerator ForcedMoveTo(
        Vector2Int cell,
        float duration = 0.2f)
    {
        Vector3 startPosition = transform.position;

        Vector3 targetPosition = new Vector3(
            cell.x,
            0.5f,
            cell.y
        );

        currentCell = cell;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            // Pequeño impulso vertical durante el empuje.
            float height = Mathf.Sin(t * Mathf.PI) * 0.35f;

            transform.position =
                Vector3.Lerp(startPosition, targetPosition, t)
                + Vector3.up * height;

            yield return null;
        }

        transform.position = targetPosition;
    }
}

