using UnityEngine;

/// <summary>
/// 보름달 자동 공격 — 클릭 없이 일정 주기마다 현재 몬스터에게 즉시 데미지를 준다.
/// 대상은 PlayerAttack의 현재 타겟(일반/보스 공용)을 사용한다.
///
/// 기본 데미지/주기는 Balance.csv 값이며, 여기에 구매한 캐릭터 이야기(StoryManager)의
/// 영구 강화가 더해져 실제 공격력과 속도가 결정된다.
/// </summary>
public class FullMoonAttack : MonoBehaviour
{
    [Header("현재 타겟을 가진 PlayerAttack")]
    public PlayerAttack playerAttack;

    [Header("기본 데미지 (Balance.csv)")]
    public int damage = 5;

    [Header("기본 공격 주기 (초, Balance.csv)")]
    public float interval = 1f;

    [Header("최소 공격 주기 (이야기로 아무리 줄여도 이 아래로는 안 내려감)")]
    public float minInterval = 0.1f;

    private float timer;

    /// <summary>기본 데미지 + 구매한 이야기들의 강화량.</summary>
    public int CurrentDamage
    {
        get
        {
            int bonus = StoryManager.Instance != null ? StoryManager.Instance.TotalMoonDamageBonus : 0;
            return damage + bonus;
        }
    }

    /// <summary>기본 주기 − 이야기 감소량 (minInterval 밑으로는 안 내려감).</summary>
    public float CurrentInterval
    {
        get
        {
            float reduction = StoryManager.Instance != null ? StoryManager.Instance.TotalMoonIntervalReduction : 0f;
            return Mathf.Max(minInterval, interval - reduction);
        }
    }

    void Update()
    {
        if (playerAttack == null) return;

        timer += Time.deltaTime;
        if (timer >= CurrentInterval)
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
            enemy.TakeDamage(CurrentDamage);
        }
    }
}
