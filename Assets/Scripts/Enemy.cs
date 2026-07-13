using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int hp = 20;

    public void TakeDamage(int damage)
    {
        hp -= damage;

        if (hp <= 0)
        {
            Destroy(gameObject);
        }
    }
}
