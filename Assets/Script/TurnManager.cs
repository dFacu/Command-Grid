using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    public GameObject commanderRedPrefab;
    public GameObject commanderBluePrefab;

    public enum TurnState { PlayerTurn, EnemyTurn }
    public TurnState currentTurn = TurnState.PlayerTurn;

    public float turnDuration = 30f;
    private float turnTimer;

    public PlayerResources player;
    public PlayerResources enemy;

    private Commander commanderRed;
    private Commander commanderBlue;

    void Awake()
    {
        Instance = this;
    }


    private void Start()
    {
        // Instanciar comandantes en posiciones iniciales
        BoardManager.Instance.SpawnUnit(commanderRedPrefab, new Vector2Int(0, 0), player);
        BoardManager.Instance.SpawnUnit(commanderBluePrefab, new Vector2Int(4, 6), enemy);

        commanderBlue = commanderBluePrefab.GetComponent<Commander>();
        commanderRed = commanderRedPrefab.GetComponent<Commander>();

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
        
        if(currentTurn == TurnState.PlayerTurn && Input.GetKeyDown(KeyCode.Space))
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


        // IA simple: mover hacia el jugador
        if (commanderBlue != null && commanderRed != null)
        {
            Vector2Int enemyPos = commanderBlue.GetCurrentCell();
            Vector2Int playerPos = commanderRed.GetCurrentCell();

            Vector2Int target = enemyPos;
            if (playerPos.x > enemyPos.x) target.x++;
            else if (playerPos.x < enemyPos.x) target.x--;

            if (playerPos.y > enemyPos.y) target.y++;
            else if (playerPos.y < enemyPos.y) target.y--;

            commanderBlue.MoveTo(target);
        }

        Invoke("EndEnemyTurn", 2f);
    }

    void EndEnemyTurn()
    {
        enemy.EndTurn();
        StartPlayerTurn();
    }
}
