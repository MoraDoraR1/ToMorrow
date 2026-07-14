using UnityEngine;

/// <summary>
/// 보스전 전체 흐름을 지휘한다.
/// [A단계] 일반 몬스터 50마리 처치(StageManager 목표 도달) 시 보스 도전 버튼(Boss_Btn)을 활성화.
/// 이후 단계에서 보스 스폰 / 20초 타이머 / 성공·실패 처리로 확장 예정.
/// </summary>
public class BossManager : MonoBehaviour
{
    [Header("보스 도전 버튼 (Boss_Btn)")]
    public GameObject bossButton;

    void Start()
    {
        // 시작 시 보스 버튼은 숨겨둔다 (50 도달 전까지 비활성)
        if (bossButton != null)
        {
            bossButton.SetActive(false);
        }

        // 일반 몬스터 목표 처치 수 도달 시 버튼을 켠다
        if (StageManager.Instance != null)
        {
            StageManager.Instance.OnKillRequirementReached += ShowBossButton;
        }
        else
        {
            Debug.LogWarning("BossManager: 씬에 StageManager가 없습니다.");
        }
    }

    void OnDestroy()
    {
        if (StageManager.Instance != null)
        {
            StageManager.Instance.OnKillRequirementReached -= ShowBossButton;
        }
    }

    void ShowBossButton()
    {
        if (bossButton != null)
        {
            bossButton.SetActive(true);
        }
        Debug.Log("보스 도전 버튼 활성화!");
    }
}
