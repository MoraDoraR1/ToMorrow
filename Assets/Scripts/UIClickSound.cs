using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 모든 UI 버튼 클릭음을 한곳에서 낸다.
///
/// 버튼마다 리스너를 다는 대신, 클릭이 일어난 지점 아래에 '활성화된 Button'이 있으면
/// 클릭음을 재생한다. 이러면 인스펙터로 단 버튼이든 런타임에 생성되는 슬롯(캐릭터/이야기)이든
/// 배선 없이 전부 커버된다.
///
/// 씬에 EventSystem 이 있는 오브젝트(또는 아무 상시 오브젝트) 하나에 붙여두면 된다.
/// 구형 Input Manager 를 쓰는 프로젝트 관례에 맞춰 UnityEngine.Input 으로 클릭을 감지한다.
/// </summary>
public class UIClickSound : MonoBehaviour
{
    // RaycastAll 결과를 매 클릭 새로 할당하지 않도록 재사용한다.
    private static readonly List<RaycastResult> results = new List<RaycastResult>();

    void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        EventSystem es = EventSystem.current;
        if (es == null) return;

        PointerEventData data = new PointerEventData(es) { position = Input.mousePosition };
        results.Clear();
        es.RaycastAll(data, results);

        for (int i = 0; i < results.Count; i++)
        {
            Button btn = results[i].gameObject.GetComponentInParent<Button>();
            if (btn != null && btn.interactable)
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
                return; // 한 번의 클릭엔 한 번만
            }
        }
    }
}
