using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 껐다 켜는 동안 흐른 시간만큼 '꿈 코인'을 지급한다. (자는 동안 달이 대신 잡아준 몫)
///
/// 지급 규칙
///  · 재화만 준다 — 스테이지 진행이나 처치 수(0/50)는 건드리지 않는다.
///  · 스테이지에 비례한다 — 현재 스테이지 일반 몬스터의 평균 보상을 기준으로 삼는다.
///    (몬스터 '보상 수치'를 쓰는 것이지 '몇 마리 잡았는지'와는 무관하다. 오직 시간에만 비례)
///  · 상한이 있다 — 오래 꺼둬도 maxHours 이상은 쌓이지 않는다.
///
/// 초당 지급 = 현재 스테이지 몬스터 평균 보상 x rate
/// 수치는 Balance.csv 의 offlineRate / offlineMaxHours 로 조정한다.
///
/// SaveManager 가 Awake 에서 경과 시간을 계산해두므로 여기서는 Start 에서 읽어 쓴다.
/// </summary>
public class OfflineReward : MonoBehaviour
{
    [Header("보상 안내 패널 (선택 — 비우면 로그로만 표시)")]
    public GameObject panel;

    [Header("안내 문구 Text (선택, 레거시/TMP 중 하나)")]
    public Text messageText;
    public TMP_Text messageTmpText;

    [Header("닫기 버튼 (선택)")]
    public Button closeButton;

    [Header("초당 지급 배율 (Balance.csv: offlineRate)")]
    public float rate = 0.05f;

    [Header("최대 인정 시간 (Balance.csv: offlineMaxHours)")]
    public float maxHours = 8f;

    [Header("이 시간(분) 미만이면 지급하지 않는다")]
    public float minMinutes = 1f;

    [Header("문구 형식 ({0}=시간, {1}=코인)")]
    [TextArea(2, 4)]
    public string format = "자는 동안 {0}\n꿈코인 {1}개를 모았습니다.";

    void Start()
    {
        if (panel != null) panel.SetActive(false);
        if (closeButton != null) closeButton.onClick.AddListener(Close);

        if (SaveManager.Instance == null)
        {
            Debug.LogWarning("OfflineReward: 씬에 SaveManager가 없습니다.");
            return;
        }

        double sec = SaveManager.Instance.OfflineSeconds;
        if (sec < minMinutes * 60.0) return;                 // 잠깐 껐다 켠 건 무시

        double capped = Math.Min(sec, maxHours * 3600.0);    // 상한
        int coin = Compute(capped);
        if (coin <= 0) return;

        if (CurrencyManager.Instance != null) CurrencyManager.Instance.AddDreamCoin(coin);
        Show(capped, coin);
    }

    /// <summary>현재 스테이지 몬스터 평균 보상 x rate x 초. (처치 횟수와 무관, 시간에만 비례)</summary>
    int Compute(double seconds)
    {
        StageManager sm = StageManager.Instance;
        if (sm == null) return 0;

        StageInfo info = sm.CurrentStageInfo;
        if (info == null || info.monsters == null || info.monsters.Length == 0) return 0;

        float sum = 0f;
        int n = 0;
        foreach (MonsterVariant m in info.monsters)
        {
            if (m == null) continue;
            sum += m.coinReward;
            n++;
        }
        if (n == 0) return 0;

        double perSec = (sum / n) * rate;
        double total = perSec * seconds;

        // int 범위를 넘지 않게 자른다 (후반 스테이지는 보상이 크다)
        return (int)Math.Min(total, int.MaxValue);
    }

    void Show(double seconds, int coin)
    {
        string t = Describe(seconds);
        string s = string.Format(format, t, coin.ToString("N0"));

        if (messageText != null) messageText.text = s;
        if (messageTmpText != null) messageTmpText.text = s;

        if (panel != null)
        {
            panel.SetActive(true);
        }
        else
        {
            Debug.Log("오프라인 보상: " + t + " → 꿈코인 +" + coin
                      + " (안내 패널이 비어 있어 로그로만 표시합니다)");
        }
    }

    static string Describe(double seconds)
    {
        int m = Mathf.FloorToInt((float)seconds / 60f);
        if (m < 60) return m + "분";
        return (m / 60) + "시간 " + (m % 60) + "분";
    }

    public void Close()
    {
        if (panel != null) panel.SetActive(false);
    }
}
