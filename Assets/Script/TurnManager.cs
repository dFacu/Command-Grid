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

    public void EndEnemyTurn()
    {
        enemy.EndTurn();

        // Reducir la duración de las zonas activas.
        EffectZone[] zones =
            FindObjectsByType<EffectZone>(FindObjectsSortMode.None);

        foreach (EffectZone zone in zones)
        {
            if (zone != null)
                zone.EndTurn();
        }

        StartPlayerTurn();
    }

    void StartPlayerTurn()
    {
        //player.InitPlayer();
        player.StartTurn(true);

        currentTurn = TurnState.PlayerTurn;
        turnTimer = turnDuration;
    }



   public void StartEnemyTurn()
    {
        currentTurn = TurnState.EnemyTurn;
        enemy.StartTurn(false); // recursos del enemigo
        turnTimer = turnDuration;

        IAEnemy.Instance.PlayTurn(); // IA arranca su lógica
    }

}
