using UnityEngine;

public class CommandPoints : MonoBehaviour
{
    public int maxPoint = 5;
    public int currentPoints;

    void StartTurn()
    {
        currentPoints = maxPoint;
        Debug.Log($"Nuevo turno: {currentPoints} PC disponibles");
    }

    public bool SpendPoints(int cost)
    {
        if(currentPoints >= cost)
        {
            currentPoints -= cost;
            Debug.Log($"Gastaste {cost} PC. Restan {currentPoints}");
            return true;
        }
        else
        {
            Debug.Log("No tienes suficientes PC");
            return false;
        }
    }
}
