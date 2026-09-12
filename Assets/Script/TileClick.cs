using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class TileClick : MonoBehaviour, IPointerClickHandler
{
    public Vector3 cellCoords; // coordenadas lógicas del tile
    public bool occupied = false;

    private void Start()
    {
        cellCoords = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Comandante"))
            occupied = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Comandante"))
            occupied = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        PlayerResources.Instance.TryMoveSelectd(cellCoords);
    }
}
