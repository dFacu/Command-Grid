using UnityEngine;

public class PlayerResources : MonoBehaviour
{
    public string ownerName;
    public int currentPoints;
    public int maxPoints = 8;
    public int turnNumber = 1;

    public PlayerResources(string name)
    {
        ownerName = name;
        currentPoints = 0;
    }

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
        if(currentPoints >= cost)
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
    }
}
