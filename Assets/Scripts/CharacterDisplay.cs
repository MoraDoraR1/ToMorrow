using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 해금된 캐릭터를 메인 화면에 배치해 보여준다. (기획서 2.6 "화면 배치")
/// 게임 시작 시엔 메인 캐릭터만 보이고, 서브 캐릭터를 해금하면 그 자리가 켜진다.
///
/// 자리(slots)는 인스펙터가 소유한다 — 배치 좌표는 기획이 정할 영역이라
/// 스프라이트/좌표는 씬에서 잡고, 이 스크립트는 켜고 끄는 것과 이미지 교체만 한다.
/// slots[0]은 메인 캐릭터(Char)의 Image다.
///
/// GameObject가 아니라 Image 컴포넌트만 켜고 끈다 — Char에는 PlayerAttack이 붙어 있어
/// 오브젝트를 끄면 클릭 공격이 멈추기 때문이다.
/// </summary>
public class CharacterDisplay : MonoBehaviour
{
    [Header("캐릭터가 표시될 자리 (인덱스 = 캐릭터 번호, 0 = 메인 캐릭터 Char)")]
    public Image[] slots;

    void Start()
    {
        if (CharacterManager.Instance != null)
        {
            CharacterManager.Instance.OnCharactersChanged += Refresh;
        }
        else
        {
            Debug.LogWarning("CharacterDisplay: 씬에 CharacterManager가 없습니다.");
        }

        Refresh();
    }

    void OnDestroy()
    {
        if (CharacterManager.Instance != null)
        {
            CharacterManager.Instance.OnCharactersChanged -= Refresh;
        }
    }

    /// <summary>해금 상태에 맞춰 각 자리를 켜고 끈다.</summary>
    void Refresh()
    {
        if (slots == null || CharacterManager.Instance == null) return;

        for (int i = 0; i < slots.Length; i++)
        {
            Image img = slots[i];
            if (img == null) continue;

            CharacterData c = CharacterManager.Instance.Get(i);
            bool show = c != null && c.unlocked && c.sprite != null;

            if (show) img.sprite = c.sprite;
            img.enabled = show;
        }
    }
}
