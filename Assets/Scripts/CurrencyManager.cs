using System;
using UnityEngine;

/// <summary>
/// 게임 내 재화 '꿈 코인'을 관리하는 싱글턴.
/// 지금은 단순 카운터이며, 이후 ShopManager로 확장 예정.
/// </summary>
public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    [Header("보유 꿈 코인")]
    [SerializeField] private int dreamCoin = 0;

    public int DreamCoin => dreamCoin;

    // UI 등에서 재화 변동을 구독할 수 있도록 이벤트 제공
    public event Action<int> OnDreamCoinChanged;

    void Awake()
    {
        // 씬에 중복 생성되면 나중 것을 제거해 싱글턴 유지
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void AddDreamCoin(int amount)
    {
        if (amount <= 0) return;

        dreamCoin += amount;
        OnDreamCoinChanged?.Invoke(dreamCoin);
        Debug.Log("꿈 코인 : " + dreamCoin);
    }

    /// <summary>보유량이 충분하면 차감하고 true. 부족하면 아무것도 안 하고 false.</summary>
    public bool TrySpend(int amount)
    {
        if (amount <= 0) return true;
        if (dreamCoin < amount) return false;

        dreamCoin -= amount;
        OnDreamCoinChanged?.Invoke(dreamCoin);
        return true;
    }
}
