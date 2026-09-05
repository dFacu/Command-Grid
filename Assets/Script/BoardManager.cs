using UnityEngine;
using System.Collections.Generic;
public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;
    public Transform tilesParent;
    public Vector2 origin = Vector2.zero;
    public float spacing = 1f;

    private Dictionary<Vector2Int, GameObject> occupied = new Dictionary<Vector2Int, GameObject>();

    void Awake()
    {
        Instance = this;
    }

    public Vector3 CellToWorld(Vector2Int cell)
    {
        return new Vector3(origin.x + cell.x * spacing, origin.y + cell.y * spacing, 0f);
    }

    public bool IsCellFree(Vector2Int cell)
    {
        return !occupied.ContainsKey(cell);
    }

    public void SpawnUnit(GameObject prefab, Vector2Int cell, PlayerResources owner)
    {
        if (!IsCellFree(cell))
        {
            Debug.Log("Celda ocupada");
            return;
        }

        Vector3 pos = CellToWorld(cell);
        GameObject unit = Instantiate(prefab, pos, Quaternion.identity, tilesParent);
        // Si la unidad tiene un componente Unit, asignar owner, stats, etc.
        var unitComp = unit.GetComponent<Unit>();
        if (unitComp != null) unitComp.Initialize(owner, cell);

        occupied[cell] = unit;
    }

    public void RemoveUnitAt(Vector2Int cell)
    {
        if (occupied.TryGetValue(cell, out GameObject unit))
        {
            Destroy(unit);
            occupied.Remove(cell);
        }
    }

    public void RegisterUnitAt(Vector2Int cell, GameObject unit)
    {
        if (occupied.ContainsKey(cell))
        {
            Debug.Log("Celda ya ocupada, no se puede registrar otra unidad.");
            return;
        }
        occupied[cell] = unit;
    }
}
