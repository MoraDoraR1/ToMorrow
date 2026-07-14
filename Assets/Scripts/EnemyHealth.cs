using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int hp = 100;

    public void TakeDamage(int damage)
    {
        hp -= damage;

        Debug.Log("적 체력 : " + hp);

        if (hp <= 0)
        {
            Destroy(gameObject);
        }
    }
}
