using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 옵션 패널. Option_Btn 으로 열고, 안에 게임 종료 버튼과 볼륨 슬라이더(BGM/SFX)를 둔다.
///
/// 슬라이더는 AudioManager 의 현재 볼륨으로 초기화되고, 움직이면 즉시 반영·저장된다.
/// (AudioManager 가 PlayerPrefs 에 저장하므로 다음 실행에도 유지된다)
/// </summary>
public class OptionPanelUI : MonoBehaviour
{
    [Header("열고 닫을 패널 (Option_Panel)")]
    public GameObject panel;

    [Header("패널을 여는 버튼 (Option_Btn)")]
    public Button openButton;

    [Header("닫기 버튼")]
    public Button closeButton;

    [Header("게임 종료 버튼")]
    public Button quitButton;

    [Header("볼륨 슬라이더 (0~1, 선택)")]
    public Slider bgmSlider;
    public Slider sfxSlider;

    void Start()
    {
        if (panel != null) panel.SetActive(false);

        if (openButton != null) openButton.onClick.AddListener(Toggle);
        if (closeButton != null) closeButton.onClick.AddListener(Close);
        if (quitButton != null) quitButton.onClick.AddListener(QuitGame);

        // 슬라이더를 현재 볼륨으로 맞추고, 변경 시 AudioManager 로 흘려보낸다
        AudioManager am = AudioManager.Instance;
        if (bgmSlider != null)
        {
            if (am != null) bgmSlider.SetValueWithoutNotify(am.BgmVolume);
            bgmSlider.onValueChanged.AddListener(OnBgmChanged);
        }
        if (sfxSlider != null)
        {
            if (am != null) sfxSlider.SetValueWithoutNotify(am.SfxVolume);
            sfxSlider.onValueChanged.AddListener(OnSfxChanged);
        }
    }

    void OnBgmChanged(float v)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SetBgmVolume(v);
    }

    void OnSfxChanged(float v)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SetSfxVolume(v);
    }

    public void Toggle()
    {
        if (panel == null) return;
        bool open = !panel.activeSelf;
        panel.SetActive(open);

        // 열 때 현재 볼륨으로 슬라이더를 다시 맞춘다 (다른 경로로 값이 바뀌었을 수 있음)
        if (open)
        {
            AudioManager am = AudioManager.Instance;
            if (am != null)
            {
                if (bgmSlider != null) bgmSlider.SetValueWithoutNotify(am.BgmVolume);
                if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(am.SfxVolume);
            }
        }
    }

    public void Close()
    {
        if (panel != null) panel.SetActive(false);
    }

    /// <summary>
    /// 게임을 종료한다. 진행도는 SaveManager 가 이미 자동 저장하지만,
    /// 종료 직전에 한 번 더 확실히 저장하고 끈다.
    /// 에디터에서는 Application.Quit 이 안 먹히므로 플레이 모드를 멈춘다.
    /// </summary>
    public void QuitGame()
    {
        if (SaveManager.Instance != null) SaveManager.Instance.Save();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
