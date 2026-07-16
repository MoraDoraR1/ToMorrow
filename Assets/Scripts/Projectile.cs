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
    public float lifeTime = 3.5f;

    private Vector2 direction;
    private RectTransform rectTransform;
    private RectTransform targetRect;
    private bool hasHit = false;
    private bool isCritical = false;

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

    public void SetDamage(int dmg)
    {
        damage = dmg;
    }

    /// <summary>이 투사체가 치명타인지. (발사 시점에 이미 굴려진 결과를 받는다 — 연출용)</summary>
    public void SetCritical(bool crit)
    {
        isCritical = crit;
    }

    /// <summary>치명타 여부. 명중 연출/로그에 쓴다.</summary>
    public bool IsCritical => isCritical;

    void Update()
    {
        if (hasHit) return;

        // 타겟(몬스터)이 파괴되면 투사체도 즉시 정리한다.
        // 이렇게 안 하면 타겟을 잃은 투사체가 명중 판정 없이 화면을 가로질러
        // lifeTime 동안 남아, 마치 잔상처럼 보인다.
        if (targetRect == null)
        {
            DestroySelf();
            return;
        }

        // RectTransform 이동은 anchoredPosition 기준
        rectTransform.anchoredPosition += direction * speed * Time.deltaTime;

        // Physics2D 콜라이더 대신 월드 좌표 기준 거리로 명중 판정
        float distance = Vector2.Distance(rectTransform.position, targetRect.position);
        if (distance <= hitRadius)
        {
            HandleHit();
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

        DestroySelf();
    }

    void DestroySelf()
    {
        // UI(Canvas) 잔상(ghost mesh) 방지: 비활성화 후 파괴
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
