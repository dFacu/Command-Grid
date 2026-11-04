using TMPro;
using Unity.Mathematics;
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

    public TextMeshProUGUI textTurn;
    public TextMeshProUGUI textTiempoTurn;
    public GameObject button;
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
        textTiempoTurn.text = turnTimer.ToString("F0");



        if(turnTimer <= 0) // si el tiempo llega a 0 cambia a alguno de estos
        {
            if(currentTurn == TurnState.PlayerTurn)
                EndPlayerTurn(); 
            else if (currentTurn == TurnState.EnemyTurn)
                EndEnemyTurn();
        }
    }

   public  void EndPlayerTurn()
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
        currentTurn = TurnState.PlayerTurn;

        if (currentTurn == TurnState.PlayerTurn) // Turno player
        {
            textTurn.text = "Tu Turno";
            button.SetActive(true);
        }

        player.StartTurn(true);
        turnTimer = turnDuration;
    }



   public void StartEnemyTurn()
    {
        currentTurn = TurnState.EnemyTurn;

        if (currentTurn == TurnState.EnemyTurn) // Turno Enemy
        {
            textTurn.text = "Turno Del Oponente";
            button.SetActive(false);
        }

        enemy.StartTurn(false); // recursos del enemigo
        turnTimer = turnDuration;

        IAEnemy.Instance.PlayTurn(); // IA arranca su lógica
    }

}
