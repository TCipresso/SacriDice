using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CameraShakeSimple : MonoBehaviour
{
    [Header("Shake Settings")]
    public float debugShakeDuration = 0.3f;
    public float debugShakeMagnitude = 0.2f;
    public float dampingSpeed = 1.0f;

    [Header("Audio")]
    [Tooltip("Optional AudioSource to play the shake sound from. If not set, PlayClipAtPoint will be used.")]
    public AudioSource audioSource;
    [Tooltip("Sound to play when a shake starts.")]
    public AudioClip shakeClip;
    [Range(0f, 1f)] public float volume = 1f;
    public bool randomizePitch = true;
    public float minPitch = 0.95f;
    public float maxPitch = 1.05f;
    [Tooltip("If false, the source will be restarted when a new shake triggers while a sound is still playing.")]
    public bool allowOverlap = true;

    private Vector3 originalPos;
    private float shakeDuration = 0f;
    private float shakeMagnitude = 0.1f;

    void Awake()
    {
        originalPos = transform.localPosition;
    }

    void Update()
    {
        if (shakeDuration > 0)
        {
            transform.localPosition = originalPos + Random.insideUnitSphere * shakeMagnitude;
            shakeDuration -= Time.deltaTime * dampingSpeed;
        }
        else
        {
            shakeDuration = 0f;
            transform.localPosition = originalPos;
        }
    }

    public void TriggerShake(float duration, float magnitude)
    {
        shakeDuration = duration;
        shakeMagnitude = magnitude;
        PlayShakeSound();
    }

    public void TriggerShakeDefault()
    {
        TriggerShake(debugShakeDuration, debugShakeMagnitude);
    }

    // ---- Audio helper ----
    void PlayShakeSound()
    {
        if (shakeClip == null && audioSource == null) return;

        if (audioSource != null)
        {
            if (!allowOverlap && audioSource.isPlaying)
                audioSource.Stop();

            if (randomizePitch)
                audioSource.pitch = Random.Range(minPitch, maxPitch);
            else
                audioSource.pitch = 1f;

            // Prefer explicit clip if set; otherwise use the source's current clip
            var clipToPlay = shakeClip != null ? shakeClip : audioSource.clip;
            if (clipToPlay != null)
                audioSource.PlayOneShot(clipToPlay, volume);
        }
        else if (shakeClip != null)
        {
            // Fallback if no AudioSource assigned
            AudioSource.PlayClipAtPoint(shakeClip, transform.position, volume);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(CameraShakeSimple))]
    public class CameraShakeSimpleEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            CameraShakeSimple shake = (CameraShakeSimple)target;
            if (GUILayout.Button("Trigger Debug Shake"))
            {
                shake.TriggerShake(shake.debugShakeDuration, shake.debugShakeMagnitude);
            }
        }
    }
#endif
}
