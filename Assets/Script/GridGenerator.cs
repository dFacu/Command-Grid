using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    public GameObject tilePrefab;
    public int width = 5;
    public int height = 7;
    public float spacing = 1.0f;

    private void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = new Vector3(x * spacing, 0 ,y * spacing);
                GameObject tile = Instantiate(tilePrefab, pos, Quaternion.identity);
                tile.name = $"Tile_{x}_{y}";
                tile.transform.parent = this.transform; // organiza en jerarquía
            }
        }
    }
}
