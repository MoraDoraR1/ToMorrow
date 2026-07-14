using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// StageManager의 현재 스테이지를 UI(Stage_text)에 표시한다.
/// 스테이지 변경(OnStageChanged) 시 자동 갱신. 레거시 Text / TextMeshPro 둘 다 지원.
/// </summary>
public class StageDisplay : MonoBehaviour
{
    [Header("표시할 Text (레거시 UI Text)")]
    public Text uiText;

    [Header("표시할 Text (TextMeshPro)")]
    public TMP_Text tmpText;

    [Header("표시 형식 ({0}=스테이지 이름)")]
    public string format = "{0}";

    void Awake()
    {
        if (uiText == null) uiText = GetComponent<Text>();
        if (tmpText == null) tmpText = GetComponent<TMP_Text>();
    }

    void Start()
    {
        if (StageManager.Instance != null)
        {
            // 시작 시 현재 스테이지로 초기화한 뒤 변경을 구독
            SetText(StageManager.Instance.CurrentStage);
            StageManager.Instance.OnStageChanged += SetText;
        }
        else
        {
            Debug.LogWarning("StageDisplay: 씬에 StageManager가 없습니다.");
        }
    }

    void OnDestroy()
    {
        if (StageManager.Instance != null)
        {
            StageManager.Instance.OnStageChanged -= SetText;
        }
    }

    void SetText(int stage)
    {
        // 스테이지 번호 → 이름 조회 (StageManager에 지정된 이름 또는 기본값)
        string name = StageManager.Instance != null
            ? StageManager.Instance.GetStageName(stage)
            : stage.ToString();

        string s = string.Format(format, name);
        if (uiText != null) uiText.text = s;
        if (tmpText != null) tmpText.text = s;
    }
}
