using UnityEngine;

/// <summary>
/// 몬스터/보스 한 종류의 데이터 (이미지 + 스탯).
/// </summary>
[System.Serializable]
public class MonsterVariant
{
    [Header("구분용 이름 (선택, 인스펙터 표시용)")]
    public string label;

    public Sprite sprite;
    public int hp = 100;
    public int coinReward = 1;
}

/// <summary>
/// 스테이지 1개의 테마 데이터. 배경 + 등장 몬스터 종류들 + 보스.
/// </summary>
[System.Serializable]
public class StageInfo
{
    [Header("스테이지 이름 (예: STAGE.00 - 낮잠 숲)")]
    public string stageName;

    [Header("배경 스프라이트")]
    public Sprite background;

    [Header("이 스테이지에 등장하는 몬스터 종류 (랜덤으로 하나씩 등장)")]
    public MonsterVariant[] monsters;

    [Header("이 스테이지의 보스")]
    public MonsterVariant boss;
}
