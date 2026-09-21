using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class TileClick : MonoBehaviour, IPointerClickHandler
{
    public Vector3 cellCoords; // coordenadas lógicas del tile
    public bool occupied = false;

    public bool hasTrap = false;
    public int trapDamage = 0;
    private void Start()
    {
        cellCoords = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Comandante"))
        {
            occupied = true;

            // Si hay trampa → explota
            if (hasTrap)
            {
                Unit unit = other.GetComponent<Unit>();
                if (unit != null)
                {
                    unit.TakeDamage(trapDamage);
                    hasTrap = false; // se consume
                    Debug.Log($"¡Trampa explotó! {unit.name} recibió {trapDamage} de daño.");
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Comandante"))
            occupied = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (PlayerResources.Instance.isPlacingTrap)
        {
            PlayerResources.Instance.PlaceTrap(this, PlayerResources.Instance.cardPendingTrap);
            PlayerResources.Instance.cardPendingTrap = null;
        }
        else if (PlayerResources.Instance.isPlacingSpawn)
        {
            PlayerResources.Instance.SpawnUnitAtTile(this, PlayerResources.Instance.cardPendingSpawn);
            PlayerResources.Instance.cardPendingSpawn = null;
        }
        else
        {
            PlayerResources.Instance.TryMoveOrAttack(cellCoords);
        }
    }
}
