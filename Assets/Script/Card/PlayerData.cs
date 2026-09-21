using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance;

    public string usuarioName;

    public List<CardSO> DeckCargado;


    private void Awake()
    {
        Instance = this;
    }


}
