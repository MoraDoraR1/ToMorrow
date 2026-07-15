using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 이야기 패널. StoryManager의 이야기 수만큼 슬롯 프리팹을 자동 생성하고,
/// 구매·캐릭터 레벨업·꿈코인 변동이 있을 때 표시를 갱신한다.
/// Story_Btn으로 패널을 열고 닫는다.
/// </summary>
public class StoryPanelUI : MonoBehaviour
{
    [Header("열고 닫을 패널 (Story_Panel)")]
    public GameObject panel;

    [Header("패널을 여는 버튼 (Story_Btn)")]
    public Button openButton;

    [Header("닫기 버튼 (선택)")]
    public Button closeButton;

    [Header("슬롯 프리팹 (StorySlotUI 포함)")]
    public GameObject slotPrefab;

    [Header("슬롯이 담길 컨테이너 (Scroll View의 Content)")]
    public Transform slotContainer;

    private readonly List<StorySlotUI> slots = new List<StorySlotUI>();

    void Start()
    {
        BuildSlots();
        if (panel != null) panel.SetActive(false);

        if (openButton != null) openButton.onClick.AddListener(Toggle);
        if (closeButton != null) closeButton.onClick.AddListener(Close);

        if (StoryManager.Instance != null)
        {
            StoryManager.Instance.OnStoriesChanged += RefreshAll;
        }
        // 캐릭터 레벨이 올라가면 이야기가 열릴 수 있으므로 함께 구독
        if (CharacterManager.Instance != null)
        {
            CharacterManager.Instance.OnCharactersChanged += RefreshAll;
        }
        // 코인이 변하면 구매 버튼 활성 여부가 달라짐
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnDreamCoinChanged += HandleCoinChanged;
        }
    }

    void OnDestroy()
    {
        if (StoryManager.Instance != null)
        {
            StoryManager.Instance.OnStoriesChanged -= RefreshAll;
        }
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
            Debug.LogWarning("StoryPanelUI: 슬롯 프리팹 또는 컨테이너가 비어 있습니다.");
            return;
        }
        if (StoryManager.Instance == null)
        {
            Debug.LogWarning("StoryPanelUI: 씬에 StoryManager가 없습니다.");
            return;
        }

        int count = StoryManager.Instance.Count;
        for (int i = 0; i < count; i++)
        {
            GameObject go = Instantiate(slotPrefab, slotContainer);
            StorySlotUI slot = go.GetComponent<StorySlotUI>();
            if (slot == null)
            {
                Debug.LogError("StoryPanelUI: 슬롯 프리팹에 StorySlotUI가 없습니다.");
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
        foreach (StorySlotUI s in slots)
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
