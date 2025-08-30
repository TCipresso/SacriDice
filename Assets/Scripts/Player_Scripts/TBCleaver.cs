using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;

public class TBCleaver : MonoBehaviour
{
    [Header("Target")]
    public TextMeshProUGUI targetText;          // assign in Inspector (auto-finds on Enable if missing)

    [Header("Lines To Type")]
    [TextArea(2, 4)]
    public List<string> candidates = new List<string>()
    {
        "You sure about this?",
        "Blood buys luck… sometimes.",
        "The house always eats.",
        "One more roll. What could go wrong?",
        "Fate loves a wager.",
    };

    [Header("Typewriter Settings")]
    [Tooltip("Base delay between characters (seconds).")]
    public float charDelay = 0.045f;

    [Tooltip("Random +/- variance added to each character delay.")]
    public float charDelayVariance = 0.015f;

    [Tooltip("Treat <rich text> tags as instant (not typed char-by-char).")]
    public bool preserveRichTextTags = true;

    [Tooltip("Clear existing text before typing.")]
    public bool clearOnStart = true;

    System.Random rng;

    void OnEnable()
    {
        if (!targetText) targetText = GetComponent<TextMeshProUGUI>();
        if (!targetText)
        {
            Debug.LogWarning("[TBCleaver] No TextMeshProUGUI assigned or found.");
            return;
        }

        if (rng == null) rng = new System.Random();

        string line = (candidates != null && candidates.Count > 0)
            ? candidates[rng.Next(0, candidates.Count)]
            : "";

        StopAllCoroutines();
        StartCoroutine(TypeRoutine(line));
    }

    IEnumerator TypeRoutine(string line)
    {
        if (clearOnStart) targetText.text = "";
        if (string.IsNullOrEmpty(line)) yield break;

        StringBuilder sb = new StringBuilder();
        int i = 0;

        while (i < line.Length)
        {
            // If preserving rich text, add tags instantly
            if (preserveRichTextTags && line[i] == '<')
            {
                int close = line.IndexOf('>', i);
                if (close != -1)
                {
                    sb.Append(line, i, close - i + 1);
                    targetText.text = sb.ToString();
                    i = close + 1;
                    continue;
                }
            }

            char c = line[i];
            sb.Append(c);
            targetText.text = sb.ToString();
            i++;

            float delay = charDelay + Random.Range(-charDelayVariance, charDelayVariance);
            if (delay < 0f) delay = 0f;
            yield return new WaitForSeconds(delay);
        }
    }
}
