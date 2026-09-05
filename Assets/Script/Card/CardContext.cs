// CardContext.cs
using UnityEngine;

public class CardContext
{
    public PlayerResources owner;        // quien juega la carta
    public Vector2Int? targetCell;       // celda objetivo opcional
    public GameObject targetUnit;        // unidad objetivo opcional
    public TurnManager turnManager;      // referencia si hace falta
    public BoardManager boardManager;    // referencia al tablero
}