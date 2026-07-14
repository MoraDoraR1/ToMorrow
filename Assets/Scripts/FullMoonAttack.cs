using UnityEngine;

/// <summary>
/// 보름달 자동 공격 — 클릭 없이 일정 주기마다 현재 몬스터에게 즉시 데미지를 준다.
/// 대상은 PlayerAttack의 현재 타겟(일반/보스 공용)을 사용한다.
/// 데미지/주기는 임시값이며, 이후 캐릭터 스탯 참조로 확장 예정.
/// </summary>
public class FullMoonAttack : MonoBehaviour
{
    [Header("현재 타겟을 가진 PlayerAttack")]
    public PlayerAttack playerAttack;

    [Header("한 번에 주는 데미지 (임시값)")]
    public int damage = 5;

    [Header("공격 주기 (초, 임시값)")]
    public float interval = 1f;

    private float timer;

    void Update()
    {
        if (playerAttack == null || interval <= 0f) return;

        timer += Time.deltaTime;
        if (timer >= interval)
        {
            timer = 0f;
            AutoAttack();
        }
    }

    void AutoAttack()
    {
        RectTransform target = playerAttack.CurrentTarget;
        if (target == null) return; // 몬스터가 없거나 파괴된 순간엔 건너뜀

        EnemyHealth enemy = target.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }
    }
}
