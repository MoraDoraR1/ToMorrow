using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 이야기 1개를 표시하는 슬롯. StoryPanelUI가 프리팹으로 생성한 뒤 Bind(index)로 연결한다.
/// 본문은 구매 전까지 가려지고, 구매하면 공개된다.
/// </summary>
public class StorySlotUI : MonoBehaviour
{
    [Header("표시 요소")]
    public Text titleText;
    public Text contentText;   // 본문 (구매 전에는 가려짐)
    public Text infoText;      // 상태 / 효과
    public Text buttonLabel;
    public Button actionButton;

    [Header("구매 전 본문 대신 보여줄 문구")]
    public string hiddenContent = "???";

    private int index = -1;

    public void Bind(int storyIndex)
    {
        index = storyIndex;

        if (actionButton != null)
        {
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(OnClickAction);
        }
        Refresh();
    }

    public void Refresh()
    {
        StoryManager sm = StoryManager.Instance;
        if (sm == null) return;

        StoryData s = sm.Get(index);
        if (s == null)
        {
            gameObject.SetActive(false);
            return;
        }

        int coin = CurrencyManager.Instance != null ? CurrencyManager.Instance.DreamCoin : 0;
        string effect = "달 데미지 +" + s.moonDamageBonus + " · 주기 -" + s.moonIntervalReduction + "초";

        if (titleText != null) titleText.text = s.title;

        if (s.purchased)
        {
            if (contentText != null) contentText.text = s.content;   // 구매 후 본문 공개
            if (infoText != null) infoText.text = "구매 완료 · " + effect;
            if (buttonLabel != null) buttonLabel.text = "구매됨";
            if (actionButton != null) actionButton.interactable = false;
        }
        else if (!sm.IsUnlocked(index))
        {
            if (contentText != null) contentText.text = hiddenContent;
            if (infoText != null) infoText.text = "캐릭터 LV " + s.requiredLevel + " 필요";
            if (buttonLabel != null) buttonLabel.text = "잠김";
            if (actionButton != null) actionButton.interactable = false;
        }
        else
        {
            if (contentText != null) contentText.text = hiddenContent;
            if (infoText != null) infoText.text = effect;
            if (buttonLabel != null) buttonLabel.text = "구매 (" + s.cost + ")";
            if (actionButton != null) actionButton.interactable = coin >= s.cost;
        }
    }

    void OnClickAction()
    {
        // 성공하면 StoryManager가 OnStoriesChanged를 쏘고, 패널이 전체를 갱신한다
        if (StoryManager.Instance != null) StoryManager.Instance.TryPurchase(index);
    }
}
