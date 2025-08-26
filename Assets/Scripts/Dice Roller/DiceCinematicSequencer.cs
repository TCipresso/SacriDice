using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceCinematicSequencer : MonoBehaviour
{
    public Camera cam;
    public Transform homePose;
    public float homeFOV = 60f;

    [Header("Shot layout")]
    public float shotHeight = 0.5f;      // fixed height above die
    public float radiusMin = 0.3f;       // random horizontal offset radius
    public float radiusMax = 0.7f;
    public float shotFOV = 35f;          // tighter FOV for hero shot
    public bool useGroundNormal = true;  // use table normal instead of world up
    public LayerMask groundMask = ~0;
    public float groundRayLength = 1.0f;

    [Header("Timing")]
    public float moveDuration = 0.6f;
    public float holdDuration = 0.45f;
    public float returnDuration = 0.7f;
    public AnimationCurve ease = null;   // if null, EaseInOut

    bool playing;

    struct Shot { public Vector3 pos; public Quaternion rot; public float fov; public Transform look; }

    void Awake()
    {
        if (!cam) cam = Camera.main;
        if (ease == null) ease = AnimationCurve.EaseInOut(0, 0, 1, 1);
    }

    // Call this once when all dice are settled
    public void PlayComputedForSettled()
    {
        if (playing || !cam || !homePose) return;

        var readers = FindObjectsOfType<DiceTopReader>();
        var shots = new List<Shot>();

        foreach (var r in readers)
        {
            if (!r || !r.isSettled) continue;

            var C = r.transform.position;
            var U = GetRefUp(C);
            var horiz = RandomHorizontal(U, Random.Range(0f, 360f), Random.Range(radiusMin, radiusMax));
            var pos = C + U * shotHeight + horiz;
            var dir = (C - pos).normalized;
            var rot = Quaternion.LookRotation(dir, U);

            shots.Add(new Shot { pos = pos, rot = rot, fov = shotFOV, look = r.transform });
        }

        if (shots.Count == 0) return;
        StartCoroutine(PlaySequence(shots));
    }

    Vector3 GetRefUp(Vector3 at)
    {
        if (!useGroundNormal) return Vector3.up;
        var ray = new Ray(at + Vector3.up * 0.05f, Vector3.down);
        if (Physics.Raycast(ray, out var hit, groundRayLength + 0.1f, groundMask, QueryTriggerInteraction.Ignore))
            return hit.normal.normalized;
        return Vector3.up;
    }

    Vector3 RandomHorizontal(Vector3 up, float degrees, float radius)
    {
        // Build an orthonormal basis (X,Z) spanning the plane perpendicular to 'up'
        Vector3 any = Mathf.Abs(Vector3.Dot(up, Vector3.up)) > 0.9f ? Vector3.right : Vector3.up;
        Vector3 x = Vector3.Normalize(Vector3.Cross(up, any));
        Vector3 z = Vector3.Normalize(Vector3.Cross(up, x));
        float rad = degrees * Mathf.Deg2Rad;
        return (x * Mathf.Cos(rad) + z * Mathf.Sin(rad)) * radius;
    }

    IEnumerator PlaySequence(List<Shot> shots)
    {
        playing = true;

        for (int i = 0; i < shots.Count; i++)
        {
            var s = shots[i];
            yield return MoveCam(cam.transform.position, cam.transform.rotation, cam.fieldOfView,
                                 s.pos, s.rot, s.fov, moveDuration);

            float t = 0f;
            while (t < holdDuration)
            {
                t += Time.deltaTime;
                if (s.look)
                {
                    var U = GetRefUp(s.look.position);
                    var want = Quaternion.LookRotation((s.look.position - cam.transform.position).normalized, U);
                    cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, want, 0.25f);
                }
                yield return null;
            }
        }

        yield return MoveCam(cam.transform.position, cam.transform.rotation, cam.fieldOfView,
                             homePose.position, homePose.rotation, homeFOV, returnDuration);

        playing = false;
    }

    IEnumerator MoveCam(Vector3 fromPos, Quaternion fromRot, float fromFOV,
                        Vector3 toPos, Quaternion toRot, float toFOV, float dur)
    {
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / dur);
            float k = ease.Evaluate(a);
            cam.transform.position = Vector3.Lerp(fromPos, toPos, k);
            cam.transform.rotation = Quaternion.Slerp(fromRot, toRot, k);
            cam.fieldOfView = Mathf.Lerp(fromFOV, toFOV, k);
            yield return null;
        }
        cam.transform.SetPositionAndRotation(toPos, toRot);
        cam.fieldOfView = toFOV;
    }
}
