using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("체력")]
    public int hp = 100;

    [Header("처치 시 지급할 꿈 코인")]
    public int coinReward = 1;

    [Header("스테이지 처치 수(0/50)에 포함 (보스는 false)")]
    public bool countsTowardStage = true;

    // 스포너가 구독하여 사망 시 다음 몬스터를 스폰한다
    public event Action<EnemyHealth> OnDeath;
    // HP바 등에서 구독 — (현재 HP, 최대 HP)
    public event Action<int, int> OnHealthChanged;

    // 시작 시의 hp를 최대 체력으로 기억 (HP바 비율 계산용)
    public int MaxHp { get; private set; }
    public int CurrentHp => hp;

    void Awake()
    {
        MaxHp = hp;
    }

    public void TakeDamage(int damage)
    {
        hp -= damage;

        Debug.Log("적 체력 : " + hp);

        // 체력 변경 통지 (0 밑으로는 표시상 0으로 고정)
        OnHealthChanged?.Invoke(Mathf.Max(hp, 0), MaxHp);

        if (hp <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // 처치 보상: 꿈 코인 지급
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.AddDreamCoin(coinReward);
        }

        // 스테이지 처치 수 등록 (보스는 countsTowardStage=false로 제외)
        if (countsTowardStage && StageManager.Instance != null)
        {
            StageManager.Instance.RegisterKill();
        }

        // 사망 통지 (스포너가 다음 몬스터 스폰)
        OnDeath?.Invoke(this);

        // UI(Canvas) 잔상(ghost mesh) 방지: 비활성화하면 캔버스가 이 요소 없이
        // 즉시 리빌드되어, 파괴된 이미지가 화면에 남는 현상이 사라진다.
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
