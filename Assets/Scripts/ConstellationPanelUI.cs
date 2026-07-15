using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 별자리 상점 패널. 한 번에 별자리 1개를 크게 보여주고,
/// ← / → 화살표 버튼으로 옆 별자리로 넘긴다. (스크롤 목록 아님)
/// Star_Btn으로 패널을 열고 닫는다.
/// </summary>
public class ConstellationPanelUI : MonoBehaviour
{
    [Header("열고 닫을 패널 (Star_Panel)")]
    public GameObject panel;

    [Header("패널을 여는 버튼 (Star_Btn)")]
    public Button openButton;

    [Header("닫기 버튼 (선택)")]
    public Button closeButton;

    [Header("좌우 이동 버튼")]
    public Button prevButton;
    public Button nextButton;

    [Header("표시 요소 (한 번에 별자리 1개)")]
    public Image previewImage;   // 별자리 그림
    public Text nameText;        // 사수자리
    public Text pageText;        // 3 / 12
    public Text progressText;    // 별 3 / 7
    public Text effectText;      // 완성 효과 설명
    public Text buyLabel;        // 별 구매 (200) / 완성 / 잠김
    public Button buyButton;

    [Header("잠긴 별자리의 그림 색")]
    public Color lockedColor = new Color(0.25f, 0.25f, 0.25f, 1f);

    private int current = 0;

    void Start()
    {
        if (panel != null) panel.SetActive(false);

        if (openButton != null) openButton.onClick.AddListener(Toggle);
        if (closeButton != null) closeButton.onClick.AddListener(Close);
        if (prevButton != null) prevButton.onClick.AddListener(Prev);
        if (nextButton != null) nextButton.onClick.AddListener(Next);
        if (buyButton != null) buyButton.onClick.AddListener(OnClickBuy);

        if (ConstellationManager.Instance != null)
        {
            ConstellationManager.Instance.OnConstellationsChanged += Refresh;
        }
        // 보스를 잡아 스테이지가 오르면 별자리가 해금됨
        if (StageManager.Instance != null)
        {
            StageManager.Instance.OnStageChanged += HandleStageChanged;
        }
        // 코인이 변하면 구매 버튼 활성 여부가 달라짐
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnDreamCoinChanged += HandleCoinChanged;
        }

        Refresh();
    }

    void OnDestroy()
    {
        if (ConstellationManager.Instance != null)
        {
            ConstellationManager.Instance.OnConstellationsChanged -= Refresh;
        }
        if (StageManager.Instance != null)
        {
            StageManager.Instance.OnStageChanged -= HandleStageChanged;
        }
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnDreamCoinChanged -= HandleCoinChanged;
        }
    }

    void HandleStageChanged(int _) => Refresh();
    void HandleCoinChanged(int _) => Refresh();

    /// <summary>이전 별자리로 (처음에서 누르면 마지막으로 순환).</summary>
    public void Prev()
    {
        int n = Count;
        if (n <= 0) return;
        current = (current - 1 + n) % n;
        Refresh();
    }

    /// <summary>다음 별자리로 (마지막에서 누르면 처음으로 순환).</summary>
    public void Next()
    {
        int n = Count;
        if (n <= 0) return;
        current = (current + 1) % n;
        Refresh();
    }

    private int Count => ConstellationManager.Instance != null ? ConstellationManager.Instance.Count : 0;

    void Refresh()
    {
        ConstellationManager cm = ConstellationManager.Instance;
        if (cm == null) return;

        int n = cm.Count;
        if (n <= 0) return;
        current = Mathf.Clamp(current, 0, n - 1);

        ConstellationData c = cm.Get(current);
        if (c == null) return;

        bool unlocked = cm.IsUnlocked(current);
        int coin = CurrencyManager.Instance != null ? CurrencyManager.Instance.DreamCoin : 0;

        if (nameText != null) nameText.text = c.constellationName;
        if (pageText != null) pageText.text = (current + 1) + " / " + n;
        if (effectText != null) effectText.text = cm.DescribeEffect(c);

        if (previewImage != null)
        {
            previewImage.sprite = c.previewSprite;
            previewImage.enabled = c.previewSprite != null;
            // 완성 전에는 어둡게, 완성하면 밝게
            previewImage.color = c.IsCompleted ? Color.white : lockedColor;
        }

        if (c.IsCompleted)
        {
            if (progressText != null) progressText.text = "별 " + c.starCount + " / " + c.starCount;
            if (buyLabel != null) buyLabel.text = "완성";
            if (buyButton != null) buyButton.interactable = false;
        }
        else if (!unlocked)
        {
            if (progressText != null) progressText.text = "STAGE." + c.unlockStage.ToString("00") + " 보스 처치 필요";
            if (buyLabel != null) buyLabel.text = "잠김";
            if (buyButton != null) buyButton.interactable = false;
        }
        else
        {
            int cost = c.NextStarCost;
            if (progressText != null) progressText.text = "별 " + c.purchasedStars + " / " + c.starCount;
            if (buyLabel != null) buyLabel.text = "별 구매 (" + cost + ")";
            if (buyButton != null) buyButton.interactable = coin >= cost;
        }
    }

    void OnClickBuy()
    {
        // 성공하면 OnConstellationsChanged로 Refresh가 다시 불린다
        if (ConstellationManager.Instance != null) ConstellationManager.Instance.TryBuyStar(current);
    }

    public void Toggle()
    {
        if (panel == null) return;
        bool open = !panel.activeSelf;
        panel.SetActive(open);
        if (open) Refresh();
    }

    public void Close()
    {
        if (panel != null) panel.SetActive(false);
    }
}
