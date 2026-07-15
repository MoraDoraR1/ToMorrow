using System;
using UnityEngine;

/// <summary>
/// 캐릭터 해금/레벨업을 관리한다. (메인 1 + 서브 7, 선형 해금)
/// 해금된 캐릭터들의 스탯 합이 클릭(콧물) 데미지가 된다.
/// </summary>
public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance { get; private set; }

    [Header("캐릭터 목록 (0=메인, 1~7=서브. 앞에서부터 순서대로 해금)")]
    [SerializeField] private CharacterData[] characters;

    // 해금/레벨업으로 상태가 바뀌었을 때 (UI 갱신용)
    public event Action OnCharactersChanged;

    public int Count => characters != null ? characters.Length : 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public CharacterData Get(int index)
    {
        if (characters != null && index >= 0 && index < characters.Length) return characters[index];
        return null;
    }

    /// <summary>해금된 캐릭터들의 클릭 데미지 합계 (최소 1).</summary>
    public int TotalClickDamage
    {
        get
        {
            int sum = 0;
            if (characters != null)
            {
                foreach (CharacterData c in characters)
                {
                    if (c != null && c.unlocked) sum += c.CurrentDamage;
                }
            }
            return Mathf.Max(sum, 1);
        }
    }

    /// <summary>해금 가능 여부 — 선형 해금(앞 캐릭터가 해금돼 있어야 함).</summary>
    public bool CanUnlock(int index)
    {
        CharacterData c = Get(index);
        if (c == null || c.unlocked) return false;

        if (index > 0)
        {
            CharacterData prev = Get(index - 1);
            if (prev == null || !prev.unlocked) return false;
        }
        return true;
    }

    /// <summary>꿈코인을 소모해 캐릭터를 해금한다.</summary>
    public bool TryUnlock(int index)
    {
        if (!CanUnlock(index)) return false;

        CharacterData c = Get(index);
        if (CurrencyManager.Instance == null || !CurrencyManager.Instance.TrySpend(c.unlockCost))
        {
            Debug.Log("꿈코인이 부족합니다. (해금 비용 " + c.unlockCost + ")");
            return false;
        }

        c.unlocked = true;
        c.level = 1;
        OnCharactersChanged?.Invoke();
        Debug.Log(c.characterName + " 해금!");
        return true;
    }

    /// <summary>현재 레벨 기준 레벨업 비용.</summary>
    public int GetLevelUpCost(int index)
    {
        CharacterData c = Get(index);
        if (c == null) return 0;
        return c.levelUpBaseCost + (c.level - 1) * c.levelUpCostPerLevel;
    }

    /// <summary>레벨업 가능 여부 (해금됐고 최대 레벨 미만).</summary>
    public bool CanLevelUp(int index)
    {
        CharacterData c = Get(index);
        return c != null && c.unlocked && c.level < c.maxLevel;
    }

    /// <summary>꿈코인을 소모해 레벨을 1 올린다.</summary>
    public bool TryLevelUp(int index)
    {
        if (!CanLevelUp(index)) return false;

        CharacterData c = Get(index);
        int cost = GetLevelUpCost(index);
        if (CurrencyManager.Instance == null || !CurrencyManager.Instance.TrySpend(cost))
        {
            Debug.Log("꿈코인이 부족합니다. (레벨업 비용 " + cost + ")");
            return false;
        }

        c.level++;
        OnCharactersChanged?.Invoke();
        Debug.Log(c.characterName + " LV" + c.level + " (클릭 데미지 합계 " + TotalClickDamage + ")");
        return true;
    }
}
