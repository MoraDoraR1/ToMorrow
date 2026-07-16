using System;
using UnityEngine;

/// <summary>
/// 진행도 저장/복원. PlayerPrefs에 JSON으로 넣는다.
///
/// 로드는 Awake에서 한다 — 모든 Awake는 모든 Start보다 먼저 실행되고,
/// UI들(DreamCoinDisplay/MonsterCountDisplay/StageBackground/각 패널)은 Start에서
/// 현재 값을 직접 읽어 초기화하므로, 이 순서면 복원값이 자동으로 화면에 반영된다.
///
/// DataTableLoader도 Awake에서 CSV를 주입하지만 서로 건드리는 필드가 다르다
/// (CSV = 비용·데미지 같은 설정값 / 저장 = 해금·레벨·구매 같은 진행값)
/// 그래서 Awake 실행 순서와 무관하게 안전하다.
///
/// 저장은 값이 바뀔 때 표시만 해두고 일정 간격으로 한 번씩 쓴다 —
/// 몬스터를 잡을 때마다 디스크에 쓰면 낭비다.
/// </summary>
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private const string KEY = "ToMorrow.Save";
    private const int VERSION = 1;

    [Header("자동 저장 간격 (초)")]
    public float autoSaveInterval = 5f;

    [Header("시작할 때 저장된 진행도 불러오기")]
    public bool loadOnStart = true;

    private bool dirty;
    private float timer;

    /// <summary>
    /// 이번 실행에서 '지난 저장 이후 흐른 시간'(초). 저장본이 없으면 0.
    /// OfflineReward가 Start에서 읽어 보상을 계산한다.
    /// </summary>
    public double OfflineSeconds { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (loadOnStart) Load();
    }

    void Start()
    {
        // 진행도가 바뀌는 순간들을 구독해 '저장 필요' 표시만 남긴다
        if (StageManager.Instance != null)
        {
            StageManager.Instance.OnStageChanged += _ => dirty = true;
            StageManager.Instance.OnKillCountChanged += (_, __) => dirty = true;
        }
        if (CurrencyManager.Instance != null) CurrencyManager.Instance.OnDreamCoinChanged += _ => dirty = true;
        if (CharacterManager.Instance != null) CharacterManager.Instance.OnCharactersChanged += () => dirty = true;
        if (StoryManager.Instance != null) StoryManager.Instance.OnStoriesChanged += () => dirty = true;
        if (ConstellationManager.Instance != null) ConstellationManager.Instance.OnConstellationsChanged += () => dirty = true;
    }

    void Update()
    {
        if (!dirty) return;
        timer += Time.deltaTime;
        if (timer >= autoSaveInterval)
        {
            timer = 0f;
            Save();
        }
    }

    // 앱을 내리거나 끌 때는 즉시 저장한다 (모바일은 OnApplicationQuit이 안 불릴 수 있어 Pause도 잡는다)
    void OnApplicationPause(bool paused) { if (paused) Save(); }
    void OnApplicationFocus(bool focus) { if (!focus) Save(); }
    void OnApplicationQuit() { Save(); }

    // ── 저장 ──────────────────────────────────────────────
    public void Save()
    {
        if (!dirty) return;

        SaveData d = new SaveData { version = VERSION, lastSaveTicks = DateTime.UtcNow.Ticks };

        if (StageManager.Instance != null)
        {
            d.stage = StageManager.Instance.CurrentStage;
            d.killCount = StageManager.Instance.KillCount;
        }
        if (CurrencyManager.Instance != null) d.dreamCoin = CurrencyManager.Instance.DreamCoin;

        CharacterManager cm = CharacterManager.Instance;
        if (cm != null)
        {
            d.charUnlocked = new bool[cm.Count];
            d.charLevel = new int[cm.Count];
            for (int i = 0; i < cm.Count; i++)
            {
                CharacterData c = cm.Get(i);
                if (c == null) continue;
                d.charUnlocked[i] = c.unlocked;
                d.charLevel[i] = c.level;
            }
        }

        StoryManager sm = StoryManager.Instance;
        if (sm != null)
        {
            d.storyPurchased = new bool[sm.Count];
            for (int i = 0; i < sm.Count; i++)
            {
                StoryData s = sm.Get(i);
                if (s != null) d.storyPurchased[i] = s.purchased;
            }
        }

        ConstellationManager km = ConstellationManager.Instance;
        if (km != null)
        {
            d.constStars = new int[km.Count];
            for (int i = 0; i < km.Count; i++)
            {
                ConstellationData c = km.Get(i);
                if (c != null) d.constStars[i] = c.purchasedStars;
            }
        }

        PlayerPrefs.SetString(KEY, JsonUtility.ToJson(d));
        PlayerPrefs.Save();
        dirty = false;
    }

    // ── 복원 ──────────────────────────────────────────────
    public void Load()
    {
        if (!PlayerPrefs.HasKey(KEY)) return;

        SaveData d;
        try
        {
            d = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(KEY));
        }
        catch (Exception e)
        {
            Debug.LogWarning("SaveManager: 저장본을 읽지 못해 새로 시작합니다. " + e.Message);
            return;
        }
        if (d == null) return;

        if (d.version != VERSION)
        {
            Debug.LogWarning("SaveManager: 저장 형식이 달라(" + d.version + " != " + VERSION + ") 무시하고 새로 시작합니다.");
            return;
        }

        if (StageManager.Instance != null) StageManager.Instance.RestoreProgress(d.stage, d.killCount);
        if (CurrencyManager.Instance != null) CurrencyManager.Instance.RestoreDreamCoin(d.dreamCoin);

        CharacterManager cm = CharacterManager.Instance;
        if (cm != null && d.charUnlocked != null && d.charLevel != null)
        {
            // 저장 이후 캐릭터 수가 바뀌었을 수 있으니 짧은 쪽에 맞춘다
            int n = Mathf.Min(cm.Count, d.charUnlocked.Length, d.charLevel.Length);
            for (int i = 0; i < n; i++)
            {
                CharacterData c = cm.Get(i);
                if (c == null) continue;
                c.unlocked = d.charUnlocked[i];
                c.level = Mathf.Clamp(d.charLevel[i], 1, c.maxLevel);
            }
        }

        StoryManager sm = StoryManager.Instance;
        if (sm != null && d.storyPurchased != null)
        {
            int n = Mathf.Min(sm.Count, d.storyPurchased.Length);
            for (int i = 0; i < n; i++)
            {
                StoryData s = sm.Get(i);
                if (s != null) s.purchased = d.storyPurchased[i];
            }
        }

        ConstellationManager km = ConstellationManager.Instance;
        if (km != null && d.constStars != null)
        {
            int n = Mathf.Min(km.Count, d.constStars.Length);
            for (int i = 0; i < n; i++)
            {
                ConstellationData c = km.Get(i);
                if (c != null) c.purchasedStars = Mathf.Clamp(d.constStars[i], 0, c.starCount);
            }
        }

        // 오프라인 경과 시간. 기기 시계를 되돌리면 음수가 나올 수 있어 0으로 막는다.
        if (d.lastSaveTicks > 0)
        {
            double sec = (DateTime.UtcNow - new DateTime(d.lastSaveTicks, DateTimeKind.Utc)).TotalSeconds;
            OfflineSeconds = Math.Max(0.0, sec);
        }

        Debug.Log("진행도 불러옴: STAGE." + d.stage.ToString("00") + " / 꿈코인 " + d.dreamCoin
                  + " / 오프라인 " + Mathf.FloorToInt((float)OfflineSeconds / 60f) + "분");
    }

    /// <summary>저장본을 지운다. (엔딩의 '처음부터 다시시작')</summary>
    public void Delete()
    {
        PlayerPrefs.DeleteKey(KEY);
        PlayerPrefs.Save();
        dirty = false;
        Debug.Log("저장된 진행도를 지웠습니다.");
    }
}
