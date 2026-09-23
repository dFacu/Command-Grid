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
                // Posición en el mundo directamente en positivo
                Vector3 spawnPosition = new Vector3(x, 0, z);

                GameObject gridCell = Instantiate(tile, spawnPosition, Quaternion.identity);
                gridCell.transform.SetParent(transform);

                // Guardar coordenadas de celda en X–Z
                gridCell.GetComponent<TileClick>().cellCoords = spawnPosition;

                gridCells[x, z] = gridCell;
            }
        }

    }

    public bool AddObjectToGrid(GameObject obj, Vector3 gridPosition)
    {
        if (gridPosition.x >= 0 && gridPosition.x < width &&
       gridPosition.z >= 0 && gridPosition.z < height)
        {
            TileClick cell = gridCells[(int)gridPosition.x, (int)gridPosition.z].GetComponent<TileClick>();

            if(cell.occupied) return false;
            else
            {
                Vector3 vec = new Vector3(cell.GetComponent<Transform>().position.x, 0.3f, cell.GetComponent<Transform>().position.z);
                GameObject newObj = Instantiate(obj, vec, Quaternion.identity);
                newObj.transform.SetParent(transform);
                gridObject.Add(newObj);
                cell.unitInCell = newObj;
                cell.occupied = true;
                return true;
            }
        }
        else return false;
    }
}
