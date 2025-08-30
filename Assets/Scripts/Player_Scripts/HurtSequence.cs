using System.Collections;
using UnityEngine;

public class HurtSequence : MonoBehaviour
{
    public static HurtSequence Instance { get; private set; }

    [Header("Objects")]
    public GameObject Clever;      // shown immediately when Sacrifice is pressed
    public GameObject Textbox;     // shown at StartSac()
    public GameObject RollUI;      // enabled after sequence finishes
    public GameObject HandUI;      // DISABLED when Sacrifice is pressed
    public GameObject SacButton;

    [Header("Camera")]
    public Transform playerCamera; // if null, falls back to Camera.main
    [Tooltip("How many degrees to yaw to the right during the sequence.")]
    public float rightYawDegrees = 45f;

    [Header("Timings")]
    public float preRotateDelay = 3f;     // wait after showing Textbox
    public float rotateDuration = 3f;     // rotate to the right (yaw)
    public float returnDuration = 3f;     // rotate back to yaw 0
    public float holdAtRightDuration = 3f;


    [Header("Pitch (X-axis)")]
    [Tooltip("Saved default camera pitch (X) in degrees.")]
    public float defaultCameraPitchX = 22.35f;
    [Tooltip("Time to smoothly rotate camera pitch to 0° when sacrifice is pressed.")]
    public float pitchToZeroDuration = 1.25f;
    public AnimationCurve pitchEasing = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    Coroutine sequenceRoutine;
    Coroutine pitchRoutine;
    bool running;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        // Optional: DontDestroyOnLoad(gameObject);
    }

    // Called by the Sacrifice button (CommitSelected is called elsewhere)
    // - Enables Clever
    // - Disables HandUI
    // - Smoothly rotates camera pitch (X) to 0°
    public void OnSacrificePressed()
    {
        if (Clever) Clever.SetActive(true);
        if (HandUI) HandUI.SetActive(false);
        if (SacButton) SacButton.SetActive(false);

        var cam = GetCam();
        if (cam != null)
        {
            if (pitchRoutine != null) StopCoroutine(pitchRoutine);
            pitchRoutine = StartCoroutine(RotatePitchToZero(cam, pitchToZeroDuration));
        }
    }

    // Call this to run the rest of the cinematic (textbox -> yaw right -> yaw back -> show RollUI)
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

        // hide textbox after the pre-rotate delay
        if (Textbox) Textbox.SetActive(false);

        var cam = GetCam();
        if (cam != null)
        {
            // keep original orientation (we'll return to this yaw)
            Vector3 startEuler = cam.eulerAngles;

            // 1) rotate yaw to the right (relative)
            Vector3 targetRight = new Vector3(startEuler.x, startEuler.y + rightYawDegrees, startEuler.z);
            yield return RotateTo(cam, targetRight, rotateDuration);

            // 2) HOLD at the right-turned angle
            if (holdAtRightDuration > 0f)
                yield return new WaitForSeconds(holdAtRightDuration);

            // 3) rotate yaw back to ORIGINAL orientation (not zero)
            Vector3 backEuler = new Vector3(22.35f, 0f, startEuler.z);

            yield return RotateTo(cam, backEuler, returnDuration);
        }

        if (RollUI) RollUI.SetActive(true);

        running = false;
        sequenceRoutine = null;
    }



    // --- Helpers ---

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
