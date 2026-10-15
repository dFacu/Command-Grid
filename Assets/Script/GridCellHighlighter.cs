using UnityEngine;
using static TurnManager;

public class GridCellHighlighter : MonoBehaviour
{
    private MeshRenderer _material;

    public Color highlightColor = Color.cadetBlue;
    public Color posColor = Color.green;
    public Color negColor = Color.red;

    private Color originalColor;

    public TileClick gridCell;
    private void Awake()
    {
        _material = GetComponent<MeshRenderer>();  
        gridCell = GetComponent<TileClick>();
        originalColor = _material.materials[0].color;
    }

    private void OnMouseEnter()
    {
        if(TurnManager.Instance.currentTurn  == TurnState.EnemyTurn)
        {
            _material.materials[0].color = highlightColor;
        }
        else if (gridCell.occupied || gridCell.cellCoords.x > 1)
        {
            _material.materials[0].color = negColor;
        }
        else _material.materials[0].color = posColor;
    }

    private void OnMouseExit()
    {
        _material.materials[0].color = originalColor;
    }
}
