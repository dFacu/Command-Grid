using UnityEngine;

public class SpriteDirection : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private SpriteRenderer spriteRenderer2;

    // true = mira a la derecha
    // false = mira a la izquierda
    [SerializeField] private bool miraDerechaPorDefecto = true;


    public void MirarDerecha()
    {
        spriteRenderer.flipX = !miraDerechaPorDefecto;
        spriteRenderer2.flipX = !miraDerechaPorDefecto;
    }

    public void MirarIzquierda()
    {
        spriteRenderer.flipX = miraDerechaPorDefecto;
        spriteRenderer2.flipX = miraDerechaPorDefecto;
    }
}