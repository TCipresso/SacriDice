using System.Collections;
using UnityEngine;

public class HurtSequence : MonoBehaviour
{
    public static HurtSequence Instance { get; private set; }

    [Header("Objects")]
    public GameObject Clever;
    public GameObject Textbox;
    public GameObject RollUI;
    public GameObject HandUI;
    public GameObject SacButton;

    [Header("Intro Mode")]
    public bool IsIntro = false;
    public GameObject TextBox3;

    [Header("Camera")]
    public Transform playerCamera;
    public float rightYawDegrees = 45f;

    [Header("Timings")]
    public float preRotateDelay = 3f;
    public float rotateDuration = 3f;
    public float returnDuration = 3f;
    public float holdAtRightDuration = 3f;

    [Header("Pitch (X-axis)")]
    public float defaultCameraPitchX = 22.35f;
    public float pitchToZeroDuration = 1.25f;
    public AnimationCurve pitchEasing = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Shake Sequence")]
    public CameraShakeSimple shaker;
    public int shakes = 3;
    public Vector2 delayBetweenShakes = new Vector2(0.35f, 1.0f);
    public float shakeDuration = 0.25f;
    public float shakeMagnitude = 0.2f;

    Coroutine sequenceRoutine;
    Coroutine pitchRoutine;
    bool running;

    Vector3 cleverStartPos;
    Quaternion cleverStartRot;
    bool cleverStartCached = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (Clever)
        {
            cleverStartPos = Clever.transform.position;
            cleverStartRot = Clever.transform.rotation;
            cleverStartCached = true;
        }
    }

    public void OnSacrificePressed()
    {
        if (Clever) Clever.SetActive(true);
        if (HandUI) HandUI.SetActive(false);
        if (SacButton) SacButton.SetActive(false);

        if (Clever && !cleverStartCached)
        {
            cleverStartPos = Clever.transform.position;
            cleverStartRot = Clever.transform.rotation;
            cleverStartCached = true;
        }

        var cam = GetCam();
        if (cam != null)
        {
            if (pitchRoutine != null) StopCoroutine(pitchRoutine);
            pitchRoutine = StartCoroutine(RotatePitchToZero(cam, pitchToZeroDuration));
        }
    }

    public void StartSac()
    {
        if (running) return;
        sequenceRoutine = StartCoroutine(SacRoutine());
    }

    public void StopSac()
    {
        if (sequenceRoutine != null) StopCoroutine(sequenceRoutine);
        sequenceRoutine = null;
        running = false;
    }

    IEnumerator SacRoutine()
    {
        running = true;

        if (Textbox) Textbox.SetActive(true);
        yield return new WaitForSeconds(preRotateDelay);
        if (Textbox) Textbox.SetActive(false);

        var cam = GetCam();
        if (cam != null)
        {
            Vector3 startEuler = cam.eulerAngles;

            Vector3 targetRight = new Vector3(startEuler.x, startEuler.y + rightYawDegrees, startEuler.z);
            yield return RotateTo(cam, targetRight, rotateDuration);

            float minDelay = Mathf.Min(delayBetweenShakes.x, delayBetweenShakes.y);
            float maxDelay = Mathf.Max(delayBetweenShakes.x, delayBetweenShakes.y);
            for (int i = 0; i < Mathf.Max(0, shakes); i++)
            {
                if (shaker != null) shaker.TriggerShake(shakeDuration, shakeMagnitude);
                yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));
            }

            Vector3 backEuler = new Vector3(22.35f, 0f, startEuler.z);
            yield return RotateTo(cam, backEuler, returnDuration);
        }

        if (IsIntro)
        {
            if (TextBox3) TextBox3.SetActive(true);
        }
        else
        {
            if (RollUI) RollUI.SetActive(true);
        }

        ResetCleverToStart();

        running = false;
        sequenceRoutine = null;
    }

    void ResetCleverToStart()
    {
        if (Clever && cleverStartCached)
        {
            var t = Clever.transform;
            t.position = cleverStartPos;
            t.rotation = cleverStartRot;
            Clever.SetActive(false);
        }
    }

    Transform GetCam()
    {
        if (playerCamera != null) return playerCamera;
        var main = Camera.main;
        return main ? main.transform : null;
    }

    IEnumerator RotatePitchToZero(Transform cam, float duration)
    {
        if (duration <= 0f)
        {
            var e = cam.eulerAngles;
            cam.rotation = Quaternion.Euler(0f, e.y, e.z);
            yield break;
        }

        Vector3 start = cam.eulerAngles;
        float startX = start.x;
        float targetX = 0f;
        float y = start.y;
        float z = start.z;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            float e = pitchEasing != null ? pitchEasing.Evaluate(k) : k;

            float x = Mathf.LerpAngle(startX, targetX, e);
            cam.rotation = Quaternion.Euler(x, y, z);
            yield return null;
        }
        cam.rotation = Quaternion.Euler(targetX, y, z);
        pitchRoutine = null;
    }

    IEnumerator RotateTo(Transform t, Vector3 targetEuler, float duration)
    {
        if (duration <= 0f) { t.rotation = Quaternion.Euler(targetEuler); yield break; }

        Quaternion from = t.rotation;
        Quaternion to = Quaternion.Euler(targetEuler);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float k = Mathf.Clamp01(elapsed / duration);
            t.rotation = Quaternion.Slerp(from, to, k);
            yield return null;
        }
        t.rotation = to;
    }
}
