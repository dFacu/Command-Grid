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
                // Posición en el plano X–Z
                Vector3 pos = new Vector3(x * spacing, 0, y * spacing);

                GameObject tile = Instantiate(tilePrefab, pos, Quaternion.identity);

                // Nombre usando enteros (sin coma ni decimales)
                int posX = Mathf.RoundToInt(pos.x);
                int posZ = Mathf.RoundToInt(pos.z);
                tile.name = $"Tile_{posX}_{posZ}";

                tile.transform.parent = this.transform; // organiza en jerarquía

                // Asignar coordenadas lógicas al script TileClick
                TileClick click = tile.GetComponent<TileClick>();
                if (click != null)
                {
                    click.cellCoords = new Vector2Int(x, y);
                }
            }
        }
    }
}