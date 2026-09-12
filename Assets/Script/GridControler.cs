using UnityEngine;

public class GridControler : MonoBehaviour
{
    public static GridControler Instance;

    [SerializeField] GameObject tile;
    public int width = 5;
    public int height = 5;

    private void Awake()
    {
        Instance = this;
    }
    
    public void DashboardGenerator()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = new Vector3(x, 0, y);
                GameObject til =  Instantiate(tile,pos,Quaternion.identity);
                til.name = $"Tile_{x}_{y}";
                til.transform.parent = this.transform;

            }
        }
    }
}
