using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    public enum TurnState { PlayerTurn, EnemyTurn }
    public TurnState currentTurn = TurnState.PlayerTurn;

    public float turnDuration = 30f;
    private float turnTimer;

    public PlayerResources player;
    public PlayerResources enemy;


    void Awake()
    {
        Instance = this;
    }


    private void Start()
    {
        StartPlayerTurn();
    }
    void Update()
    {
        turnTimer -= Time.deltaTime;

        if(turnTimer <= 0) // si el tiempo llega a 0 cambia a alguno de estos
        {
            if(currentTurn == TurnState.PlayerTurn)
                EndPlayerTurn();
            else if (currentTurn == TurnState.EnemyTurn)
                EndEnemyTurn();
        }
        
        if(currentTurn == TurnState.PlayerTurn && Input.GetKeyDown(KeyCode.Space))
        {
            EndPlayerTurn();
        }
    }

    void EndPlayerTurn()
    {
        player.EndTurn();
        StartEnemyTurn();
    }

    void EndEnemyTurn()
    {
        enemy.EndTurn();
        StartPlayerTurn();
    }


    void StartPlayerTurn()
    {
        //player.InitPlayer();
        player.StartTurn(true);

        currentTurn = TurnState.PlayerTurn;
        turnTimer = turnDuration;
    }



    void StartEnemyTurn()
    {
        IAEnemy.Instance.PlayTurn();
        currentTurn = TurnState.EnemyTurn;
        enemy.StartTurn(false); // false = enemigo
        turnTimer = turnDuration;

        Invoke("EndEnemyTurn", 2f);
    }


}
