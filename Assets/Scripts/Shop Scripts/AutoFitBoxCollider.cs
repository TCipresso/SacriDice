// File: AutoFitBoxCollider.cs
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class AutoFitBoxCollider : MonoBehaviour
{
    [Tooltip("Extra padding around the combined bounds in world units.")]
    public Vector3 padding = new Vector3(0.02f, 0.02f, 0.02f);

    void Reset() { FitNow(); }
    void Awake() { FitNow(); }
#if UNITY_EDITOR
    void OnValidate() { FitNow(); }
#endif

    public void FitNow()
    {
        var renderers = GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0) return;

        // Combine world-space bounds of all renderers
        Bounds b = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
        b.Expand(padding);

        // Convert world center/size to local for the BoxCollider
        var box = GetComponent<BoxCollider>();
        Vector3 localCenter = transform.InverseTransformPoint(b.center);
        Vector3 worldSize = b.size;
        Vector3 localSize = new Vector3(
            worldSize.x / Mathf.Max(0.0001f, transform.lossyScale.x),
            worldSize.y / Mathf.Max(0.0001f, transform.lossyScale.y),
            worldSize.z / Mathf.Max(0.0001f, transform.lossyScale.z)
        );

        box.center = localCenter;
        box.size = localSize;
        box.isTrigger = false; // should be solid for raycasts
    }
}
