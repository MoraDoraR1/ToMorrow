using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public GameObject projectilePrefab;
    public Transform firePoint;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        Enemy enemy = FindObjectOfType<Enemy>();

        if (enemy == null)
            return;

        GameObject obj = Instantiate(projectilePrefab,
                                     firePoint.position,
                                     Quaternion.identity);

        obj.GetComponent<Projectile>().SetTarget(enemy.transform);
    }
}