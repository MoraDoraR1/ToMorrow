using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 캐릭터 패널. CharacterManager의 캐릭터 수만큼 슬롯 프리팹을 자동 생성하고,
/// 해금/강화나 꿈코인 변동이 있을 때 표시를 갱신한다.
/// Char_Btn으로 패널을 열고 닫는다.
/// </summary>
public class CharacterPanelUI : MonoBehaviour
{
    [Header("열고 닫을 패널 (Char_Panel)")]
    public GameObject panel;

    [Header("패널을 여는 버튼 (Char_Btn)")]
    public Button openButton;

    [Header("닫기 버튼 (선택)")]
    public Button closeButton;

    [Header("슬롯 프리팹 (CharacterSlotUI 포함)")]
    public GameObject slotPrefab;

    [Header("슬롯이 담길 컨테이너 (Vertical Layout Group 권장)")]
    public Transform slotContainer;

    private readonly List<CharacterSlotUI> slots = new List<CharacterSlotUI>();

    void Start()
    {
        // 슬롯을 먼저 만든 뒤 패널을 닫는다
        BuildSlots();
        if (panel != null) panel.SetActive(false);

        if (openButton != null) openButton.onClick.AddListener(Toggle);
        if (closeButton != null) closeButton.onClick.AddListener(Close);

        if (CharacterManager.Instance != null)
        {
            CharacterManager.Instance.OnCharactersChanged += RefreshAll;
        }
        // 코인이 변하면 버튼 활성/비활성(구매 가능 여부)이 달라지므로 같이 갱신
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnDreamCoinChanged += HandleCoinChanged;
        }
    }

    void OnDestroy()
    {
        if (CharacterManager.Instance != null)
        {
            CharacterManager.Instance.OnCharactersChanged -= RefreshAll;
        }
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnDreamCoinChanged -= HandleCoinChanged;
        }
    }

    void BuildSlots()
    {
        if (slotPrefab == null || slotContainer == null)
        {
            Debug.LogWarning("CharacterPanelUI: 슬롯 프리팹 또는 컨테이너가 비어 있습니다.");
            return;
        }
        if (CharacterManager.Instance == null)
        {
            Debug.LogWarning("CharacterPanelUI: 씬에 CharacterManager가 없습니다.");
            return;
        }

        int count = CharacterManager.Instance.Count;
        for (int i = 0; i < count; i++)
        {
            GameObject go = Instantiate(slotPrefab, slotContainer);
            CharacterSlotUI slot = go.GetComponent<CharacterSlotUI>();
            if (slot == null)
            {
                Debug.LogError("CharacterPanelUI: 슬롯 프리팹에 CharacterSlotUI가 없습니다.");
                Destroy(go);
                continue;
            }
            slot.Bind(i);
            slots.Add(slot);
        }
    }

    void HandleCoinChanged(int _) => RefreshAll();

    void RefreshAll()
    {
        foreach (CharacterSlotUI s in slots)
        {
            if (s != null) s.Refresh();
        }
    }

    public void Toggle()
    {
        if (panel == null) return;
        bool open = !panel.activeSelf;
        panel.SetActive(open);
        if (open) RefreshAll();
    }

    public void Close()
    {
        if (panel != null) panel.SetActive(false);
    }
}
