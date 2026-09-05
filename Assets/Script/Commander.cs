using UnityEngine;

public class Commander : Unit
{
    public string commanderName = "Comandante";
    public int specialAbilityCost = 3;

    // Ejemplo de habilidad especial
    public void UseSpecialAbility()
    {
        if (owner.SpendPoints(specialAbilityCost))
        {
            Debug.Log($"{commanderName} usa su habilidad especial");
            // Aquí podés meter lógica: curar unidades, buff global, etc.
        }
        else
        {
            Debug.Log("No hay suficientes puntos de comando para la habilidad especial");
        }
    }
}
