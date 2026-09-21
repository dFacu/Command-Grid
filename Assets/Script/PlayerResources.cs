using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class PlayerResources : MonoBehaviour
{
    public static PlayerResources Instance;

    public string ownerName;
    public int currentPoints;
    public int maxPoints = 8;
    public int turnNumber = 1;


    public Unit selectedUnit;

    public List<GameObject> units = new List<GameObject>();

    public bool isPlacingTrap = false;
    public CardSO cardPendingTrap; // carta que espera colocación

    public bool isPlacingSpawn = false;
    public CardSO cardPendingSpawn;

    public Deck deck;


    private void Awake()
    {
        Instance = this;
    }

    public PlayerResources(string name)
    {
        ownerName = name;
        currentPoints = 0;
    }



    public void SelectUnit(Unit unit) { selectedUnit = unit; }

    public void TryMoveOrAttack(Vector3 targetCell)
    {
        if (selectedUnit == null) return;

        //Buscar si hay una unidad en esa celda
        GameObject target = FindUnitAtCell(targetCell);
        Unit targetUnit = null;
       
        if (target != null)
        {
             targetUnit = target.GetComponent<Unit>();
        }

        if (targetUnit != null && targetUnit.owner != selectedUnit.owner)
        {
            //Atacar
            selectedUnit.Attack(targetUnit);
        }
        else if (!selectedUnit.hasMovedThisTurn && targetUnit == null)
        {
            selectedUnit.MoveTo(targetCell);
            selectedUnit.hasMovedThisTurn = true;
        }
        else
        {
            Debug.Log("Movimiento inválido o celda ocupada por aliado");
        }
    }

    public void ResetTurn()
    {
        foreach (GameObject unit in units)
        {
            var und = unit.GetComponent<Unit>();
            und.hasMovedThisTurn = false;
            selectedUnit = null;
        }
    }

    public void RegisterUnit(GameObject unit) { units.Add(unit); }

    public void StartTurn(bool isFirstPlayer)
    {
        if (turnNumber == 1)
        {
            // Primer turno: +1 si empieza, +2 si es segundo
            currentPoints = isFirstPlayer ? 1 : 2;
        }
        else
        {
            // A partir del segundo turno: +2 hasta máximo
            currentPoints += 2;
            if (currentPoints > maxPoints)
                currentPoints = maxPoints;
        }

        Debug.Log($"Turno {turnNumber} de {ownerName}: {currentPoints} PC disponibles");
    }

    public bool SpendPoints(int cost)
    {
        if (currentPoints >= cost)
        {
            currentPoints -= cost;
            Debug.Log($"{ownerName} gastó {cost} PC. Restan {currentPoints}");
            return true;
        }
        else
        {
            Debug.Log($"{ownerName} no tiene suficientes PC");
            return false;
        }
    }

    public void EndTurn()
    {
        turnNumber++;
        ResetTurn();
    }

    private GameObject FindUnitAtCell(Vector3 cell)
    {
        // Buscar en jugador
        foreach (GameObject u in PlayerResources.Instance.units)
        {
            Vector3 unitCell = u.GetComponent<Unit>().currentCell;

            // Normalizar altura: (y - 0.5) → redondear
            int unitY = Mathf.RoundToInt(unitCell.y - 0.5f);
            int cellY = Mathf.RoundToInt(cell.y - 0.5f);

            if (Mathf.RoundToInt(unitCell.x) == Mathf.RoundToInt(cell.x) &&
                unitY == cellY &&
                Mathf.RoundToInt(unitCell.z) == Mathf.RoundToInt(cell.z))
            {
                return u;
            }
        }

        // Buscar en IA
        foreach (GameObject u in IAEnemy.Instance.resources.units)
        {
            Vector3 unitCell = u.GetComponent<Unit>().currentCell;

            int unitY = Mathf.RoundToInt(unitCell.y - 0.5f);
            int cellY = Mathf.RoundToInt(cell.y - 0.5f);

            if (Mathf.RoundToInt(unitCell.x) == Mathf.RoundToInt(cell.x) &&
                unitY == cellY &&
                Mathf.RoundToInt(unitCell.z) == Mathf.RoundToInt(cell.z))
            {
                return u;
            }
        }
        return null;
    }


    public void SpawnUnit(GameObject unit, Vector3 cell, PlayerResources owner)
    {
        Vector3 pos = new Vector3(cell.x, 0.5f, cell.y);

        GameObject clon = Instantiate(unit, pos, Quaternion.identity);

        var unitComp = clon.GetComponent<Unit>();
        if (unitComp != null) unitComp.Initialize(owner);

        owner.RegisterUnit(unitComp.gameObject);
    }

    public void SpawnUnitAtTile(TileClick tile, CardSO card)
    {
        if (card.prefab == null) return;

        Vector3 spawnPos = new Vector3( tile.cellCoords.x, 0.5f, tile.cellCoords.y);
        GameObject unit = Instantiate(card.prefab, spawnPos, Quaternion.identity);
        isPlacingSpawn = false; // salir del modo spawn

        Debug.Log($"Unidad {card.cardName} invocada en {tile.name}");
    }


    public void PlaceTrap(TileClick tile, CardSO card)
    {
        tile.hasTrap = true;
        tile.trapDamage = card.value;
        isPlacingTrap = false; // salir del modo trampa
        Debug.Log($"Trampa {card.cardName} colocada en {tile.name} con daño {tile.trapDamage}");
    }
}


