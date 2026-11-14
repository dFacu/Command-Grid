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


    [Header("Pasivas de Kalyar")]
    public bool hasKalyarPassive;

    [Header("Pasivas de Heraldo")]
    public bool hasHeraldoPassive;
    // Control de activación por vuelta.
    public bool heraldoPassiveUsed;

    [Header("Pasivas del Orquestador")]
    public bool hasOrquestadorPassive;

    // Convergencia Arcana
    public bool convergenceUsed;

    // Resonancia Elemental
    public bool resonanceAvailable;
    public Element firstResonanceElement;

    [Header("Pasivas del Sabio")]
    public bool hasSagePassive;

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

        CheckHeraldoReaction();
    }

    public void TakeDamage(int dmg)
    {
        if (dmg <= 0)
            return;

        int remainingDamage = dmg;

        if (shield > 0)
        {
            int absorbed = Mathf.Min(shield, remainingDamage);

            shield -= absorbed;
            remainingDamage -= absorbed;
        }

        health -= remainingDamage;

        if (health <= 0)
        {
            health = 0;

            Debug.Log($"{gameObject.name} ha sido derrotado");

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

        CheckHeraldoReaction();
    }

    public void CheckHeraldoReaction()
    {
        if (owner == null)
            return;

        // Buscar todas las unidades activas.
        Unit[] allUnits = FindObjectsByType<Unit>(
            FindObjectsSortMode.None
        );

        foreach (Unit heraldo in allUnits)
        {
            if (heraldo == null || heraldo == this)
                continue;

            if (!heraldo.hasHeraldoPassive)
                continue;

            if (heraldo.owner == null || heraldo.owner == owner)
                continue;

            if (heraldo.heraldoPassiveUsed)
                continue;

            // Distancia entre las casillas.
            int distance =
                Mathf.Abs(currentCell.x - heraldo.currentCell.x) +
                Mathf.Abs(currentCell.y - heraldo.currentCell.y);

            // Adyacencia ortogonal.
            if (distance == 1)
            {
                heraldo.heraldoPassiveUsed = true;

                TakeDamage(1);

                Debug.Log(
                    $"¡Llama Reactiva! {heraldo.name} infligió " +
                    $"1 punto de daño a {name}."
                );
            }
        }
    }

    public int GetCardCost(CardSO card)
    {
        int finalCost = card.cost;

        if (!hasOrquestadorPassive)
            return finalCost;

        // Las cartas All no activan ni consumen Resonancia.
        if (card.elements == Element.All)
            return finalCost;

        // Si hay resonancia disponible y la carta es de
        // un elemento diferente al primero utilizado.
        if (resonanceAvailable &&
            card.elements != firstResonanceElement)
        {
            finalCost = Mathf.Max(0, finalCost - 1);
        }

        return finalCost;
    }

    public void RegisterElementalCard(CardSO card)
    {
        if (!hasOrquestadorPassive || card == null)
            return;

        // Las cartas All no participan de las pasivas.
        if (card.elements == Element.All)
            return;

        // Primera carta elemental de esta vuelta.
        if (!convergenceUsed)
        {
            convergenceUsed = true;
            firstResonanceElement = card.elements;
            resonanceAvailable = true;

            owner.currentPoints = Mathf.Min(
                owner.currentPoints + 1,
                owner.maxPoints
            );

            Debug.Log(
                $"{name}: ¡Convergencia Arcana! " +
                "El jugador obtiene 1 punto de maná."
            );

            return;
        }

        // Una carta de elemento diferente consume la resonancia.
        if (resonanceAvailable &&
            card.elements != firstResonanceElement)
        {
            resonanceAvailable = false;

            Debug.Log(
                $"{name}: ¡Resonancia Elemental consumida!"
            );
        }
    }
}

