using UnityEngine;
using TMPro;

public class AnteManager : MonoBehaviour
{
    public static AnteManager Instance { get; private set; }

    [Header("Ante Rules")]
    public int targetAnte = 32;  // the goal to meet/exceed
    public int maxRounds = 3;   // rounds per ante

    [Header("Scaling")]
    public int anteStep = 8;     // how much to increase target each time you enter the Shop

    [Header("UI")]
    public TextMeshProUGUI anteTMP;     // assign in inspector
    public string antePrefix = "Ante: ";

    [Header("Runtime")]
    public int currentRoundIndex = 0;  // 0-based count of completed rounds in this ante
    public int cumulativeTotal = 0;  // carries across rounds within the current ante
    public bool earlyWin = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        UpdateAnteUI();
    }

    /// <summary>
    /// Call once per round after the cinematic finishes (pass this round's roll total).
    /// </summary>
    public void ApplyRoundResult(int roundTotal)
    {
        currentRoundIndex++;
        cumulativeTotal += Mathf.Max(0, roundTotal);

        Debug.Log($"[ANTE] Round {currentRoundIndex}/{maxRounds}: +{roundTotal}  -> Total {cumulativeTotal}/{targetAnte}");

        if (cumulativeTotal >= targetAnte && !earlyWin && currentRoundIndex < maxRounds)
        {
            earlyWin = true;
            Debug.Log("[ANTE] EARLY WIN: target reached before final round.");
            // optional: trigger early-win UI
        }

        if (currentRoundIndex >= maxRounds)
        {
            if (cumulativeTotal >= targetAnte)
                Debug.Log("[ANTE] WIN after final round.");
            else
                Debug.Log("[ANTE] LOSS: target not met after final round.");

            // (Do NOT reset here; we prepare the next loop when entering Shop)
        }
    }

    /// <summary>
    /// Called when entering the Shop after a 3-round loop (or early win).
    /// Increases the ante for the next loop and resets counters.
    /// </summary>
    public void HandleEnterShop()
    {
        // Bump difficulty for NEXT loop
        targetAnte += anteStep;
        UpdateAnteUI();

        // Prepare a fresh run for the next three rounds
        ResetAnteRun();
    }

    /// <summary> Reset for a new ante loop (Round1 will start from zero).</summary>
    public void ResetAnteRun()
    {
        currentRoundIndex = 0;
        cumulativeTotal = 0;
        earlyWin = false;
        Debug.Log("[ANTE] Reset run.");
    }

    void UpdateAnteUI()
    {
        if (anteTMP) anteTMP.text = antePrefix + targetAnte.ToString();
    }
}
