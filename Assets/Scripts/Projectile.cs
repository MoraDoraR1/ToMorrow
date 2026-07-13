using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;

    Transform target;

    public void SetTarget(Transform targetTransform)
    {
        target = targetTransform;
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position =
            Vector2.MoveTowards(transform.position,
                                target.position,
                                speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, target.position) < 0.1f)
        {
            target.GetComponent<Enemy>().TakeDamage(1);

            Destroy(gameObject);
        }
    }
}