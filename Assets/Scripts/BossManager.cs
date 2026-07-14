using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 보스전 전체 흐름을 지휘한다.
/// [A] 50 처치 → Boss_Btn 활성화
/// [B] 버튼 클릭 → 일반 몬스터 정지/제거 + 보스 스폰 + 타겟 전환
/// [C] 20초 타이머 → 시간 초과 시 실패(일반 복귀, 카운트 50 유지, 버튼 재노출)
/// [D] 보스 처치 성공 → 다음 스테이지 + 카운트 0 리셋 + 일반 재개
/// </summary>
public class BossManager : MonoBehaviour
{
    [Header("보스 도전 버튼 (Boss_Btn)")]
    public GameObject bossButton;

    [Header("보스 프리팹")]
    public GameObject bossPrefab;

    [Header("보스가 생성될 부모 (Canvas)")]
    public Transform spawnParent;

    [Header("스폰 위치 (선택 - 비우면 프리팹 위치 사용)")]
    public RectTransform spawnPoint;

    [Header("타겟을 갱신할 PlayerAttack")]
    public PlayerAttack playerAttack;

    [Header("일반 몬스터 스포너")]
    public MonsterSpawner monsterSpawner;

    [Header("제한 시간 (초)")]
    public float bossDuration = 20f;

    [Header("남은 시간 표시 Text (선택, 레거시/TMP 중 하나)")]
    public Text timerText;
    public TMP_Text timerTmpText;

    private EnemyHealth currentBoss;
    private bool bossActive = false;
    private float timeLeft;

    void Start()
    {
        // 시작 시 버튼/타이머 숨김
        if (bossButton != null) bossButton.SetActive(false);
        ShowTimer(false);

        if (StageManager.Instance != null)
        {
            StageManager.Instance.OnKillRequirementReached += ShowBossButton;
        }
        else
        {
            Debug.LogWarning("BossManager: 씬에 StageManager가 없습니다.");
        }

        // 버튼 클릭 → 보스전 시작 (Inspector onClick 배선 불필요)
        if (bossButton != null)
        {
            Button btn = bossButton.GetComponent<Button>();
            if (btn != null) btn.onClick.AddListener(StartBossBattle);
        }
    }

    void OnDestroy()
    {
        if (StageManager.Instance != null)
        {
            StageManager.Instance.OnKillRequirementReached -= ShowBossButton;
        }
    }

    void Update()
    {
        if (!bossActive) return;

        timeLeft -= Time.deltaTime;
        if (timeLeft < 0f) timeLeft = 0f;
        UpdateTimerText();

        if (timeLeft <= 0f)
        {
            BossFailed();
        }
    }

    // [A] 목표 도달 시 버튼 노출 (보스전 중이 아닐 때만)
    void ShowBossButton()
    {
        if (!bossActive && bossButton != null) bossButton.SetActive(true);
    }

    // [B] 보스전 시작
    public void StartBossBattle()
    {
        if (bossActive || bossPrefab == null || spawnParent == null) return;
        bossActive = true;

        if (bossButton != null) bossButton.SetActive(false);

        // 일반 몬스터 정지 + 제거
        if (monsterSpawner != null) monsterSpawner.StopAndClear();

        // 보스 스폰
        GameObject bossObj = Instantiate(bossPrefab, spawnParent);
        RectTransform bossRect = bossObj.GetComponent<RectTransform>();
        if (bossRect != null && spawnPoint != null) bossRect.position = spawnPoint.position;

        currentBoss = bossObj.GetComponent<EnemyHealth>();
        if (currentBoss != null)
        {
            // 현재 스테이지의 보스 이미지/스탯 적용
            StageInfo info = StageManager.Instance != null ? StageManager.Instance.CurrentStageInfo : null;
            if (info != null && info.boss != null)
            {
                currentBoss.Initialize(info.boss.hp, info.boss.coinReward, info.boss.sprite);
            }

            currentBoss.OnDeath += HandleBossDefeated;
        }

        // 공격 타겟을 보스로
        if (playerAttack != null && bossRect != null) playerAttack.SetTarget(bossRect);

        // [C] 타이머 시작
        timeLeft = bossDuration;
        ShowTimer(true);
        UpdateTimerText();

        Debug.Log("보스전 시작! 제한시간 " + bossDuration + "초");
    }

    // [D] 보스 처치 성공
    void HandleBossDefeated(EnemyHealth boss)
    {
        boss.OnDeath -= HandleBossDefeated;
        currentBoss = null; // 보스는 EnemyHealth.Die()에서 스스로 파괴됨
        EndBossPhase();

        if (StageManager.Instance != null) StageManager.Instance.AdvanceStage();
        if (monsterSpawner != null) monsterSpawner.ResumeSpawning();

        Debug.Log("보스 처치 성공! 다음 스테이지로 이동.");
    }

    // [C] 시간 초과 실패
    void BossFailed()
    {
        // 보스 제거
        if (currentBoss != null)
        {
            currentBoss.OnDeath -= HandleBossDefeated;
            currentBoss.gameObject.SetActive(false); // UI 잔상 방지
            Destroy(currentBoss.gameObject);
            currentBoss = null;
        }

        EndBossPhase();

        // 일반 페이즈 복귀 (카운트 50 유지 → 재도전 버튼 노출)
        if (monsterSpawner != null) monsterSpawner.ResumeSpawning();
        if (bossButton != null) bossButton.SetActive(true);

        Debug.Log("보스전 실패 (시간 초과). 재도전 가능.");
    }

    void EndBossPhase()
    {
        bossActive = false;
        ShowTimer(false);
    }

    void ShowTimer(bool show)
    {
        if (timerText != null) timerText.gameObject.SetActive(show);
        if (timerTmpText != null) timerTmpText.gameObject.SetActive(show);
    }

    void UpdateTimerText()
    {
        string s = Mathf.CeilToInt(timeLeft).ToString();
        if (timerText != null) timerText.text = s;
        if (timerTmpText != null) timerTmpText.text = s;
    }
}
