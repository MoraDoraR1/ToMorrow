using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("투사체 프리팹 (RectTransform + Projectile 스크립트 포함)")]
    public GameObject projectilePrefab;

    [Header("발사 위치 (UI RectTransform)")]
    public RectTransform firePoint;

    [Header("공격 대상 (UI RectTransform, Enemy)")]
    public RectTransform enemyTarget;

    [Header("투사체가 생성될 부모 (보통 Canvas 또는 그 하위 컨테이너)")]
    public Transform canvasTransform;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (enemyTarget != null)
            {
                ThrowProjectile();
            }
        }
    }

    /// <summary>현재 공격 대상 (보름달 자동공격 등에서 참조).</summary>
    public RectTransform CurrentTarget => enemyTarget;

    /// <summary>
    /// 현재 공격 대상을 갱신한다. (몬스터 재스폰 시 스포너가 호출)
    /// </summary>
    public void SetTarget(RectTransform target)
    {
        enemyTarget = target;
    }

    void ThrowProjectile()
    {
        if (projectilePrefab == null || firePoint == null || canvasTransform == null)
        {
            Debug.LogWarning("PlayerAttack: 필수 참조가 비어 있습니다.");
            return;
        }

        // Canvas(또는 지정된 부모) 아래에 생성해야 UI 계층에 정상적으로 표시됨
        GameObject projectileObj = Instantiate(projectilePrefab, canvasTransform);

        RectTransform projectileRect = projectileObj.GetComponent<RectTransform>();
        if (projectileRect == null)
        {
            Debug.LogError("PlayerAttack: 투사체 프리팹에 RectTransform이 없습니다. (UI 오브젝트가 맞는지 확인)");
            Destroy(projectileObj);
            return;
        }

        // 월드 좌표 기준으로 위치를 맞춰야 부모 계층이 달라도 정확히 정렬됨
        projectileRect.position = firePoint.position;

        Projectile projectileScript = projectileObj.GetComponent<Projectile>();
        if (projectileScript == null)
        {
            Debug.LogError("PlayerAttack: 투사체 프리팹에 Projectile 스크립트가 없습니다.");
            Destroy(projectileObj);
            return;
        }

        Vector2 direction = ((Vector2)enemyTarget.position - (Vector2)firePoint.position).normalized;
        projectileScript.SetDirection(direction);
        projectileScript.SetTarget(enemyTarget);

        // 클릭 데미지 = 해금된 캐릭터 스탯 합계 + 완성한 별자리 보너스
        // (CharacterManager가 없으면 프리팹 기본값을 그대로 쓴다)
        if (CharacterManager.Instance != null)
        {
            int dmg = CharacterManager.Instance.TotalClickDamage;
            if (ConstellationManager.Instance != null)
            {
                dmg += ConstellationManager.Instance.TotalClickDamageBonus;
            }

            // 치명타는 여기서 굴린다 — 데미지가 투사체에 실려 날아가므로 명중 시점이 아니라
            // 발사 시점에 확정돼야 한다.
            dmg = CombatStats.RollClick(dmg, out bool isCrit);
            projectileScript.SetCritical(isCrit);

            projectileScript.SetDamage(dmg);
        }
    }
}
