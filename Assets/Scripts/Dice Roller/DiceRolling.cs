using System.Collections.Generic;
using UnityEngine;

public class DiceRolling : MonoBehaviour
{
    public LayerMask pickMask = ~0;
    public float followSpeed = 28f;
    public float maxPickDistance = 200f;
    public bool forceCursor = true;

    public Vector3 planeNormal = Vector3.up;
    public float dragHeight = 0.6f;
    public bool centerOnPickup = true;
    public float pickupLiftLerp = 18f;

    public bool spinWhileHeld = true;
    public float spinSpeedDegPerSec = 420f;
    public bool randomizeSpinAxisEachPickup = true;

    Camera cam;
    Vector3 planePoint;

    class HeldState
    {
        public Rigidbody rb;
        public Vector3 localGrabOffset;
        public bool lifting;
        public bool wasKinematic;
        public bool wasUseGravity;
        public RigidbodyInterpolation wasInterp;
        public float wasDrag;
        public float wasAngularDrag;
        public Vector3 spinAxis;
    }

    readonly List<HeldState> held = new();

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (!cam) cam = Camera.main;
    }

    void Update()
    {
        if (forceCursor) { Cursor.visible = true; Cursor.lockState = CursorLockMode.None; }

        if (Input.GetMouseButtonDown(0)) TryPickSingle();
        if (Input.GetMouseButtonUp(0)) ReleaseAll();

        if (held.Count == 0) return;

        var p = new Plane(planeNormal.normalized, planePoint + planeNormal.normalized * dragHeight);
        var ray = cam.ScreenPointToRay(Input.mousePosition);
        Vector3 targetPos;
        if (p.Raycast(ray, out var enter)) targetPos = ray.GetPoint(enter);
        else targetPos = held[0].rb.position;

        for (int i = 0; i < held.Count; i++)
        {
            var h = held[i];
            if (!h.rb) continue;

            var grabWorld = h.rb.transform.TransformPoint(h.localGrabOffset);
            var offset = grabWorld - h.rb.position;
            var desired = targetPos - offset;

            if (h.lifting)
            {
                var lifted = Vector3.Lerp(h.rb.position, desired, 1f - Mathf.Exp(-pickupLiftLerp * Time.deltaTime));
                h.rb.linearVelocity = (lifted - h.rb.position) / Mathf.Max(Time.deltaTime, 0.0001f);
                if ((lifted - desired).sqrMagnitude < 0.0004f) h.lifting = false;
            }
            else
            {
                var toTarget = desired - h.rb.position;
                h.rb.linearVelocity = toTarget * followSpeed;
            }
        }
    }

    void FixedUpdate()
    {
        if (held.Count == 0) return;

        if (spinWhileHeld)
        {
            float radPerSec = spinSpeedDegPerSec * Mathf.Deg2Rad;
            for (int i = 0; i < held.Count; i++)
            {
                var h = held[i];
                if (!h.rb) continue;
                h.rb.angularVelocity = h.spinAxis * radPerSec;
            }
        }
        else
        {
            for (int i = 0; i < held.Count; i++)
            {
                var h = held[i];
                if (!h.rb) continue;
                h.rb.angularVelocity = Vector3.zero;
            }
        }
    }

    void TryPickSingle()
    {
        if (held.Count > 0) return;

        var ray = cam.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out var hit, maxPickDistance, pickMask, QueryTriggerInteraction.Ignore)) return;
        var rb = hit.rigidbody; if (!rb) return;

        planePoint = hit.point;

        var hs = MakeHeldState(rb, hit.point);
        held.Add(hs);
    }

    HeldState MakeHeldState(Rigidbody rb, Vector3 hitPoint)
    {
        var hs = new HeldState();
        hs.rb = rb;
        hs.wasKinematic = rb.isKinematic;
        hs.wasUseGravity = rb.useGravity;
        hs.wasInterp = rb.interpolation;
        hs.wasDrag = rb.linearDamping;
        hs.wasAngularDrag = rb.angularDamping;

        rb.isKinematic = false;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.linearDamping = 5f;
        rb.angularDamping = 5f;

        hs.localGrabOffset = centerOnPickup ? Vector3.zero : rb.transform.InverseTransformPoint(hitPoint);
        hs.lifting = true;

        if (randomizeSpinAxisEachPickup) hs.spinAxis = Random.onUnitSphere.normalized;
        else hs.spinAxis = Vector3.up;

        return hs;
    }

    public void BeginHoldGroup(IEnumerable<Rigidbody> rbs)
    {
        held.Clear();

        var ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit, maxPickDistance, pickMask, QueryTriggerInteraction.Ignore))
            planePoint = hit.point;
        else
            planePoint = cam.transform.position + cam.transform.forward * 2f;

        foreach (var rb in rbs)
        {
            if (!rb) continue;
            var hs = MakeHeldState(rb, rb.position);
            hs.localGrabOffset = Vector3.zero;
            held.Add(hs);
        }
    }

    public void SpawnFromStashAndBeginHold(float spreadRadius = 0.15f)
    {
        if (DiceStash.Instance == null) return;

        var ray = cam.ScreenPointToRay(Input.mousePosition);
        Vector3 basePos;
        if (Physics.Raycast(ray, out var hit, 500f, pickMask, QueryTriggerInteraction.Ignore))
            basePos = hit.point + planeNormal.normalized * dragHeight;
        else
            basePos = cam.transform.position + cam.transform.forward * 2f;

        var spawned = new List<Rigidbody>();
        foreach (var prefab in DiceStash.Instance.dice)
        {
            if (!prefab) continue;
            var inst = Instantiate(prefab, basePos + Random.insideUnitSphere * spreadRadius, Random.rotation);
            var rb = inst.GetComponent<Rigidbody>();
            if (rb) spawned.Add(rb);
        }

        BeginHoldGroup(spawned);
    }

    public void ReleaseAll()
    {
        if (held.Count == 0) return;

        for (int i = 0; i < held.Count; i++)
        {
            var h = held[i];
            if (!h.rb) continue;
            h.rb.useGravity = h.wasUseGravity;
            h.rb.isKinematic = h.wasKinematic;
            h.rb.interpolation = h.wasInterp;
            h.rb.linearDamping = h.wasDrag;
            h.rb.angularDamping = h.wasAngularDrag;
        }
        held.Clear();
    }
}
