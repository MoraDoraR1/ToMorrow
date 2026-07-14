using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 스테이지가 바뀔 때 배경 이미지를 해당 스테이지 테마로 교체한다.
/// BackGround 오브젝트에 붙여 사용 (Image 자동 인식).
/// </summary>
public class StageBackground : MonoBehaviour
{
    [Header("배경 Image (비우면 자기 자신의 Image)")]
    public Image backgroundImage;

    void Awake()
    {
        if (backgroundImage == null) backgroundImage = GetComponent<Image>();
    }

    void Start()
    {
        if (StageManager.Instance != null)
        {
            Apply(StageManager.Instance.CurrentStage);
            StageManager.Instance.OnStageChanged += Apply;
        }
        else
        {
            Debug.LogWarning("StageBackground: 씬에 StageManager가 없습니다.");
        }
    }

    void OnDestroy()
    {
        if (StageManager.Instance != null)
        {
            StageManager.Instance.OnStageChanged -= Apply;
        }
    }

    void Apply(int stage)
    {
        StageInfo info = StageManager.Instance != null ? StageManager.Instance.GetStageInfo(stage) : null;
        if (info != null && info.background != null && backgroundImage != null)
        {
            backgroundImage.sprite = info.background;
        }
    }
}
