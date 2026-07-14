using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class Projectile : MonoBehaviour
{
    [Header("이동 속도 (UI 픽셀/초 단위라 값이 커야 함)")]
    public float speed = 300f;

    [Header("데미지")]
    public int damage = 10;

    [Header("타겟에 명중했다고 판정할 거리 (픽셀)")]
    public float hitRadius = 30f;

    [Header("자동 파괴까지의 시간 (타겟을 못 맞췄을 경우)")]
    public float lifeTime = 3f;

    private Vector2 direction;
    private RectTransform rectTransform;
    private RectTransform targetRect;
    private bool hasHit = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Start()
    {
        // 타겟을 못 맞추고 화면 밖으로 나가는 경우를 대비한 안전장치
        Destroy(gameObject, lifeTime);
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
    }

    public void SetTarget(RectTransform target)
    {
        targetRect = target;
    }

    void Update()
    {
        if (hasHit) return;

        // RectTransform 이동은 anchoredPosition 기준
        rectTransform.anchoredPosition += direction * speed * Time.deltaTime;

        if (targetRect != null)
        {
            // Physics2D 콜라이더 대신 월드 좌표 기준 거리로 명중 판정
            float distance = Vector2.Distance(rectTransform.position, targetRect.position);
            if (distance <= hitRadius)
            {
                HandleHit();
            }
        }
    }

    void HandleHit()
    {
        hasHit = true;

        if (targetRect != null)
        {
            EnemyHealth enemy = targetRect.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }

        Destroy(gameObject);
    }
}
