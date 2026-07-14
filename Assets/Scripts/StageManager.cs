using System;
using UnityEngine;

/// <summary>
/// 스테이지 진행을 관리한다. (STAGE.00 ~ STAGE.12)
/// 지금 단계에서는 일반 몬스터 처치 수를 세고, 목표(50) 도달을 알린다.
/// 이후 보스 도전/타이머/스테이지 이동으로 확장 예정.
/// </summary>
public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    [Header("현재 스테이지 (STAGE.00 ~ STAGE.12)")]
    [SerializeField] private int currentStage = 0;

    [Header("스테이지 이름 (인덱스 = 스테이지 번호, 0번=STAGE.00)")]
    [SerializeField] private string[] stageNames;

    [Header("보스 도전까지 필요한 일반 몬스터 처치 수")]
    [SerializeField] private int killsRequired = 50;

    [Header("현재 처치 수")]
    [SerializeField] private int killCount = 0;

    public int CurrentStage => currentStage;
    public int KillsRequired => killsRequired;
    public int KillCount => killCount;
    public string CurrentStageName => GetStageName(currentStage);

    /// <summary>스테이지 번호에 해당하는 이름을 돌려준다. 지정 안 됐으면 "STAGE.00" 형식 기본값.</summary>
    public string GetStageName(int stage)
    {
        if (stageNames != null && stage >= 0 && stage < stageNames.Length
            && !string.IsNullOrEmpty(stageNames[stage]))
        {
            return stageNames[stage];
        }
        return "STAGE." + stage.ToString("00");
    }

    // (현재 처치 수, 필요 처치 수) — UI 표시용
    public event Action<int, int> OnKillCountChanged;
    // 목표 처치 수 도달 (보스 도전 버튼 활성화 등에 사용)
    public event Action OnKillRequirementReached;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // 스테이지가 바뀌었을 때 (테마 변경 등에 사용)
    public event Action<int> OnStageChanged;

    /// <summary>다음 스테이지로 진행하고 처치 수를 0으로 리셋한다. (보스 처치 성공 시)</summary>
    public void AdvanceStage()
    {
        currentStage++;
        killCount = 0;
        OnKillCountChanged?.Invoke(killCount, killsRequired);
        OnStageChanged?.Invoke(currentStage);
        Debug.Log("스테이지 이동: STAGE." + currentStage.ToString("00"));
    }

    /// <summary>일반 몬스터 처치를 1 등록한다.</summary>
    public void RegisterKill()
    {
        // 목표에 이미 도달했으면 더 세지 않는다 (보스 도전 대기 상태)
        if (killCount >= killsRequired) return;

        killCount++;
        OnKillCountChanged?.Invoke(killCount, killsRequired);

        if (killCount >= killsRequired)
        {
            Debug.Log("보스 도전 가능! (일반 몬스터 " + killsRequired + "마리 처치 완료)");
            OnKillRequirementReached?.Invoke();
        }
    }
}
