using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 스토리 이미지를 화면에 크게 보여주는 뷰어. (기획서 2.7)
/// - 이야기를 구매하면 즉시 열린다 (StoryManager.OnStoryPurchased 구독)
/// - 구매한 이야기는 슬롯의 '다시보기' 버튼으로 언제든 몇 번이든 다시 열 수 있다
///
/// 패널을 인스펙터에 안 물려도 로그로 동작은 확인된다.
/// </summary>
public class StoryViewerUI : MonoBehaviour
{
    public static StoryViewerUI Instance { get; private set; }

    [Header("뷰어 패널 (Story_View_Panel)")]
    public GameObject panel;

    [Header("스토리 이미지가 표시될 Image")]
    public Image storyImage;

    [Header("제목 / 본문 (선택)")]
    public Text titleText;
    public Text contentText;

    [Header("닫기 버튼")]
    public Button closeButton;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (panel != null) panel.SetActive(false);
        if (closeButton != null) closeButton.onClick.AddListener(Close);

        if (StoryManager.Instance != null)
        {
            StoryManager.Instance.OnStoryPurchased += Show;   // 구매 즉시 열기
        }
        else
        {
            Debug.LogWarning("StoryViewerUI: 씬에 StoryManager가 없습니다.");
        }
    }

    void OnDestroy()
    {
        if (StoryManager.Instance != null)
        {
            StoryManager.Instance.OnStoryPurchased -= Show;
        }
    }

    /// <summary>해당 이야기의 스토리를 크게 보여준다. 횟수 제한 없다.</summary>
    public void Show(int index)
    {
        StoryData s = StoryManager.Instance != null ? StoryManager.Instance.Get(index) : null;
        if (s == null) return;

        if (storyImage != null)
        {
            storyImage.sprite = s.storyImage;
            storyImage.enabled = s.storyImage != null;   // 이미지가 없으면 빈 사각형이 뜨지 않게
        }
        if (titleText != null) titleText.text = s.title;
        if (contentText != null) contentText.text = s.content;

        if (panel != null)
        {
            panel.SetActive(true);
        }
        else
        {
            Debug.Log("스토리 열람: " + s.title + " (Story_View_Panel이 비어 있어 로그로만 표시합니다)");
        }
    }

    public void Close()
    {
        if (panel != null) panel.SetActive(false);
    }
}
