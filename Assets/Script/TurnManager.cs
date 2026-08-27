using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public enum TurnState { PlayerTurn, EnemyTurn }
    public TurnState currentTurn = TurnState.PlayerTurn;

    void Update()
    {
        if (currentTurn == TurnState.PlayerTurn)
        {
            // Aquí va la lógica del jugador
            if (Input.GetKeyDown(KeyCode.Space)) // ejemplo: terminar turno con Space
            {
                EndPlayerTurn();
            }
        }
        else if (currentTurn == TurnState.EnemyTurn)
        {
            // Aquí va la lógica de la IA enemiga
            Debug.Log("Turno enemigo ejecutando...");
            EndEnemyTurn();
        }
    }

    void EndPlayerTurn()
    {
        currentTurn = TurnState.EnemyTurn;
    }

    void EndEnemyTurn()
    {
        currentTurn = TurnState.PlayerTurn;
    }
}
