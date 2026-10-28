using UnityEngine;
using UnityEngine.EventSystems;

public class UnitSelector : MonoBehaviour, IPointerClickHandler
{
    public Unit unit;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (TurnManager.Instance.currentTurn != TurnManager.TurnState.PlayerTurn) return;

        PlayerResources.Instance.SelectUnit(unit);
       // Debug.Log($"Unidad selecionada: {unit.name}");
    }
}
