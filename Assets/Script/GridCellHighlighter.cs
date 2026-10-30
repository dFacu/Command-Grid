
using UnityEngine;
using static TurnManager;

public class GridCellHighlighter : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    private Color originalColor;

    public Color highlightColor = Color.cadetBlue;
    public Color posColor = Color.green;
    public Color negColor = Color.red;

    public TileClick gridCell;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        gridCell = GetComponent<TileClick>();

        originalColor = meshRenderer.material.color;
    }

    private void OnMouseEnter()
    {
        if (TurnManager.Instance.currentTurn == TurnState.EnemyTurn)
        {
            meshRenderer.material.color = highlightColor;
            return;
        }

        Unit selectedUnit =  Manager.Instance.player.selectedUnit;

        if (selectedUnit == null)
        {
            meshRenderer.material.color = highlightColor;
            return;
        }

        if (gridCell.occupied)
        {
            meshRenderer.material.color = negColor;
            return;
        }

        int distance = Mathf.Abs(
            gridCell.cellCoords.x - selectedUnit.currentCell.x
        ) + Mathf.Abs(
            gridCell.cellCoords.y - selectedUnit.currentCell.y
        );

        int availableMovement = EffectZone.IsInsideMovementPenalty(
            selectedUnit,
            gridCell.cellCoords
        )
        ? Mathf.CeilToInt(selectedUnit.movementRange * 0.5f)
        : selectedUnit.movementRange;

        meshRenderer.material.color =
            distance <= availableMovement ? posColor : negColor;
    }

    private void OnMouseExit()
    {
        meshRenderer.material.color = originalColor;
    }
}
