using UnityEngine;

/// <summary>
/// 게임의 모든 사운드를 관리하는 싱글턴. (BGM / SFX 분리)
///
/// 클립은 다른 에셋(스프라이트 등)과 마찬가지로 인스펙터에서 배정한다.
/// (Assets/Resource/사운드/ 아래의 파일들을 각 필드에 드래그)
/// 호출 측은 클립을 몰라도 되도록 PlayAttack()/PlayHit()/... 같은 의미 있는
/// 메서드만 부른다 — 어떤 클립이 날지는 여기 한곳에서만 정한다.
///
/// 볼륨은 SaveManager 의 진행도 저장과 별개로 다룬다 — 진행도를 초기화(엔딩 다시시작)해도
/// 사운드 설정은 유지되어야 하므로 각자 PlayerPrefs 키를 쓴다.
///
/// BGM 전환:
///   스테이지 BGM — StageManager.OnStageChanged 를 구독해 스테이지가 바뀌면 자동 교체.
///   보스 BGM     — 보스전이 시작되면 BossManager 가 PlayBossBgm() 을 부른다.
///                  보스전 실패 시 ResumeStageBgm() 으로 원래 스테이지 곡으로 되돌린다.
///                  (보스 처치 성공은 스테이지가 올라가며 OnStageChanged 로 자동 교체된다)
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private const string KEY_BGM = "ToMorrow.Vol.BGM";
    private const string KEY_SFX = "ToMorrow.Vol.SFX";

    [Header("배경음 재생 소스 (loop 켠 AudioSource)")]
    public AudioSource bgmSource;

    [Header("효과음 재생 소스 (공용 재생기, PlaySfx가 PlayOneShot으로 낸다)")]
    public AudioSource sfxSource;

    [Header("초기 볼륨 (저장값이 없을 때, 0~1)")]
    [Range(0f, 1f)] public float defaultBgm = 0.7f;
    [Range(0f, 1f)] public float defaultSfx = 0.8f;

    [Header("효과음 클립 (Assets/Resource/사운드/)")]
    [Tooltip("플레이어 공격 sfx/attack")] public AudioClip sfxAttack;
    [Tooltip("몬스터가 공격받는 sfx/hit")] public AudioClip sfxHit;
    [Tooltip("꿈코인드랍 sfx/coin")] public AudioClip sfxCoin;
    [Tooltip("버튼 클릭 sfx/ui_click")] public AudioClip sfxClick;
    [Tooltip("보스 등장 sfx/boss_appeared (보스전 시작 시 1회)")] public AudioClip sfxBossAppear;

    [Header("스테이지 BGM (인덱스 = 스테이지 번호. 0번=STAGE.00=stage1_bgm ... 12번=STAGE.12=stage13_bgm)")]
    public AudioClip[] stageBgms;

    [Header("보스전 BGM (보스전투 bgm/boss_bgm)")]
    public AudioClip bossBgm;

    public float BgmVolume { get; private set; }
    public float SfxVolume { get; private set; }

    // 지금 재생 중인 스테이지 곡의 스테이지 번호. 보스전 실패 후 복귀할 때 참조한다.
    private int currentBgmStage = -1;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 저장된 볼륨 불러오기 (없으면 기본값)
        BgmVolume = PlayerPrefs.GetFloat(KEY_BGM, defaultBgm);
        SfxVolume = PlayerPrefs.GetFloat(KEY_SFX, defaultSfx);
        ApplyBgmVolume();
    }

    void Start()
    {
        // 모든 Awake 이후(=저장 복원·CSV 주입 완료 후)라 CurrentStage 가 확정돼 있다.
        if (StageManager.Instance != null)
        {
            PlayStageBgm(StageManager.Instance.CurrentStage);
            StageManager.Instance.OnStageChanged += PlayStageBgm;
        }
    }

    void OnDestroy()
    {
        if (StageManager.Instance != null)
        {
            StageManager.Instance.OnStageChanged -= PlayStageBgm;
        }
    }

    // ── 볼륨 설정 (슬라이더가 호출) ────────────────────────
    public void SetBgmVolume(float v)
    {
        BgmVolume = Mathf.Clamp01(v);
        PlayerPrefs.SetFloat(KEY_BGM, BgmVolume);
        PlayerPrefs.Save();
        ApplyBgmVolume();
    }

    public void SetSfxVolume(float v)
    {
        SfxVolume = Mathf.Clamp01(v);
        PlayerPrefs.SetFloat(KEY_SFX, SfxVolume);
        PlayerPrefs.Save();
        // 효과음은 PlayOneShot(clip, SfxVolume) 로 매번 볼륨을 실어 낸다.
        // 그래서 sfxSource.volume 은 1로 두고 여기서 건드리지 않는다 — 안 그러면 볼륨이 두 번 곱해진다.
    }

    void ApplyBgmVolume()
    {
        if (bgmSource != null) bgmSource.volume = BgmVolume;
    }

    // ── 저수준 재생 ────────────────────────────────────────
    /// <summary>효과음을 1회 재생한다. 현재 SFX 볼륨이 실린다.</summary>
    public void PlaySfx(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, SfxVolume);
    }

    /// <summary>배경음을 바꿔 재생한다. 같은 곡이 이미 재생 중이면 다시 시작하지 않는다.</summary>
    public void PlayBgm(AudioClip clip, bool loop = true)
    {
        if (bgmSource == null || clip == null) return;
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.volume = BgmVolume;
        bgmSource.Play();
    }

    // ── 효과음 (호출 측은 이 메서드만 부른다) ───────────────
    public void PlayAttack() => PlaySfx(sfxAttack);
    public void PlayHit() => PlaySfx(sfxHit);
    public void PlayCoin() => PlaySfx(sfxCoin);
    /// <summary>버튼 클릭음. 인스펙터 Button.onClick 에 직접 연결해도 된다.</summary>
    public void PlayClick() => PlaySfx(sfxClick);
    public void PlayBossAppear() => PlaySfx(sfxBossAppear);

    // ── 배경음 ─────────────────────────────────────────────
    /// <summary>해당 스테이지의 BGM을 재생한다. (StageManager.OnStageChanged 구독 대상)</summary>
    public void PlayStageBgm(int stage)
    {
        currentBgmStage = stage;
        if (stageBgms == null || stage < 0 || stage >= stageBgms.Length)
        {
            Debug.LogWarning("AudioManager: 스테이지 " + stage + " 의 BGM 클립이 배정돼 있지 않습니다.");
            return;
        }
        PlayBgm(stageBgms[stage]);
    }

    /// <summary>보스전 BGM으로 전환한다. (보스전 시작 시)</summary>
    public void PlayBossBgm()
    {
        PlayBgm(bossBgm);
    }

    /// <summary>보스전에서 원래 스테이지 BGM으로 되돌린다. (보스전 실패 시)</summary>
    public void ResumeStageBgm()
    {
        PlayStageBgm(currentBgmStage);
    }

    /// <summary>배경음을 완전히 멈춘다. (최종 보스 처치 → 엔딩)</summary>
    public void StopBgm()
    {
        if (bgmSource != null) bgmSource.Stop();
    }
}
