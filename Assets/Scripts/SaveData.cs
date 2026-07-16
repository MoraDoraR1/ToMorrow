using System;

/// <summary>
/// 저장되는 진행도 전체. JsonUtility로 직렬화해 PlayerPrefs에 넣는다.
/// (JsonUtility는 public 필드만 직렬화하므로 프로퍼티를 쓰지 않는다)
/// </summary>
[Serializable]
public class SaveData
{
    /// <summary>저장 형식 버전. 필드가 바뀌면 올려서 옛 저장본을 걸러낸다.</summary>
    public int version = 1;

    // 진행
    public int stage;
    public int killCount;
    public int dreamCoin;

    // 캐릭터 (인덱스 = 캐릭터 번호)
    public bool[] charUnlocked;
    public int[] charLevel;

    // 이야기 (인덱스 = 이야기 번호)
    public bool[] storyPurchased;

    // 별자리 (인덱스 = 별자리 번호)
    public int[] constStars;

    /// <summary>마지막 저장 시각 (UTC ticks). 오프라인 보상 계산에 쓸 수 있다.</summary>
    public long lastSaveTicks;
}
