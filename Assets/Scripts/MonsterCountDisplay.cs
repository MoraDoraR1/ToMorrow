using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// StageManager의 일반 몬스터 처치 수를 UI(Monster_Count)에 (현재/목표) 형식으로 표시한다.
/// 레거시 UI Text와 TextMeshPro 둘 다 지원.
/// </summary>
public class MonsterCountDisplay : MonoBehaviour
{
    [Header("표시할 Text (레거시 UI Text)")]
    public Text uiText;

    [Header("표시할 Text (TextMeshPro)")]
    public TMP_Text tmpText;

    [Header("표시 형식 ({0}=현재 처치 수, {1}=목표)")]
    public string format = "({0}/{1})";

    void Awake()
    {
        // Monster_Count 오브젝트에 직접 붙인 경우 참조를 자동으로 찾는다
        if (uiText == null) uiText = GetComponent<Text>();
        if (tmpText == null) tmpText = GetComponent<TMP_Text>();
    }

    void Start()
    {
        if (StageManager.Instance != null)
        {
            // 시작 시 현재 값으로 초기화한 뒤 변동을 구독
            SetText(StageManager.Instance.KillCount, StageManager.Instance.KillsRequired);
            StageManager.Instance.OnKillCountChanged += SetText;
        }
        else
        {
            Debug.LogWarning("MonsterCountDisplay: 씬에 StageManager가 없습니다.");
        }
    }

    void OnDestroy()
    {
        if (StageManager.Instance != null)
        {
            StageManager.Instance.OnKillCountChanged -= SetText;
        }
    }

    void SetText(int current, int required)
    {
        string s = string.Format(format, current, required);
        if (uiText != null) uiText.text = s;
        if (tmpText != null) tmpText.text = s;
    }
}
