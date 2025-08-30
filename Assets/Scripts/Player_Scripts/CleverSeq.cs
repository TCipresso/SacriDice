using System.Collections;
using UnityEngine;

public class CleaverSeq : MonoBehaviour
{
    [Header("Target")]
    public Transform endPos;

    [Header("Motion")]
    [Min(0.01f)] public float duration = 1.5f;
    public AnimationCurve easing = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public bool matchRotation = false;
    public bool useUnscaledTime = false;

    Coroutine moveCo;

    void OnEnable()
    {
        if (endPos == null)
        {
            Debug.LogWarning("[CleaverSeq] endPos not set.");
            return;
        }

        if (moveCo != null) StopCoroutine(moveCo);
        moveCo = StartCoroutine(MoveToEnd());
    }

    IEnumerator MoveToEnd()
    {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        Vector3 targetPos = endPos.position;
        Quaternion targetRot = endPos.rotation;

        float t = 0f;
        while (t < duration)
        {
            t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float k = duration <= 0f ? 1f : Mathf.Clamp01(t / duration);
            float e = (easing != null) ? easing.Evaluate(k) : k;

            transform.position = Vector3.Lerp(startPos, targetPos, e);
            if (matchRotation)
                transform.rotation = Quaternion.Slerp(startRot, targetRot, e);

            yield return null;
        }

        // snap final
        transform.position = targetPos;
        if (matchRotation) transform.rotation = targetRot;

        // kick off the hurt sequence
        if (HurtSequence.Instance != null)
        {
            HurtSequence.Instance.StartSac();
        }
        else
        {
            Debug.LogWarning("[CleaverSeq] HurtSequence.Instance is null; cannot StartSac().");
        }

        moveCo = null;
    }
}
