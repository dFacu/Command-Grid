using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class Manager : MonoBehaviour
{
    public static Manager Instance;

    [SerializeField] GameObject Comandante_1;
    [SerializeField] GameObject Comandante_2;

    public PlayerResources player;

    public IAEnemy iAEnemy;
    public PlayerResources enemy;

    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        GridControler.Instance.DashboardGenerator();
        GridControler.Instance.AddObjectToGrid(Comandante_1, new Vector2Int(0, 0), player);
        GridControler.Instance.AddObjectToGrid(Comandante_2, new Vector2Int(4, 4), enemy);
        iAEnemy.InitPlayer(iAEnemy.deckManager.allCards);
    }




    //--------------------------------------


}
