using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 엔딩 패널. 마지막 스테이지(STAGE.12)의 보스를 처치하면 열린다.
/// '처음부터 다시시작' 버튼을 누르면 씬을 다시 불러 모든 진행도를 초기화한다.
///
/// 패널은 인스펙터에서 물려주면 되고, 비어 있어도 로그로 동작은 확인된다.
/// </summary>
public class EndingPanelUI : MonoBehaviour
{
    [Header("엔딩 패널 (Ending_Panel)")]
    public GameObject panel;

    [Header("처음부터 다시시작 버튼")]
    public Button restartButton;

    [Header("엔딩 문구 표시 Text (선택, 레거시/TMP 중 하나)")]
    public Text messageText;
    public TMP_Text messageTmpText;

    [Header("엔딩 문구")]
    [TextArea(2, 5)]
    public string message = "모든 꿈을 건넜습니다.\n내일은, 내일 할래.";

    void Start()
    {
        if (panel != null) panel.SetActive(false);

        if (restartButton != null) restartButton.onClick.AddListener(Restart);

        if (StageManager.Instance != null)
        {
            StageManager.Instance.OnAllStagesCleared += Show;
        }
        else
        {
            Debug.LogWarning("EndingPanelUI: 씬에 StageManager가 없습니다.");
        }
    }

    void OnDestroy()
    {
        if (StageManager.Instance != null)
        {
            StageManager.Instance.OnAllStagesCleared -= Show;
        }
    }

    void Show()
    {
        if (messageText != null) messageText.text = message;
        if (messageTmpText != null) messageTmpText.text = message;

        if (panel != null)
        {
            panel.SetActive(true);
        }
        else
        {
            Debug.Log("엔딩! (Ending_Panel이 인스펙터에 비어 있어 로그로만 표시합니다)");
        }
    }

    /// <summary>
    /// 씬을 다시 불러 처음(STAGE.00)부터 시작한다.
    /// 스테이지·꿈코인·캐릭터 해금/레벨·이야기·별자리가 모두 씬의 초기값으로 되돌아가고,
    /// DataTableLoader가 Awake에서 CSV 수치를 다시 주입한다.
    /// (매니저별로 일일이 되돌리는 것보다 빠뜨릴 여지가 없다)
    /// </summary>
    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
