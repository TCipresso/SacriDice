using UnityEngine;
using UnityEngine.EventSystems;  // <-- needed for IsPointerOverGameObject

public class ClickRaycaster : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private float maxDistance = 100f;
    [SerializeField] private LayerMask layerMask = ~0; // Everything by default
    [SerializeField] private bool ignoreWhenPointerOverUI = true;
    [SerializeField] private bool debugLogs = false;

    void Awake()
    {
        if (cam == null) cam = Camera.main;
    }

    void Update()
    {
        // Mouse click
        if (Input.GetMouseButtonDown(0))
        {
            if (ignoreWhenPointerOverUI && IsPointerOverUI()) return;
            DoPhysicsClick(Input.mousePosition);
        }

        // Touch support (began)
        for (int i = 0; i < Input.touchCount; i++)
        {
            var touch = Input.GetTouch(i);
            if (touch.phase == TouchPhase.Began)
            {
                if (ignoreWhenPointerOverUI && IsPointerOverUI(touch.fingerId)) continue;
                DoPhysicsClick(touch.position);
            }
        }
    }

    bool IsPointerOverUI(int pointerId = -1)
    {
        // -1 is mouse, fingerId for touch
        if (EventSystem.current == null) return false;
        return EventSystem.current.IsPointerOverGameObject(pointerId);
    }

    void DoPhysicsClick(Vector3 screenPos)
    {
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, layerMask, QueryTriggerInteraction.Ignore))
        {
            if (debugLogs) Debug.Log("[ClickRaycaster] Hit: " + hit.collider.name);
            hit.collider.GetComponentInParent<IClickable>()?.OnClick();
        }
        else
        {
            if (debugLogs) Debug.Log("[ClickRaycaster] Raycast hit nothing.");
        }
    }
}
