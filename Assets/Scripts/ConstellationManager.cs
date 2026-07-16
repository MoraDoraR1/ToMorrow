using System;
using UnityEngine;

/// <summary>
/// 별자리를 관리한다. 보스를 처치해 스테이지가 오르면 해당 별자리의 별을 구매할 수 있고,
/// 별을 전부 사서 완성하면 고유 효과가 발동하며 배경 리소스가 활성화된다.
/// </summary>
public class ConstellationManager : MonoBehaviour
{
    public static ConstellationManager Instance { get; private set; }

    [Header("별자리 목록")]
    [SerializeField] private ConstellationData[] constellations;

    // 별 구매/완성으로 상태가 바뀌었을 때 (UI 갱신용)
    public event Action OnConstellationsChanged;

    public int Count => constellations != null ? constellations.Length : 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // 시작 시엔 완성된 별자리만 배경에 보이게 맞춘다
        RefreshVisuals();
    }

    public ConstellationData Get(int index)
    {
        if (constellations != null && index >= 0 && index < constellations.Length) return constellations[index];
        return null;
    }

    /// <summary>별을 구매할 수 있는 상태인가 — 보스를 처치해 스테이지가 충분히 올랐는가.</summary>
    public bool IsUnlocked(int index)
    {
        ConstellationData c = Get(index);
        if (c == null) return false;
        if (StageManager.Instance == null) return false;
        return StageManager.Instance.CurrentStage >= c.unlockStage;
    }

    /// <summary>별 1개를 더 살 수 있는가.</summary>
    public bool CanBuyStar(int index)
    {
        ConstellationData c = Get(index);
        return c != null && !c.IsCompleted && IsUnlocked(index);
    }

    /// <summary>꿈코인을 소모해 이 별자리의 별을 1개 산다. 다 모으면 완성 처리.</summary>
    public bool TryBuyStar(int index)
    {
        if (!CanBuyStar(index)) return false;

        ConstellationData c = Get(index);
        int cost = c.NextStarCost;
        if (CurrencyManager.Instance == null || !CurrencyManager.Instance.TrySpend(cost))
        {
            Debug.Log("꿈코인이 부족합니다. (별 구매 비용 " + cost + ")");
            return false;
        }

        c.purchasedStars++;
        Debug.Log(c.constellationName + " 별 구매 (" + c.purchasedStars + "/" + c.starCount + ")");

        if (c.IsCompleted)
        {
            Debug.Log("★ " + c.constellationName + " 완성! 효과 발동 → " + DescribeEffect(c));
            RefreshVisuals();
        }

        OnConstellationsChanged?.Invoke();
        return true;
    }

    /// <summary>완성된 별자리의 배경 리소스만 켠다.</summary>
    public void RefreshVisuals()
    {
        if (constellations == null) return;
        foreach (ConstellationData c in constellations)
        {
            if (c != null && c.visualObject != null)
            {
                c.visualObject.SetActive(c.IsCompleted);
            }
        }
    }

    /// <summary>완성된 별자리 중 해당 종류의 효과 수치 합.</summary>
    private float SumEffect(ConstellationEffectType type)
    {
        float sum = 0f;
        if (constellations != null)
        {
            foreach (ConstellationData c in constellations)
            {
                if (c != null && c.IsCompleted && c.effectType == type) sum += c.effectValue;
            }
        }
        return sum;
    }

    /// <summary>클릭 데미지 증가량.</summary>
    public int TotalClickDamageBonus => Mathf.RoundToInt(SumEffect(ConstellationEffectType.ClickDamage));

    /// <summary>달 데미지 증가량.</summary>
    public int TotalMoonDamageBonus => Mathf.RoundToInt(SumEffect(ConstellationEffectType.MoonDamage));

    /// <summary>달 공격주기 감소량(초).</summary>
    public float TotalMoonIntervalReduction => SumEffect(ConstellationEffectType.MoonSpeed);

    /// <summary>꿈코인 획득 추가 비율 (0.2 = +20%).</summary>
    public float TotalCoinGainBonus => SumEffect(ConstellationEffectType.CoinGain);

    /// <summary>치명타 확률 추가분 (0.05 = +5%). 클릭·달 모두에 적용된다.</summary>
    public float TotalCritChanceBonus => SumEffect(ConstellationEffectType.CritChance);

    /// <summary>치명타 피해 배율 추가분 (0.5 = +50%p). 클릭·달 모두에 적용된다.</summary>
    public float TotalCritDamageBonus => SumEffect(ConstellationEffectType.CritDamage);

    /// <summary>UI/로그에 보여줄 효과 설명.</summary>
    public string DescribeEffect(ConstellationData c)
    {
        if (c == null) return "";
        switch (c.effectType)
        {
            case ConstellationEffectType.ClickDamage: return "클릭 데미지 +" + Mathf.RoundToInt(c.effectValue);
            case ConstellationEffectType.MoonDamage:  return "달 데미지 +" + Mathf.RoundToInt(c.effectValue);
            case ConstellationEffectType.MoonSpeed:   return "달 공격주기 -" + c.effectValue + "초";
            case ConstellationEffectType.CoinGain:    return "꿈코인 획득 +" + Mathf.RoundToInt(c.effectValue * 100f) + "%";
            case ConstellationEffectType.CritChance:  return "치명타 확률 +" + Mathf.RoundToInt(c.effectValue * 100f) + "%";
            case ConstellationEffectType.CritDamage:  return "치명타 피해 +" + Mathf.RoundToInt(c.effectValue * 100f) + "%";
        }
        return "";
    }
}
