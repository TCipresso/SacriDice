using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class IntroSequence : MonoBehaviour
{
    [Header("UI")]
    public GameObject textBox;              // <-- will be enabled after initial delay
    public TextMeshProUGUI textTarget;

    [Header("Lines (in order)")]
    [TextArea] public List<string> lines = new List<string>();

    [Header("Timing")]
    [Min(0f)] public float initialDelay = 2f;  // <-- wait before enabling textBox
    [Min(0f)] public float charDelay = 0.03f;
    [Min(0f)] public float postLineDelay = 1.5f;
    public bool startOnEnable = true;

    [Header("Typing Sound")]
    public AudioClip keyClip;
    [Min(1)] public int charsPerSound = 2;
    public bool ignoreWhitespaceForSound = true;
    [Range(0f, 1f)] public float keyVolume = 1f;
    public AudioSource keyAudioSource;

    Coroutine runCo;
    bool running;

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
            StartIntro();
    }

    [ContextMenu("Start Intro")]
    public void StartIntro()
    {
        if (runCo != null) StopCoroutine(runCo);
        runCo = StartCoroutine(RunIntro());
    }

    [ContextMenu("Stop Intro")]
    public void StopIntro()
    {
        if (runCo != null) StopCoroutine(runCo);
        runCo = null;
        running = false;
    }

    IEnumerator RunIntro()
    {
        running = true;
        if (textTarget) textTarget.text = "";

        if (lines == null || lines.Count == 0 || textTarget == null)
        {
            Debug.LogWarning("[IntroSequence] No lines or text target set.");
            running = false;
            yield break;
        }

        // Wait, then enable the text box
        if (initialDelay > 0f) yield return new WaitForSeconds(initialDelay);
        if (textBox) textBox.SetActive(true);

        // Type each line, pausing between them
        for (int i = 0; i < lines.Count; i++)
        {
            yield return TypeRoutine(lines[i]);
            if (postLineDelay > 0f) yield return new WaitForSeconds(postLineDelay);
        }

        Debug.Log("[IntroSequence] Reached end of lines.");
        running = false;
        runCo = null;
    }

    IEnumerator TypeRoutine(string content)
    {
        textTarget.text = "";
        int typedCountForSound = 0;

        for (int i = 0; i < content.Length; i++)
        {
            char c = content[i];
            textTarget.text += c;

            // Sound trigger (every N non-whitespace chars)
            bool countThis = !(ignoreWhitespaceForSound && char.IsWhiteSpace(c));
            if (countThis)
            {
                typedCountForSound++;
                if (charsPerSound > 0 && (typedCountForSound % charsPerSound) == 0)
                    PlayKeySound();
            }

            if (charDelay > 0f) yield return new WaitForSeconds(charDelay);
            else yield return null;
        }
    }

    void PlayKeySound()
    {
        if (!keyClip || !keyAudioSource) return;
        keyAudioSource.PlayOneShot(keyClip, keyVolume);
    }

    public bool IsRunning() => running;
}
