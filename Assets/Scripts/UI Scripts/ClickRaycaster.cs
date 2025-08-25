// File: ClickRaycaster.cs
using UnityEngine;

public class ClickRaycaster : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private float maxDistance = 100f;
    [SerializeField] private LayerMask layerMask = ~0; // Everything by default
    [SerializeField] private bool debugLogs = true;

    void Awake()
    {
        if (cam == null) cam = Camera.main;
        if (debugLogs && cam == null)
            Debug.LogWarning("[ClickRaycaster] No camera set and no Camera.main found. Assign a Camera.");
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (cam == null) return;

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.cyan, 1.0f);

            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, layerMask, QueryTriggerInteraction.Ignore))
            {
                if (debugLogs)
                    Debug.Log("[ClickRaycaster] Hit: " + hit.collider.name + " on layer " + LayerMask.LayerToName(hit.collider.gameObject.layer));

                var clickable = hit.collider.GetComponentInParent<IClickable>();
                if (clickable != null)
                {
                    clickable.OnClick();
                }
                else if (debugLogs)
                {
                    Debug.LogWarning("[ClickRaycaster] No IClickable found on parent of " + hit.collider.name);
                }
            }
            else if (debugLogs)
            {
                Debug.Log("[ClickRaycaster] Raycast hit nothing.");
            }
        }
    }
}
