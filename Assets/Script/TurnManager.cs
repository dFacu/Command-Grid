using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public enum TurnState { PlayerTurn, EnemyTurn }
    public TurnState currentTurn = TurnState.PlayerTurn;

    public float turnDuration = 30f;
    private float turnTimer;

    private void Start()
    {
        StartPlayerTurn();
    }
    void Update()
    {
        turnTimer -= Time.deltaTime;

        if(turnTimer <= 0)
        {
            if(currentTurn == TurnState.PlayerTurn)
                EndPlayerTurn();
            else if (currentTurn == TurnState.EnemyTurn)
                EndEnemyTurn();
        }
        
        if(currentTurn == TurnState.PlayerTurn && Input.GetKeyDown(KeyCode.Escape))
        {
            EndPlayerTurn();
        }
    }

    void StartPlayerTurn()
    {
        currentTurn = TurnState.PlayerTurn;
        turnTimer = turnDuration;
        Debug.Log("Comienza turno del jugador");
    }

    void EndPlayerTurn()
    {
        Debug.Log("Termina turno del jugador");
        StartEnemyTurn();
    }

    void StartEnemyTurn()
    {
        currentTurn = TurnState.EnemyTurn;
        turnTimer = turnDuration;
        Debug.Log("Comienza turno del enemigo");

        Invoke("EndEnemyTurn", 2f);
    }

    void EndEnemyTurn()
    {
        Debug.Log("Termina turno del enemigo");
        StartPlayerTurn();
    }
}
