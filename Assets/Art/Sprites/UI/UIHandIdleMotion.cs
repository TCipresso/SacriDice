using UnityEngine;

public class UIHandIdleMotion : MonoBehaviour
{
    public Vector2 amplitude = new Vector2(6f, 4f);
    public Vector2 frequency = new Vector2(0.35f, 0.42f);
    public float followSpeed = 12f;
    public bool randomizeSeed = true;
    public float seed = 0f;

    RectTransform rt;
    Vector2 basePos;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        if (randomizeSeed) seed = Random.value * 1000f;
        basePos = rt.anchoredPosition;
    }

    void OnEnable()
    {
        basePos = rt.anchoredPosition;
    }

    void Update()
    {
        float t = Time.unscaledTime;
        float ox = (Mathf.PerlinNoise(seed, t * frequency.x) - 0.5f) * 2f * amplitude.x;
        float oy = (Mathf.PerlinNoise(seed + 123.456f, t * frequency.y) - 0.5f) * 2f * amplitude.y;
        Vector2 target = basePos + new Vector2(ox, oy);
        float k = 1f - Mathf.Exp(-followSpeed * Time.unscaledDeltaTime);
        rt.anchoredPosition = Vector2.Lerp(rt.anchoredPosition, target, k);
    }
}
