using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// CurrencyManager의 '꿈 코인' 보유량을 UI(Money_Text)에 표시한다.
/// 재화 변동 이벤트(OnDreamCoinChanged)를 구독하여 값이 바뀔 때만 갱신한다.
/// 레거시 UI Text와 TextMeshPro 둘 다 지원 — 연결(또는 같은 오브젝트에 존재)된 쪽을 사용.
/// </summary>
public class DreamCoinDisplay : MonoBehaviour
{
    [Header("표시할 Text (레거시 UI Text)")]
    public Text uiText;

    [Header("표시할 Text (TextMeshPro)")]
    public TMP_Text tmpText;

    [Header("표시 형식 ({0} 자리에 꿈 코인 수)")]
    public string format = "{0}";

    void Awake()
    {
        // 이 스크립트를 Money_Text 오브젝트에 직접 붙인 경우, 참조를 자동으로 찾는다
        if (uiText == null) uiText = GetComponent<Text>();
        if (tmpText == null) tmpText = GetComponent<TMP_Text>();
    }

    void Start()
    {
        if (CurrencyManager.Instance != null)
        {
            // 시작 시 현재 보유량으로 초기화한 뒤, 이후 변동을 구독
            SetText(CurrencyManager.Instance.DreamCoin);
            CurrencyManager.Instance.OnDreamCoinChanged += SetText;
        }
        else
        {
            Debug.LogWarning("DreamCoinDisplay: 씬에 CurrencyManager가 없습니다.");
        }
    }

    void OnDestroy()
    {
        // 파괴 시 구독 해제 (이벤트 누수 방지)
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnDreamCoinChanged -= SetText;
        }
    }

    void SetText(int amount)
    {
        string s = string.Format(format, amount);
        if (uiText != null) uiText.text = s;
        if (tmpText != null) tmpText.text = s;
    }
}
