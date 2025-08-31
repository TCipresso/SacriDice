using UnityEngine;

public class DisableAfterDelay : MonoBehaviour
{
    public GameObject targetObject;
    public float delay = 3f;

    void OnEnable()
    {
        if (targetObject != null)
            Invoke(nameof(DisableTarget), delay);
    }

    void DisableTarget()
    {
        if (targetObject != null)
            targetObject.SetActive(false);
    }
}
