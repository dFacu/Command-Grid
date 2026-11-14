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
        if (unit == null)
            return;

        // Evitar seleccionar unidades del enemigo.
        if (unit.owner != TurnManager.Instance.player)
        {
            Debug.Log("No podés seleccionar unidades enemigas.");
            return;
        }

        // Si había una unidad seleccionada, desmarcarla.
        if (selectedUnit != null)
            selectedUnit.SetSelected(false);

        // Actualizar la nueva unidad seleccionada.
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
        }
        else
        {
            Debug.Log("Movimiento inválido o celda ocupada por aliado");
        }
    }

    public void ResetTurn()
    {
        units.RemoveAll(unit => unit == null);
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
            currentPoints = isFirstPlayer ? 1 : 2;
        }
        else
        {
            currentPoints += 2;
            if (currentPoints > maxPoints)
                currentPoints = maxPoints;
        }

        // Pasivas que se activan al comenzar el turno.
        foreach (GameObject unitObject in units)
        {
            if (unitObject == null) continue;

            Unit unit = unitObject.GetComponent<Unit>();

            if (unit == null || unit.isPetrified) continue;

            if (unit.hasKalyarPassive &&
                EffectZone.IsUnitInsideWaterZone(unit))
            {
                unit.Heal(1);

                Debug.Log($"{unit.name}: Corriente Vital activada.");
            }

            // Reiniciar la pasiva del Heraldo al comenzar su turno.
            if (unit.hasHeraldoPassive)
            {
                unit.heraldoPassiveUsed = false;
            }
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
        // Activar las pasivas que dependen de cómo termina el turno.
        ActivateSagePassive();

        turnNumber++;

        ResetTurn();
    }

    private void ActivateSagePassive()
    {
        foreach (GameObject unitObject in units)
        {
            if (unitObject == null)
                continue;

            Unit sage = unitObject.GetComponent<Unit>();

            if (sage == null)
                continue;

            if (!sage.hasSagePassive)
                continue;

            // Si se movió o está petrificado, no activa la pasiva.
            if (sage.hasMovedThisTurn || sage.isPetrified)
                continue;

            // El Sabio recibe 1 punto de escudo.
            sage.AddShield(1);

            Debug.Log(
                $"¡Fortaleza Ancestral! {sage.name} obtiene 1 punto de escudo."
            );

            // Buscar aliados adyacentes.
            foreach (GameObject allyObject in units)
            {
                if (allyObject == null)
                    continue;

                Unit ally = allyObject.GetComponent<Unit>();

                if (ally == null || ally == sage)
                    continue;

                int distance =
                    Mathf.Abs(sage.currentCell.x - ally.currentCell.x) +
                    Mathf.Abs(sage.currentCell.y - ally.currentCell.y);

                // Adyacencia ortogonal.
                if (distance == 1)
                {
                    ally.AddShield(1);

                    Debug.Log(
                        $"¡Fortaleza Ancestral! {ally.name} " +
                        "recibe 1 punto de escudo."
                    );
                }
            }
        }
    }
}


