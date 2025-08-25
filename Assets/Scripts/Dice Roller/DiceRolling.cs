using UnityEngine;
using System.Linq;

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
    Rigidbody held;
    Vector3 planePoint;
    Vector3 localGrabOffset;
    bool lifting;
    Vector3 targetPos;
    bool wasKinematic;
    bool wasUseGravity;
    RigidbodyInterpolation wasInterp;
    float wasDrag;
    float wasAngularDrag;
    Vector3 spinAxis;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (!cam) cam = Camera.main;
    }

    void Update()
    {
        if (forceCursor) { Cursor.visible = true; Cursor.lockState = CursorLockMode.None; }

        if (Input.GetMouseButtonDown(0)) TryPick();
        if (Input.GetMouseButtonUp(0)) Release();

        if (!held) return;

        var p = new Plane(planeNormal.normalized, planePoint + planeNormal.normalized * dragHeight);
        var ray = cam.ScreenPointToRay(Input.mousePosition);
        if (p.Raycast(ray, out var enter))
            targetPos = ray.GetPoint(enter);
        else
            targetPos = held.position;

        var grabWorld = held.transform.TransformPoint(localGrabOffset);
        var offset = grabWorld - held.position;
        var desired = targetPos - offset;

        if (lifting)
        {
            var lifted = Vector3.Lerp(held.position, desired, 1f - Mathf.Exp(-pickupLiftLerp * Time.deltaTime));
            held.linearVelocity = (lifted - held.position) / Mathf.Max(Time.deltaTime, 0.0001f);
            if ((lifted - desired).sqrMagnitude < 0.0004f) lifting = false;
        }
        else
        {
            var toTarget = desired - held.position;
            held.linearVelocity = toTarget * followSpeed;
        }
    }

    void FixedUpdate()
    {
        if (!held) return;
        if (spinWhileHeld)
        {
            float radPerSec = spinSpeedDegPerSec * Mathf.Deg2Rad;
            held.angularVelocity = spinAxis * radPerSec;
        }
        else
        {
            held.angularVelocity = Vector3.zero;
        }
    }

    void TryPick()
    {
        if (held) return;
        var ray = cam.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out var hit, maxPickDistance, pickMask, QueryTriggerInteraction.Ignore)) return;
        var rb = hit.rigidbody; if (!rb) return;

        planePoint = hit.point;
        held = rb;
        wasKinematic = held.isKinematic;
        wasUseGravity = held.useGravity;
        wasInterp = held.interpolation;
        wasDrag = held.linearDamping;
        wasAngularDrag = held.angularDamping;

        held.isKinematic = false;
        held.useGravity = false;
        held.interpolation = RigidbodyInterpolation.Interpolate;
        held.linearDamping = 5f;
        held.angularDamping = 5f;

        localGrabOffset = centerOnPickup ? Vector3.zero : held.transform.InverseTransformPoint(hit.point);
        lifting = true;

        if (randomizeSpinAxisEachPickup)
            spinAxis = Random.onUnitSphere.normalized;
        else
            spinAxis = Vector3.up;
    }

    void Release()
    {
        if (!held) return;
        held.useGravity = wasUseGravity;
        held.isKinematic = wasKinematic;
        held.interpolation = wasInterp;
        held.linearDamping = wasDrag;
        held.angularDamping = wasAngularDrag;
        held = null;
        lifting = false;
    }
}
