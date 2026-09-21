using UnityEngine;

public class Manager : MonoBehaviour
{
    public static Manager Instance;

    [SerializeField] GameObject Comandante_1;
    [SerializeField] GameObject Comandante_2;

    public PlayerResources player;
    public PlayerData playerData;

    public IAEnemy iAEnemy;
    public PlayerResources enemy;

    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        GridControler.Instance.DashboardGenerator();
        player.InitPlayer(playerData.DeckCargado);
        player.SpawnUnit(Comandante_1, new Vector3(0, 0, 0), player);
        enemy.InitPlayer(iAEnemy.DeckCargado);
        enemy.SpawnUnit(Comandante_2, new Vector3(4, 4, 0), enemy);

    }




    //--------------------------------------


}
