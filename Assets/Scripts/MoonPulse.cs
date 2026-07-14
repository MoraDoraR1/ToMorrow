using UnityEngine;

/// <summary>
/// 오브젝트(보름달)를 부드럽게 커졌다 작아지게 하는 펄스(숨쉬기) 효과.
/// 시작 시의 크기를 기준으로 minScale~maxScale 사이를 사인파로 오간다.
/// </summary>
public class MoonPulse : MonoBehaviour
{
    [Header("기준 크기 대비 최소 배율")]
    public float minScale = 0.9f;

    [Header("기준 크기 대비 최대 배율")]
    public float maxScale = 1.1f;

    [Header("한 번 커졌다 작아지는 주기 (초)")]
    public float period = 1.5f;

    private Vector3 baseScale;

    void Awake()
    {
        baseScale = transform.localScale;
    }

    void Update()
    {
        if (period <= 0f) return;

        // 0~1 사이를 오가는 사인파 (t=0.5에서 기준 크기)
        float t = (Mathf.Sin(Time.time * Mathf.PI * 2f / period) + 1f) * 0.5f;
        float scale = Mathf.Lerp(minScale, maxScale, t);
        transform.localScale = baseScale * scale;
    }
}
