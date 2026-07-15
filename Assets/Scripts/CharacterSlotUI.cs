using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 캐릭터 1명을 표시하는 슬롯. CharacterPanelUI가 프리팹으로 생성한 뒤 Bind(index)로 연결한다.
/// 버튼은 상태에 따라 '해금' 또는 '강화'로 동작한다.
/// </summary>
public class CharacterSlotUI : MonoBehaviour
{
    [Header("표시 요소")]
    public Image portrait;
    public Text nameText;
    public Text infoText;      // LV / 데미지, 또는 잠김 안내
    public Text buttonLabel;   // 해금(100) / 강화(60) / MAX
    public Button actionButton;

    [Header("미해금 상태의 초상화 색")]
    public Color lockedColor = new Color(0.25f, 0.25f, 0.25f, 1f);

    private int index = -1;

    /// <summary>이 슬롯이 담당할 캐릭터 번호를 지정한다.</summary>
    public void Bind(int characterIndex)
    {
        index = characterIndex;

        if (actionButton != null)
        {
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(OnClickAction);
        }
        Refresh();
    }

    /// <summary>현재 캐릭터 상태와 보유 코인에 맞춰 표시를 갱신한다.</summary>
    public void Refresh()
    {
        CharacterManager cm = CharacterManager.Instance;
        if (cm == null) return;

        CharacterData c = cm.Get(index);
        if (c == null)
        {
            gameObject.SetActive(false);
            return;
        }

        int coin = CurrencyManager.Instance != null ? CurrencyManager.Instance.DreamCoin : 0;

        if (portrait != null)
        {
            if (c.sprite != null) portrait.sprite = c.sprite;
            portrait.color = c.unlocked ? Color.white : lockedColor;
        }
        if (nameText != null) nameText.text = c.characterName;

        if (!c.unlocked)
        {
            // 미해금 — 선형 해금이라 앞 캐릭터가 잠겨 있으면 아예 못 누름
            bool can = cm.CanUnlock(index);
            if (infoText != null) infoText.text = can ? "미해금" : "이전 캐릭터를 먼저 해금하세요";
            if (buttonLabel != null) buttonLabel.text = "해금 (" + c.unlockCost + ")";
            if (actionButton != null) actionButton.interactable = can && coin >= c.unlockCost;
        }
        else if (c.level >= c.maxLevel)
        {
            if (infoText != null) infoText.text = "LV " + c.level + " · 데미지 " + c.CurrentDamage;
            if (buttonLabel != null) buttonLabel.text = "MAX";
            if (actionButton != null) actionButton.interactable = false;
        }
        else
        {
            int cost = cm.GetLevelUpCost(index);
            if (infoText != null) infoText.text = "LV " + c.level + " · 데미지 " + c.CurrentDamage;
            if (buttonLabel != null) buttonLabel.text = "강화 (" + cost + ")";
            if (actionButton != null) actionButton.interactable = coin >= cost;
        }
    }

    void OnClickAction()
    {
        CharacterManager cm = CharacterManager.Instance;
        if (cm == null) return;

        CharacterData c = cm.Get(index);
        if (c == null) return;

        // 성공하면 CharacterManager가 OnCharactersChanged를 쏘고, 패널이 전체를 갱신한다
        if (!c.unlocked) cm.TryUnlock(index);
        else cm.TryLevelUp(index);
    }
}
