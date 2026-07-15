using UnityEngine;

/// <summary>별자리 완성 시 발동하는 효과의 종류. (CSV에는 아래 숫자로 적는다)</summary>
public enum ConstellationEffectType
{
    ClickDamage = 0,   // 클릭(콧물) 데미지 +value
    MoonDamage  = 1,   // 달 자동공격 데미지 +value
    MoonSpeed   = 2,   // 달 자동공격 주기 -value 초 (빨라짐)
    CoinGain    = 3,   // 꿈코인 획득량 +value 비율 (0.2 = +20%)
}

/// <summary>
/// 별자리 1개. 보스를 처치하면 이 별자리의 별들이 구매 가능해지고,
/// 별을 하나씩 사서 전부(starCount) 모으면 완성되어 효과가 발동하고 배경에 배치된다.
/// (별은 재화가 아니라 '구매 대상'이다)
/// </summary>
[System.Serializable]
public class ConstellationData
{
    [Header("별자리 이름 (예: 사수자리)")]
    public string constellationName;

    [Header("상점에서 보여줄 별자리 이미지")]
    public Sprite previewSprite;

    [Header("구성 별 개수 (예: 7)")]
    public int starCount = 7;

    [Header("해금 조건 — 현재 스테이지가 이 값 이상이면 별 구매 가능")]
    public int unlockStage = 1;

    [Header("별 1개 구매 비용 (첫 별 기준)")]
    public int starBaseCost = 200;

    [Header("별을 살 때마다 늘어나는 비용")]
    public int starCostIncrease = 50;

    [Header("완성 시 발동할 효과")]
    public ConstellationEffectType effectType = ConstellationEffectType.ClickDamage;

    [Header("효과 수치 (MoonSpeed는 초, CoinGain은 비율 0.2=+20%)")]
    public float effectValue = 10f;

    [Header("완성 시 활성화할 배경 오브젝트 (Star_Shinny 안의 별자리 리소스)")]
    public GameObject visualObject;

    [Header("--- 런타임 상태 ---")]
    public int purchasedStars = 0;

    public bool IsCompleted => purchasedStars >= starCount;

    /// <summary>다음에 살 별 1개의 가격.</summary>
    public int NextStarCost => starBaseCost + purchasedStars * starCostIncrease;
}
