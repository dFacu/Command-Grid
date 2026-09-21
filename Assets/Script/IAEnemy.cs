using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR;

public class IAEnemy : MonoBehaviour
{
    public static IAEnemy Instance;
    
    public PlayerResources resources;
    public PlayerResources resourcesPlayer;


    public List<CardSO> DeckCargado;
    public List<CardSO> hand;

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


    public void SpawnUnit(GameObject unit, Vector3 cell, PlayerResources owner)
    {
        Vector3 pos = new Vector3(cell.x, 0.5f, cell.y);

        GameObject clon = Instantiate(unit, pos, Quaternion.identity);

        var unitComp = clon.GetComponent<Unit>();
        if (unitComp != null) unitComp.Initialize(owner);

        owner.RegisterUnit(unitComp.gameObject);
    }

    public CardSO DrawCard()
    {
        if (DeckCargado.Count == 0) return null;

        CardSO draw = DeckCargado[0];
        DeckCargado.RemoveAt(0);
        return draw;
    }
    public void InitPlayer(List<CardSO> startingDeck)
    {
        //Roba x cartas iniciales
        for (int i = 0; i < 4; i++)
        {
            CardSO card = DrawCard();
            if (card != null)
                hand.Add(card);
        }
    }
}
