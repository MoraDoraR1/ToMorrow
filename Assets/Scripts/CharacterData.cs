using UnityEngine;

/// <summary>
/// 캐릭터 1명의 데이터 + 런타임 상태(해금 여부/레벨).
/// 캐릭터는 클릭(콧물) 데미지에 기여한다.
/// </summary>
[System.Serializable]
public class CharacterData
{
    [Header("이름 / 이미지")]
    public string characterName;
    public Sprite sprite;

    [Header("해금 비용 (꿈코인)")]
    public int unlockCost = 100;

    [Header("클릭 데미지 기여 (LV1 기준)")]
    public int baseDamage = 10;

    [Header("레벨당 데미지 증가량")]
    public int damagePerLevel = 2;

    [Header("최대 레벨")]
    public int maxLevel = 200;

    [Header("레벨업 비용 (LV1 기준)")]
    public int levelUpBaseCost = 50;

    [Header("레벨업 비용 증가량 (레벨당)")]
    public int levelUpCostPerLevel = 10;

    [Header("--- 런타임 상태 (메인 캐릭터만 unlocked 체크) ---")]
    public bool unlocked = false;
    public int level = 1;

    /// <summary>이 캐릭터가 현재 기여하는 클릭 데미지.</summary>
    public int CurrentDamage => baseDamage + (level - 1) * damagePerLevel;
}
