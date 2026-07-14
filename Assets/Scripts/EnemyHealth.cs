using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("체력")]
    public int hp = 100;

    [Header("처치 시 지급할 꿈 코인")]
    public int coinReward = 1;

    // 스포너가 구독하여 사망 시 다음 몬스터를 스폰한다
    public event Action<EnemyHealth> OnDeath;

    public void TakeDamage(int damage)
    {
        hp -= damage;

        Debug.Log("적 체력 : " + hp);

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

        // 사망 통지 (스포너가 다음 몬스터 스폰)
        OnDeath?.Invoke(this);

        // UI(Canvas) 잔상(ghost mesh) 방지: 비활성화하면 캔버스가 이 요소 없이
        // 즉시 리빌드되어, 파괴된 이미지가 화면에 남는 현상이 사라진다.
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
