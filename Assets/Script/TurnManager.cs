using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public enum TurnState { PlayerTurn, EnemyTurn }
    public TurnState currentTurn = TurnState.PlayerTurn;

    public float turnDuration = 30f;
    private float turnTimer;

    public PlayerResources player;
    public PlayerResources enemy;

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
        player.StartTurn(true);

        currentTurn = TurnState.PlayerTurn;
        turnTimer = turnDuration;
    }

    void EndPlayerTurn()
    {
        player.EndTurn();
        StartEnemyTurn();
    }

    void StartEnemyTurn()
    {
        currentTurn = TurnState.EnemyTurn;
        enemy.StartTurn(false); // false = enemigo
        turnTimer = turnDuration;

        Invoke("EndEnemyTurn", 2f);
    }

    void EndEnemyTurn()
    {
        enemy.EndTurn();
        StartPlayerTurn();
    }
}
