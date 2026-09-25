using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NUnit.Framework;
using System.Collections.Generic;

public class CardUI : MonoBehaviour
{
    public string name;
    public TextMeshProUGUI nameText;

    public string description;
    public TextMeshProUGUI descriptionText;

    public int cost;
    public TextMeshProUGUI costText;

    public GameObject powerPanel;
    public int value;
    public TextMeshProUGUI valueText;

    public CardType type;

    public List<Element> elements;

    public EffectType effectType;

    public GameObject Prefab;

    public int distance;
    public void LoadCard(CardSO card)
    {
        Prefab = card.prefab;
        name = card.name;
        nameText.text = card.name.ToString();

        description = card.description;
        descriptionText.text = description.ToString();

        cost = card.cost;
        costText.text = cost.ToString();

        value = card.value;

        if(value == 0)
        {
            powerPanel.SetActive(false);
        }
        else
        {
            powerPanel.SetActive(true);
            valueText.text = value.ToString();
        }

        type = card.type;
        elements = card.elements;
        effectType = card.effectType;
        distance = card.distance;
    }


}
