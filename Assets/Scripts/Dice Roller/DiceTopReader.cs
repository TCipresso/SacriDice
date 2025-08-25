using System.Collections.Generic;
using UnityEngine;

public class DiceTopReader : MonoBehaviour
{
    public DiceBase data;
    public Rigidbody rb;
    public List<Transform> faceUpMarkers = new List<Transform>();

    public float settleLinear = 0.05f;
    public float settleAngular = 1f;
    public float settleHold = 0.6f;

    public float sampleDuration = 0.25f;
    public float minTopDot = 0.88f;      // how "up" the winning face must be
    public float minMargin = 0.05f;      // gap vs 2 face to accept result

    public bool useGroundNormal = true;  // use table normal instead of world up
    public LayerMask groundMask = ~0;
    public float groundRay = 0.5f;

    public bool debugDraw = true;

    public bool isSettled;
    public int resultIndex = -1;
    public string resultValue;

    float stillFor;
    bool sampling;
    float sampleT;
    float[] vote;         // vote count per face
    float[] sumDot;       // accumulated dot per face
    Vector3 refUp;        // reference "up" for this sampling window

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody>();
        if (!data) data = GetComponent<DiceBase>();
    }

    void Update()
    {
        if (!rb || faceUpMarkers == null || faceUpMarkers.Count == 0) return;

        bool isStill = rb.linearVelocity.sqrMagnitude < settleLinear * settleLinear &&
                       rb.angularVelocity.magnitude < settleAngular;

        if (isStill)
        {
            stillFor += Time.deltaTime;
        }
        else
        {
            ResetStateTransient();
        }

        if (!isSettled && stillFor >= settleHold)
        {
            if (!sampling)
            {
                BeginSampling();
            }
            else
            {
                SampleOnce();
                sampleT -= Time.deltaTime;

                if (sampleT <= 0f)
                {
                    TryFinalize();
                }
            }
        }
    }

    void BeginSampling()
    {
        int n = faceUpMarkers.Count;
        vote = new float[n];
        sumDot = new float[n];
        sampleT = sampleDuration;
        sampling = true;

        refUp = GetReferenceUp();
        if (debugDraw) Debug.DrawRay(transform.position, refUp * 0.6f, Color.green, sampleDuration, false);
    }

    void SampleOnce()
    {
        int best = -1;
        float bestD = -999f;
        int second = -1;
        float secondD = -999f;

        for (int i = 0; i < faceUpMarkers.Count; i++)
        {
            var t = faceUpMarkers[i];
            if (!t) continue;

            float d = Vector3.Dot(t.up, refUp); // how "up" this face is wrt refUp
            if (d > bestD) { second = best; secondD = bestD; best = i; bestD = d; }
            else if (d > secondD) { second = i; secondD = d; }

            if (debugDraw)
            {
                var col = Color.Lerp(Color.red, Color.green, Mathf.InverseLerp(-1f, 1f, d));
                Debug.DrawRay(t.position, t.up * 0.25f, col, 0.1f, false);
            }

            sumDot[i] += d;
        }

        if (best >= 0)
        {
            // weight votes by how confidently "up" it is this frame
            float weight = Mathf.Max(0f, bestD);
            vote[best] += 1f + weight; // vote  confidence
        }
    }

    void TryFinalize()
    {
        int winner = -1;
        float bestVotes = -1f;
        int runner = -1;
        float secondVotes = -1f;

        for (int i = 0; i < vote.Length; i++)
        {
            if (vote[i] > bestVotes) { runner = winner; secondVotes = bestVotes; winner = i; bestVotes = vote[i]; }
            else if (vote[i] > secondVotes) { runner = i; secondVotes = vote[i]; }
        }

        float avgDotWinner = sumDot[winner] / Mathf.Max(1f, (sampleDuration / Time.deltaTime)); // approximate
        float margin = (bestVotes - Mathf.Max(0f, secondVotes)) / Mathf.Max(1f, bestVotes);

        bool passDot = avgDotWinner >= minTopDot;
        bool passMargin = margin >= minMargin;

        if (passDot && passMargin)
        {
            resultIndex = winner;
            resultValue = IndexToValue(winner);
            isSettled = true;

            Debug.Log($"[DiceTopReader] {name}  face {winner} value {resultValue} | avgDot={avgDotWinner:F3} margin={margin:F3}");
            DiceRollManager.Instance?.OnDieSettled(this);
        }
        else
        {
            // not confident keep waiting for a more stable posture
            sampling = false;
            // keep stillFor so we try again soon, but avoid hammering:
            stillFor = Mathf.Min(stillFor, settleHold + 0.1f);
            if (debugDraw)
                Debug.Log($"[DiceTopReader] {name} not confident yet (avgDot {avgDotWinner:F3} >= {minTopDot}, margin {margin:F3} >= {minMargin}). Waiting�");
        }
    }

    Vector3 GetReferenceUp()
    {
        if (!useGroundNormal) return Vector3.up;

        Ray ray = new Ray(transform.position + Vector3.up * 0.05f, Vector3.down);
        if (Physics.Raycast(ray, out var hit, groundRay + 0.2f, groundMask, QueryTriggerInteraction.Ignore))
            return hit.normal.normalized;

        return Vector3.up;
    }

    string IndexToValue(int i)
    {
        if (!data || data.sideValues == null || data.sideValues.Count == 0) return (i + 1).ToString();
        if (i < 0 || i >= data.sideValues.Count) return (i + 1).ToString();
        return data.sideValues[i];
    }

    void ResetStateTransient()
    {
        stillFor = 0f;
        sampling = false;
        isSettled = false;
        resultIndex = -1;
        resultValue = null;
    }
}
