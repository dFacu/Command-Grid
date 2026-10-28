using UnityEngine;
using System.Collections.Generic;

public enum Owner { Player, AI }


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
        return new Vector3(cell.x * spacing, 0.5f, cell.y * spacing);
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
        if (unitComp != null) unitComp.Initialize(owner);

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

    // Nuevo método para registrar unidades en movimiento
    public void RegisterUnitAt(Vector2Int cell, GameObject unit)
    {
        occupied[cell] = unit;
    }

    public GameObject GetUnitAt(Vector2Int cell)
    {
        if (occupied.TryGetValue(cell, out GameObject unit))
        {
            return unit;
        }
        return null;
    }

    // --- Métodos para la IA ---
    public bool HasBoardSpace(Owner owner)
    {
        // Ejemplo simple: contar unidades del owner
        int count = 0;
        foreach (var kvp in occupied)
        {
            Unit u = kvp.Value.GetComponent<Unit>();
            if (u != null && u.owner.ownerType == owner) count++;
        }
        return count < 5; // límite arbitrario, ajustá según tu juego
    }

    public bool EnemyInRange(Owner owner)
    {
        foreach (var kvp in occupied)
        {
            Unit u = kvp.Value.GetComponent<Unit>();
            if (u != null && u.owner.ownerType != owner) return true;
        }
        return false;
    }

    public Unit GetBestEnemyTarget(Owner owner)
    {
        Unit best = null;
        int lowestHP = int.MaxValue;
        foreach (var kvp in occupied)
        {
            Unit u = kvp.Value.GetComponent<Unit>();
            if (u != null && u.owner.ownerType != owner)
            {
                if (u.health < lowestHP)
                {
                    lowestHP = u.health;
                    best = u;
                }
            }
        }
        return best;
    }

    public Unit GetLowestHealthAlly(Owner owner)
    {
        Unit best = null;
        int lowestHP = int.MaxValue;
        foreach (var kvp in occupied)
        {
            Unit u = kvp.Value.GetComponent<Unit>();
            if (u != null && u.owner.ownerType == owner)
            {
                if (u.health < lowestHP)
                {
                    lowestHP = u.health;
                    best = u;
                }
            }
        }
        return best;
    }

    public bool CommanderHealthBelow(Owner owner, float fraction)
    {
        // Ejemplo: si tenés un "Commander" como unidad especial
        foreach (var kvp in occupied)
        {
            Commander u = kvp.Value.GetComponent<Commander>();
            if (u != null && u.owner.ownerType == owner && !string.IsNullOrEmpty(u.commanderName))
            {
                return (float)u.health / u.health < fraction;
            }
        }
        return false;
    }

    // --- Helpers extra para IA ---

    public Unit FindBestEnemy(Owner owner, float range)
    {
        Unit best = null;
        int lowestHP = int.MaxValue;

        foreach (var kvp in occupied)
        {
            Unit enemy = kvp.Value.GetComponent<Unit>();

            if (enemy == null || enemy.owner == null)
                continue;

            // Buscar solamente unidades enemigas.
            if (enemy.owner.ownerType == owner)
                continue;

            // Comprobar si alguna unidad aliada puede alcanzar al enemigo.
            bool inRange = false;

            foreach (var allyEntry in occupied)
            {
                Unit ally = allyEntry.Value.GetComponent<Unit>();

                if (ally == null || ally.owner == null)
                    continue;

                if (ally.owner.ownerType != owner)
                    continue;

                float distance = Vector3.Distance(
                    ally.transform.position,
                    enemy.transform.position
                );

                if (distance <= range)
                {
                    inRange = true;
                    break;
                }
            }

            if (inRange && enemy.health < lowestHP)
            {
                lowestHP = enemy.health;
                best = enemy;
            }
        }

        return best;
    }
    public TileClick FindTrapTile(PlayerResources owner)
    {
        Vector2Int cell = FindStrategicTrapCell(owner);

        TileClick tile = GetTile(cell);

        if (tile == null)
            return null;

        if (tile.occupied || tile.hasTrap)
            return null;

        return tile;
    }
    public TileClick FindFreeTile(PlayerResources owner)
    {
        if (owner == null || GridControler.Instance == null)
            return null;

        GridControler grid = GridControler.Instance;

        // Buscar dos casillas adelante de las unidades existentes.
        foreach (GameObject obj in owner.units)
        {
            if (obj == null)
                continue;

            Unit unit = obj.GetComponent<Unit>();

            if (unit == null || unit.owner != owner)
                continue;

            // Convertir la posición de la unidad a coordenadas de la grilla.
            Vector2Int posicion = new Vector2Int(
                Mathf.RoundToInt(obj.transform.position.x),
                Mathf.RoundToInt(obj.transform.position.z)
            );

            // La IA avanza hacia Y negativo.
            Vector2Int destino = posicion + new Vector2Int(0, -2);

            TileClick tile = GetTile(destino);

            if (tile != null && !tile.occupied && !tile.hasTrap)
                return tile;
        }

        // Si todavía no tiene unidades, empezar por la última fila.
        for (int x = 0; x < grid.width; x++)
        {
            int y = grid.height - 1;

            TileClick tile = GetTile(new Vector2Int(x, y));

            if (tile != null && !tile.occupied && !tile.hasTrap)
                return tile;
        }

        return null;
    }

    public Vector2Int FindStrategicTrapCell(PlayerResources owner)
    {
        // recorrer todas las celdas del tablero
        for (int x = 0; x < GridControler.Instance.width; x++)
        {
            for (int y = 0; y < GridControler.Instance.height; y++)
            {
                TileClick tile = GridControler.Instance.gridCells[x, y].GetComponent<TileClick>();

                // condición: celda libre y sin trampa
                if (!tile.occupied && !tile.hasTrap)
                {
                    return new Vector2Int(x, y);
                }
            }
        }

        // si no encuentra nada, devuelve Vector2Int.zero
        return Vector2Int.zero;
    }

    public TileClick GetTile(Vector2Int cell)
    {
        // Usamos el GridControler para acceder a la matriz de tiles
        if (GridControler.Instance != null &&
            cell.x >= 0 && cell.x < GridControler.Instance.width &&
            cell.y >= 0 && cell.y < GridControler.Instance.height)
        {
            GameObject tileObj = GridControler.Instance.gridCells[cell.x, cell.y];
            if (tileObj != null)
            {
                return tileObj.GetComponent<TileClick>();
            }
        }
        return null;
    }

    public List<TileClick> GetAreaTiles(Vector2Int centerCell, AreaShape shape, int size)
    {
        return GridControler.Instance.GetAreaTiles(centerCell, shape, size);
    }

}
