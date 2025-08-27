using UnityEngine;
using TMPro;

public class AnteManager : MonoBehaviour
{
    public static AnteManager Instance { get; private set; }

    [Header("Ante Rules")]
    public int targetAnte = 32;    // goal to meet/exceed for the current loop
    public int maxRounds = 3;      // rounds per loop
    public int currentLoop = 0;    // completed loops so far (used for scaling)

    [Header("Scaling")]
    public int anteStep = 8;       // base step; scaled exponentially each loop

    [Header("UI")]
    public TextMeshProUGUI anteTMP;
    public string antePrefix = "Ante: ";

    [Header("Runtime")]
    public int currentRoundIndex = 0; // 0-based rounds completed in this loop
    public int cumulativeTotal = 0;   // carries across rounds within this loop
    public bool earlyWin = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        UpdateAnteUI();
    }

    /// Call once per round after cinematic finishes, passing that round's total.
    public void ApplyRoundResult(int roundTotal)
    {
        currentRoundIndex++;
        cumulativeTotal += Mathf.Max(0, roundTotal);

        Debug.Log($"[ANTE] Round {currentRoundIndex}/{maxRounds}: +{roundTotal}  -> Total {cumulativeTotal}/{targetAnte}");

        // Early win before final round
        if (cumulativeTotal >= targetAnte && !earlyWin && currentRoundIndex < maxRounds)
        {
            earlyWin = true;
            Debug.Log("[ANTE] EARLY WIN: target reached before final round.");

            // Clear per-round dice since remaining rounds are skipped
            if (DiceStash.Instance)
            {
                DiceStash.Instance.ResetGenDiceList();
                DiceStash.Instance.ResetCurrStash();
            }
        }

        // After final round, just report result; Shop will handle reset/scale.
        if (currentRoundIndex >= maxRounds)
        {
            if (cumulativeTotal >= targetAnte)
                Debug.Log("[ANTE] WIN after final round.");
            else
                Debug.Log("[ANTE] LOSS: target not met after final round.");
        }
    }

    /// Call when entering Shop (after a loop or early win):
    /// scales difficulty for the NEXT loop and resets counters.
    public void HandleEnterShop()
    {
        // Exponential + random growth per loop
        int minIncrease = Mathf.RoundToInt(anteStep * Mathf.Pow(1.15f, currentLoop));
        int maxIncrease = Mathf.RoundToInt(anteStep * Mathf.Pow(1.30f, currentLoop));
        if (maxIncrease < minIncrease) maxIncrease = minIncrease;

        int increase = Random.Range(minIncrease, maxIncrease + 1);

        targetAnte += increase;
        currentLoop++;

        Debug.Log($"[ANTE] New Target = {targetAnte}  ( +{increase} )");

        UpdateAnteUI();
        ResetAnteRun();
    }

    /// Reset for a new loop (Round1 will start from zero).
    public void ResetAnteRun()
    {
        currentRoundIndex = 0;
        cumulativeTotal = 0;
        earlyWin = false;
        Debug.Log("[ANTE] Reset run.");
    }

    void UpdateAnteUI()
    {
        if (anteTMP) anteTMP.text = $"{antePrefix}{targetAnte}";
    }
}
