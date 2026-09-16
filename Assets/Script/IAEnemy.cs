using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class IAEnemy : MonoBehaviour
{
    public static IAEnemy Instance;
    
    public PlayerResources resources;
    public PlayerResources resourcesPlayer;

    private void Awake()
    {
        Instance = this;
    }

    public void PlayTurn()
    {
        // Recorre todas sus Unidades en su equipo
        foreach (GameObject unitObj in resources.units)
        {
            Unit u = unitObj.GetComponent<Unit>();
            if (u != null && !u.hasMovedThisTurn)
            {
                // Objetivo: el comandante del jugador
                Unit target = resourcesPlayer.units[0].GetComponent<Unit>();

                //Si esta en rango de ataque → atacar
                if (u.IsInAttackRange(target))
                {
                    u.Attack(target);
                }
                else
                {
                    // IA simple: mover hacia el comandante del jugador
                    Vector3 enemyPos = u.transform.position;
                    Vector3 playerPos = resourcesPlayer.units[0].transform.position;

                    Vector3 targetPos = enemyPos;

                    if (playerPos.x > enemyPos.x) targetPos.x++;
                    else if (playerPos.x < enemyPos.x) targetPos.x--;

                    if (playerPos.z > enemyPos.z) targetPos.z++;
                    else if (playerPos.z < enemyPos.z) targetPos.z--;

                    u.MoveTo(targetPos);
                    u.hasMovedThisTurn = true;
                }

               
            }
        }

        resources.EndTurn();
    }
}
