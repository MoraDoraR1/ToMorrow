using UnityEngine;

/// <summary>
/// 캐릭터 이야기 1개. 해당 캐릭터가 requiredLevel에 도달하면 열리고,
/// 꿈코인으로 구매하면 달(보름달) 자동공격이 영구 강화된다.
/// </summary>
[System.Serializable]
public class StoryData
{
    [Header("어느 캐릭터의 이야기인가 (CharacterManager의 캐릭터 번호)")]
    public int characterIndex;

    [Header("제목 / 본문")]
    public string title;

    [TextArea(3, 8)]
    public string content;

    [Header("스토리 이미지 (구매 즉시 크게 표시되고, 다시보기로 언제든 재열람)")]
    public Sprite storyImage;

    [Header("열리는 데 필요한 캐릭터 레벨")]
    public int requiredLevel = 150;

    [Header("구매 비용 (꿈코인)")]
    public int cost = 1000;

    [Header("달 영구 강화 — 데미지 증가량")]
    public int moonDamageBonus = 10;

    [Header("달 영구 강화 — 공격 주기 감소량(초, 작을수록 빨라짐)")]
    public float moonIntervalReduction = 0.05f;

    [Header("--- 런타임 상태 ---")]
    public bool purchased = false;
}
