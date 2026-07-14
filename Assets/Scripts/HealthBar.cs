using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// EnemyHealth의 체력을 Fill 이미지의 fillAmount(0~1)로 표시하는 HP바.
/// 몬스터/보스 프리팹 안에 넣어 사용 — 대상은 부모의 EnemyHealth에서 자동으로 찾는다.
/// </summary>
public class HealthBar : MonoBehaviour
{
    [Header("채워지는 Fill 이미지 (Image Type = Filled)")]
    public Image fillImage;

    [Header("대상 EnemyHealth (비우면 부모에서 자동 탐색)")]
    public EnemyHealth target;

    void Awake()
    {
        // 프리팹 안 계층에서 EnemyHealth를 자동으로 찾는다
        if (target == null) target = GetComponentInParent<EnemyHealth>();
    }

    void Start()
    {
        if (target != null)
        {
            // 시작 시 현재 체력으로 초기화한 뒤 변동을 구독
            UpdateBar(target.CurrentHp, target.MaxHp);
            target.OnHealthChanged += UpdateBar;
        }
        else
        {
            Debug.LogWarning("HealthBar: 대상 EnemyHealth를 찾지 못했습니다.");
        }
    }

    void OnDestroy()
    {
        if (target != null) target.OnHealthChanged -= UpdateBar;
    }

    void UpdateBar(int current, int max)
    {
        if (fillImage != null && max > 0)
        {
            fillImage.fillAmount = (float)current / max;
        }
    }
}
