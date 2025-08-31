using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GenTalk1 : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI textTarget;
    public GameObject TotalRoll;
    public GameObject HandUI;
    public GameObject ChipsUI;

    [Header("Scene Swap")]
    public string nextSceneName; // name of the scene to load after last element

    [Header("Lines (in order)")]
    [TextArea] public List<string> lines = new List<string>();

    [Header("Timing")]
    [Min(0f)] public float charDelay = 0.03f;
    [Min(0f)] public float postLineDelay = 1.5f;

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
            keyAudioSource.spatialBlend = 0f;
        }
    }

    void OnEnable()
    {
        if (runCo != null) StopCoroutine(runCo);
        runCo = StartCoroutine(RunTalk());
    }

    IEnumerator RunTalk()
    {
        running = true;
        if (textTarget) textTarget.text = "";

        if (lines == null || lines.Count == 0 || textTarget == null)
        {
            Debug.LogWarning("[GenTalk1] No lines or text target set.");
            running = false;
            yield break;
        }

        for (int i = 0; i < lines.Count; i++)
        {
            yield return TypeRoutine(lines[i]);

            if (i == 1)
            {
                ResetAllFingers();
                if (HandUI != null) HandUI.SetActive(true);
                if (TotalRoll != null) TotalRoll.SetActive(true);
            }

            if (i == 2)
            {
                if (ChipsUI != null) ChipsUI.SetActive(true);
            }

            if (i == 5)
            {
                if (HandUI != null) HandUI.SetActive(false);
            }

            // After last element, swap scene
            if (i == lines.Count - 1 && !string.IsNullOrEmpty(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }

            if (postLineDelay > 0f) yield return new WaitForSeconds(postLineDelay);
        }

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

    void ResetAllFingers()
    {
        // Add your reset logic here
        // Example: FingerManager.Instance?.ResetFingers();
    }

    public bool IsRunning() => running;
}
