using System;
using UnityEngine;

/// <summary>
/// 캐릭터 이야기를 관리한다. (캐릭터당 1개)
/// 해당 캐릭터가 일정 레벨에 도달하면 열리고, 구매하면 달 자동공격이 영구 강화된다.
/// </summary>
public class StoryManager : MonoBehaviour
{
    public static StoryManager Instance { get; private set; }

    [Header("이야기 목록 (캐릭터당 1개)")]
    [SerializeField] private StoryData[] stories;

    // 구매로 상태가 바뀌었을 때 (UI 갱신용)
    public event Action OnStoriesChanged;

    public int Count => stories != null ? stories.Length : 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public StoryData Get(int index)
    {
        if (stories != null && index >= 0 && index < stories.Length) return stories[index];
        return null;
    }

    /// <summary>구매한 이야기들의 달 데미지 증가 합.</summary>
    public int TotalMoonDamageBonus
    {
        get
        {
            int sum = 0;
            if (stories != null)
            {
                foreach (StoryData s in stories)
                {
                    if (s != null && s.purchased) sum += s.moonDamageBonus;
                }
            }
            return sum;
        }
    }

    /// <summary>구매한 이야기들의 달 공격주기 감소 합.</summary>
    public float TotalMoonIntervalReduction
    {
        get
        {
            float sum = 0f;
            if (stories != null)
            {
                foreach (StoryData s in stories)
                {
                    if (s != null && s.purchased) sum += s.moonIntervalReduction;
                }
            }
            return sum;
        }
    }

    /// <summary>이야기가 열렸는가 — 해당 캐릭터가 해금됐고 필요 레벨 이상.</summary>
    public bool IsUnlocked(int index)
    {
        StoryData s = Get(index);
        if (s == null) return false;

        CharacterManager cm = CharacterManager.Instance;
        if (cm == null) return false;

        CharacterData c = cm.Get(s.characterIndex);
        return c != null && c.unlocked && c.level >= s.requiredLevel;
    }

    /// <summary>구매 가능 여부 (열렸고 아직 미구매).</summary>
    public bool CanPurchase(int index)
    {
        StoryData s = Get(index);
        return s != null && !s.purchased && IsUnlocked(index);
    }

    /// <summary>꿈코인을 소모해 이야기를 구매하고 달을 영구 강화한다.</summary>
    public bool TryPurchase(int index)
    {
        if (!CanPurchase(index)) return false;

        StoryData s = Get(index);
        if (CurrencyManager.Instance == null || !CurrencyManager.Instance.TrySpend(s.cost))
        {
            Debug.Log("꿈코인이 부족합니다. (이야기 구매 비용 " + s.cost + ")");
            return false;
        }

        s.purchased = true;
        OnStoriesChanged?.Invoke();
        Debug.Log("이야기 구매: " + s.title
                  + " → 달 데미지 +" + s.moonDamageBonus
                  + ", 주기 -" + s.moonIntervalReduction + "초");
        return true;
    }
}
