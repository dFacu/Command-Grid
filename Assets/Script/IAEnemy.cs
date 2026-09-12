using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

    public class IAEnemy : MonoBehaviour
    {
        public static IAEnemy Instance;

        public int currentPoints;
        public int maxPoints = 8;
        public int turnNumber = 1;

        public List<GameObject> units = new List<GameObject>();

        private void Awake()   {      Instance = this;      }

        public void ResetTurn()
        {
            foreach (GameObject unit in units)
            {
                var und = unit.GetComponent<Unit>();
                und.hasMovedThisTurn = false;
            }
        }

        public void RegisterUnit(GameObject unit) { units.Add(unit); }

        public void EndTurn()
        {
            turnNumber++;
            ResetTurn();
        }

    }
