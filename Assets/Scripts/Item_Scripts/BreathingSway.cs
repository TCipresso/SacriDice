using UnityEngine;

public class BreathingSway : MonoBehaviour
{
    [Header("Breathing Settings")]
    [SerializeField] private float amplitudeX = 0.2f;  // how far it moves side-to-side
    [SerializeField] private float amplitudeY = 0.1f;  // how far it moves up-and-down
    [SerializeField] private float frequencyX = 1f;    // speed of sway on X
    [SerializeField] private float frequencyY = 0.5f;  // speed of sway on Y

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        float x = Mathf.Sin(Time.time * frequencyX) * amplitudeX;
        float y = Mathf.Sin(Time.time * frequencyY) * amplitudeY;

        // Only modify X and Y, leave Z alone
        transform.localPosition = new Vector3(
            startPos.x + x,
            startPos.y + y,
            startPos.z
        );
    }
}
