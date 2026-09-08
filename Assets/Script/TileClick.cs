using UnityEngine;
using UnityEngine.EventSystems;

public class TileClick : MonoBehaviour
{
    public Vector2Int cellCoords; // coordenadas lógicas del tile

    private void OnMouseDown()
    {
        // Solo permitir movimiento en turno del jugador
        if (TurnManager.Instance.currentTurn != TurnManager.TurnState.PlayerTurn) return;

        // Buscar el Comandante rojo (podés asignarle un tag "PlayerCommander")
        Commander commander = GameObject.FindWithTag("PlayerCommander").GetComponent<Commander>();
        if (commander != null)
        {
            commander.MoveTo(cellCoords);
        }
    }
}
