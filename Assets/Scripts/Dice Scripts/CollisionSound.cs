using UnityEngine;

public class ImpactSounds : MonoBehaviour
{
    public AudioClip[] clips;
    public float minImpulse = 0.15f;
    public float minInterval = 0.06f;
    public float volume = 1f;
    public Vector2 pitchRange = new Vector2(0.96f, 1.04f);
    public int voices = 4;
    public float stayScale = 0.5f;

    AudioSource[] pool;
    int nextIdx;
    float nextTime;

    void Awake()
    {
        pool = new AudioSource[Mathf.Max(1, voices)];
        for (int i = 0; i < pool.Length; i++)
        {
            var a = gameObject.AddComponent<AudioSource>();
            a.playOnAwake = false;
            a.spatialBlend = 1f;
            pool[i] = a;
        }
    }

    void TryPlay(float impulse)
    {
        if (clips == null || clips.Length == 0) return;
        if (Time.time < nextTime) return;
        if (impulse < minImpulse) return;

        nextTime = Time.time + minInterval;

        var a = pool[nextIdx];
        nextIdx = (nextIdx + 1) % pool.Length;

        a.pitch = Random.Range(pitchRange.x, pitchRange.y);
        a.PlayOneShot(clips[Random.Range(0, clips.Length)], volume);
    }

    void OnCollisionEnter(Collision c)
    {
        TryPlay(c.impulse.magnitude);
    }

    void OnCollisionStay(Collision c)
    {
        TryPlay(c.impulse.magnitude * stayScale);
    }
}
