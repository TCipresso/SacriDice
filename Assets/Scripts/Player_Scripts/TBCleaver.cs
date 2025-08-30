using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TBCleaver : MonoBehaviour
{
    [Header("Target")]
    public TextMeshProUGUI textTarget;

    [Header("Lines")]
    [TextArea] public List<string> lines = new List<string>();

    [Header("Typewriter")]
    [Min(0f)] public float charDelay = 0.03f;
    public bool startOnEnable = true;

    [Header("Typing Sound")]
    public AudioClip keyClip;
    [Min(1)] public int charsPerSound = 2;      // Play a sound every N characters typed
    public bool ignoreWhitespaceForSound = true;
    [Range(0f, 1f)] public float keyVolume = 1f;
    [Tooltip("If not set, an AudioSource will be added automatically.")]
    public AudioSource keyAudioSource;

    Coroutine typeCo;

    void Awake()
    {
        if (!keyAudioSource)
        {
            keyAudioSource = gameObject.AddComponent<AudioSource>();
            keyAudioSource.playOnAwake = false;
            keyAudioSource.spatialBlend = 0f; // 2D
        }
    }

    void OnEnable()
    {
        if (startOnEnable)
            StartTypingRandom();
    }

    public void StartTypingRandom()
    {
        if (lines == null || lines.Count == 0 || textTarget == null)
            return;

        string pick = lines[Random.Range(0, lines.Count)];
        StartTyping(pick);
    }

    public void StartTyping(string content)
    {
        if (typeCo != null) StopCoroutine(typeCo);
        typeCo = StartCoroutine(TypeRoutine(content));
    }

    IEnumerator TypeRoutine(string content)
    {
        textTarget.text = "";
        int typedCountForSound = 0;

        for (int i = 0; i < content.Length; i++)
        {
            char c = content[i];
            textTarget.text += c;

            // Count for sound trigger
            bool countThis = true;
            if (ignoreWhitespaceForSound && char.IsWhiteSpace(c))
                countThis = false;

            if (countThis)
            {
                typedCountForSound++;
                if (charsPerSound > 0 && (typedCountForSound % charsPerSound) == 0)
                    PlayKeySound();
            }

            if (charDelay > 0f) yield return new WaitForSeconds(charDelay);
            else yield return null;
        }

        typeCo = null;
    }

    void PlayKeySound()
    {
        if (!keyClip || !keyAudioSource) return;
        keyAudioSource.PlayOneShot(keyClip, keyVolume);
    }

    // Optional: instantly finish current line
    public void SkipToEnd()
    {
        if (typeCo != null)
        {
            StopCoroutine(typeCo);
            typeCo = null;
        }
    }
}
