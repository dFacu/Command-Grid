using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class GridControler : MonoBehaviour
{
    public static GridControler Instance;

    [SerializeField] GameObject tile;

    public int width = 8;
    public int height = 4;

    public List<GameObject> gridObject = new List<GameObject>();

    public GameObject[,] gridCells;

    private void Awake()
    {
        Instance = this;
    }
    
    public void DashboardGenerator()
    {
        gridCells = new GameObject[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                // Coordenadas lógicas de la celda
                Vector2Int spawnCoords = new Vector2Int(x, z);

                // Convertir a posición en el mundo (X–Z → Vector3)
                Vector3 worldPos = new Vector3(spawnCoords.x, 0f, spawnCoords.y);

                GameObject gridCell = Instantiate(tile, worldPos, Quaternion.identity);
                gridCell.transform.SetParent(transform);

                // Guardar coordenadas lógicas en el TileClick
                gridCell.GetComponent<TileClick>().cellCoords = spawnCoords;

                gridCells[x, z] = gridCell;
            }
        }

    }

    public bool AddObjectToGrid(GameObject obj, Vector2Int gridPosition, PlayerResources owner)
    {
        if (gridPosition.x >= 0 && gridPosition.x < width &&
            gridPosition.y >= 0 && gridPosition.y < height)
        {
            TileClick cell = gridCells[gridPosition.x, gridPosition.y].GetComponent<TileClick>();

            if (cell.occupied) return false;
            else
            {
                // Convertimos la celda lógica a posición en mundo
                Vector3 worldPos = new Vector3(gridPosition.x, 0.5f, gridPosition.y);

                GameObject newObj = Instantiate(obj, worldPos, Quaternion.identity);
                var unitComp = newObj.GetComponent<Unit>();
                unitComp.currentCell = gridPosition; 
                
                if (unitComp != null) unitComp.Initialize(owner);

                newObj.transform.SetParent(transform);

                gridObject.Add(newObj);
                owner.RegisterUnit(newObj);

                cell.unitInCell = newObj;
                cell.occupied = true;

                return true;
            }
        }
        else return false;
    }

    public List<TileClick> GetAreaTiles(Vector2Int centerCell, AreaShape shape, int size)
    {
        List<TileClick> tiles = new List<TileClick>();

        // Siempre agregamos el centro
        tiles.Add(gridCells[centerCell.x, centerCell.y].GetComponent<TileClick>());

        if (shape == AreaShape.Cross)
        {
            if (centerCell.y + size < height)
                tiles.Add(gridCells[centerCell.x, centerCell.y + size].GetComponent<TileClick>());
            if (centerCell.y - size >= 0)
                tiles.Add(gridCells[centerCell.x, centerCell.y - size].GetComponent<TileClick>());
            if (centerCell.x + size < width)
                tiles.Add(gridCells[centerCell.x + size, centerCell.y].GetComponent<TileClick>());
            if (centerCell.x - size >= 0)
                tiles.Add(gridCells[centerCell.x - size, centerCell.y].GetComponent<TileClick>());
        }

        return tiles;
    }

}
