using UnityEngine;

/// <summary>
/// 치명타 스탯을 한곳에서 계산한다. (기획서 2.7)
///
/// 클릭과 달이 서로 다른 지점에서 데미지를 적용하므로(클릭=발사 시점, 달=AutoAttack),
/// 공식을 각자 두면 반드시 어긋난다. 여기 모아둔다.
///
/// 합산 규칙
///   클릭 : 기본 + 별자리
///   달   : 기본 + 별자리 + 이야기   ← 기획서 2.7 "이야기가 달의 치명타를 강화"
///
/// 기본값은 Balance.csv 의 critChance / critMultiplier 로 조정한다.
/// </summary>
public class CombatStats : MonoBehaviour
{
    public static CombatStats Instance { get; private set; }

    [Header("기본 치명타 확률 (0.05 = 5%) — Balance.csv: critChance")]
    public float baseCritChance = 0.05f;

    [Header("기본 치명타 피해 배율 (2 = 200%) — Balance.csv: critMultiplier")]
    public float baseCritMultiplier = 2f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private static float ConstChance =>
        ConstellationManager.Instance != null ? ConstellationManager.Instance.TotalCritChanceBonus : 0f;

    private static float ConstDamage =>
        ConstellationManager.Instance != null ? ConstellationManager.Instance.TotalCritDamageBonus : 0f;

    /// <summary>클릭(콧물) 치명타 확률. 0~1 로 묶는다.</summary>
    public float ClickCritChance => Mathf.Clamp01(baseCritChance + ConstChance);

    /// <summary>클릭 치명타 피해 배율. 1 밑으로는 내려가지 않는다(치명타가 손해면 안 된다).</summary>
    public float ClickCritMultiplier => Mathf.Max(1f, baseCritMultiplier + ConstDamage);

    /// <summary>달 치명타 확률 = 기본 + 별자리 + 이야기.</summary>
    public float MoonCritChance
    {
        get
        {
            float story = StoryManager.Instance != null ? StoryManager.Instance.TotalMoonCritChanceBonus : 0f;
            return Mathf.Clamp01(baseCritChance + ConstChance + story);
        }
    }

    /// <summary>달 치명타 피해 배율 = 기본 + 별자리 + 이야기.</summary>
    public float MoonCritMultiplier
    {
        get
        {
            float story = StoryManager.Instance != null ? StoryManager.Instance.TotalMoonCritDamageBonus : 0f;
            return Mathf.Max(1f, baseCritMultiplier + ConstDamage + story);
        }
    }

    /// <summary>확률을 굴려 치명타면 배율을 곱한 데미지를 돌려준다.</summary>
    public static int Roll(int damage, float chance, float multiplier, out bool isCrit)
    {
        isCrit = Random.value < chance;
        return isCrit ? Mathf.RoundToInt(damage * multiplier) : damage;
    }

    /// <summary>클릭 데미지에 치명타를 적용한다. (CombatStats가 없으면 원래 데미지 그대로)</summary>
    public static int RollClick(int damage, out bool isCrit)
    {
        if (Instance == null) { isCrit = false; return damage; }
        return Roll(damage, Instance.ClickCritChance, Instance.ClickCritMultiplier, out isCrit);
    }

    /// <summary>달 데미지에 치명타를 적용한다. (CombatStats가 없으면 원래 데미지 그대로)</summary>
    public static int RollMoon(int damage, out bool isCrit)
    {
        if (Instance == null) { isCrit = false; return damage; }
        return Roll(damage, Instance.MoonCritChance, Instance.MoonCritMultiplier, out isCrit);
    }
}
