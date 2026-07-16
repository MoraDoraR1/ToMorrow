using UnityEngine;

/// <summary>
/// 사운드 볼륨을 관리한다. (BGM / SFX 분리)
///
/// 지금은 효과음 자체가 없지만, 볼륨값을 저장·적용하는 뼈대를 먼저 둔다.
/// 팀원이 사운드를 붙이면 PlaySfx()/PlayBgm() 를 호출하기만 하면 볼륨이 자동 적용된다.
///
/// 볼륨은 SaveManager 의 진행도 저장과 별개로 다룬다 — 진행도를 초기화(엔딩 다시시작)해도
/// 사운드 설정은 유지되어야 하므로 각자 PlayerPrefs 키를 쓴다.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private const string KEY_BGM = "ToMorrow.Vol.BGM";
    private const string KEY_SFX = "ToMorrow.Vol.SFX";

    [Header("배경음 재생 소스 (선택 — 붙이면 볼륨이 실시간 반영)")]
    public AudioSource bgmSource;

    [Header("효과음 재생 소스 (선택 — 공용 재생기, PlaySfx가 이걸로 낸다)")]
    public AudioSource sfxSource;

    [Header("초기 볼륨 (저장값이 없을 때, 0~1)")]
    [Range(0f, 1f)] public float defaultBgm = 0.7f;
    [Range(0f, 1f)] public float defaultSfx = 0.8f;

    public float BgmVolume { get; private set; }
    public float SfxVolume { get; private set; }

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
        ApplyBgm();
    }

    // ── 볼륨 설정 (슬라이더가 호출) ────────────────────────
    public void SetBgmVolume(float v)
    {
        BgmVolume = Mathf.Clamp01(v);
        PlayerPrefs.SetFloat(KEY_BGM, BgmVolume);
        PlayerPrefs.Save();
        ApplyBgm();
    }

    public void SetSfxVolume(float v)
    {
        SfxVolume = Mathf.Clamp01(v);
        PlayerPrefs.SetFloat(KEY_SFX, SfxVolume);
        PlayerPrefs.Save();
        if (sfxSource != null) sfxSource.volume = SfxVolume;
    }

    void ApplyBgm()
    {
        if (bgmSource != null) bgmSource.volume = BgmVolume;
    }

    // ── 재생 (사운드가 붙으면 이 두 개를 호출) ─────────────
    /// <summary>효과음을 1회 재생한다. 볼륨은 현재 SFX 설정이 곱해진다.</summary>
    public void PlaySfx(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, SfxVolume);
    }

    /// <summary>배경음을 바꿔 재생한다.</summary>
    public void PlayBgm(AudioClip clip, bool loop = true)
    {
        if (bgmSource == null || clip == null) return;
        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.volume = BgmVolume;
        bgmSource.Play();
    }
}
