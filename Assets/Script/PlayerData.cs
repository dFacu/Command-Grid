using UnityEngine;
using TMPro;

public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance { get; private set; }

    public TextMeshProUGUI textPoint;
    public TextMeshProUGUI textLifeComander;
    //public TextMeshProUGUI textName;


    private void Update()
    {
        textLifeComander.text = Manager.Instance.player.units[0].GetComponent<Unit>().health.ToString();
        textPoint.text = Manager.Instance.player.currentPoints.ToString();
    }
}
