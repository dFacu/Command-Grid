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
        playerData.InitPlayer(playerData.DeckCargado);
        GridControler.Instance.AddObjectToGrid(Comandante_1, new Vector3(0, 0.5f, 0));
        //player.SpawnUnit(Comandante_1, new Vector3(0, 0, 0), player);
        iAEnemy.InitPlayer(iAEnemy.DeckCargado);
        GridControler.Instance.AddObjectToGrid(Comandante_2, new Vector3(4, 0.5f, 4));
        //enemy.SpawnUnit(Comandante_2, new Vector3(4, 4, 0), enemy);

    }




    //--------------------------------------


}
