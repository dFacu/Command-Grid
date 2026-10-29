using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;


public class PlayerResources : MonoBehaviour
{
    public static PlayerResources Instance;

    public Owner ownerType; // Player o AI
    public string ownerName;
    public int currentPoints;
    public int maxPoints = 8;
    public int turnNumber = 1;

    

    public Unit selectedUnit;

    public List<GameObject> units = new List<GameObject>();    

    private void Awake()
    {
        Instance = this;
    }

    public PlayerResources(string name)
    {
        ownerName = name;
        currentPoints = 0;
    }

    public void SelectUnit(Unit unit)
    {
        // Si había una unidad seleccionada, desmarcarla
        if (selectedUnit != null)
            selectedUnit.SetSelected(false);

        // Actualizar la nueva unidad seleccionada
        selectedUnit = unit;
        selectedUnit.SetSelected(true);

        Debug.Log($"Unidad seleccionada: {selectedUnit.name}");
    }

    public void TryMoveOrAttack(Vector2Int targetCell)
    {
        if (selectedUnit == null) return;

        if (selectedUnit.isPetrified)
        {
            Debug.Log("Esta unidad está petrificada y no puede moverse.");
            return;
        }

        if (!selectedUnit.hasMovedThisTurn)
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
            und.marker.SetActive(false);
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

    
}


