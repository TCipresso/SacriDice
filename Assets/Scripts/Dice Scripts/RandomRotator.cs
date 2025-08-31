using UnityEngine;

public class ContinuousRandomRotator : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 60f;       // deg/sec
    [SerializeField] private float directionChangeSpeed = 0.5f; // how quickly axis drifts

    private Vector3 currentAxis;
    private Vector3 targetAxis;

    void Start()
    {
        // pick an initial random axis
        currentAxis = Random.onUnitSphere;
        targetAxis = Random.onUnitSphere;
    }

    void Update()
    {
        // smoothly drift current axis toward the target axis
        currentAxis = Vector3.Slerp(currentAxis, targetAxis, directionChangeSpeed * Time.deltaTime);

        // if we're very close, pick a new target axis
        if (Vector3.Angle(currentAxis, targetAxis) < 5f)
            targetAxis = Random.onUnitSphere;

        // apply rotation continuously
        transform.Rotate(currentAxis, rotationSpeed * Time.deltaTime, Space.World);
    }
}
