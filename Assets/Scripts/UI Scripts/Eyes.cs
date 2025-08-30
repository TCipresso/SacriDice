using System.Collections;
using UnityEngine;

public class Eyes : MonoBehaviour
{
    [Header("Motion")]
    [Tooltip("How far to move upward on the Y axis.")]
    public float moveAmountY = 0.5f;

    [Tooltip("How long the move should take (seconds).")]
    [Min(0.01f)] public float duration = 1.0f;

    [Tooltip("Wait this long before starting the move (seconds).")]
    [Min(0f)] public float startDelay = 2.0f;

    [Tooltip("Start moving automatically when enabled.")]
    public bool startOnEnable = true;

    [Tooltip("Easing for the movement (0..1).")]
    public AnimationCurve easing = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Tooltip("Move in local space instead of world space.")]
    public bool useLocalSpace = true;

    [Header("After Move")]
    public GameObject cigar;   // enabled after the move completes

    Coroutine moveCo;
    Vector3 originalPos;

    void Awake()
    {
        originalPos = useLocalSpace ? transform.localPosition : transform.position;
    }

    void OnEnable()
    {
        if (startOnEnable)
            MoveUp();
    }

    /// <summary>
    /// Starts the smooth upward move by moveAmountY (after startDelay).
    /// </summary>
    [ContextMenu("Move Up")]
    public void MoveUp()
    {
        if (moveCo != null) StopCoroutine(moveCo);
        moveCo = StartCoroutine(MoveUpRoutine());
    }

    /// <summary>
    /// Resets back to the position captured at Awake.
    /// </summary>
    [ContextMenu("Reset Position")]
    public void ResetPosition()
    {
        if (moveCo != null) StopCoroutine(moveCo);
        if (useLocalSpace) transform.localPosition = originalPos;
        else transform.position = originalPos;
    }

    IEnumerator MoveUpRoutine()
    {
        // Delay before starting the motion
        if (startDelay > 0f)
            yield return new WaitForSeconds(startDelay);

        // Capture start AFTER delay so we move from the current spot
        Vector3 start = useLocalSpace ? transform.localPosition : transform.position;
        Vector3 end = start + Vector3.up * moveAmountY;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            float e = easing != null ? easing.Evaluate(k) : k;

            Vector3 p = Vector3.LerpUnclamped(start, end, e);
            if (useLocalSpace) transform.localPosition = p;
            else transform.position = p;

            yield return null;
        }

        if (useLocalSpace) transform.localPosition = end;
        else transform.position = end;

        // Enable cigar at the end
        if (cigar) cigar.SetActive(true);

        moveCo = null;
    }
}
