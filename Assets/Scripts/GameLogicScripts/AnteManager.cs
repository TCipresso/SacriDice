using System;
using UnityEngine;
using TMPro;

public class AnteManager : MonoBehaviour
{
    public static AnteManager Instance { get; private set; }

    [Header("Ante Rules")]
    public int targetAnte = 32;
    public int maxRounds = 3;
    public int currentLoop = 0;

    [Header("Scaling")]
    public int anteStep = 8;

    [Header("UI")]
    public TextMeshProUGUI anteTMP;
    public string antePrefix = "Ante: ";

    [Header("Runtime")]
    public int currentRoundIndex = 0;
    public int cumulativeTotal = 0;
    public bool earlyWin = false;
    public bool loopResolved = false;

    public event Action<bool> OnLoopResolved;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        UpdateAnteUI();
    }

    public void ApplyRoundResult(int roundTotal)
    {
        if (loopResolved) return;

        currentRoundIndex++;
        cumulativeTotal += Mathf.Max(0, roundTotal);

        if (cumulativeTotal >= targetAnte && !earlyWin && currentRoundIndex < maxRounds)
        {
            earlyWin = true;
            ResolveWinLoop();
            return;
        }

        if (currentRoundIndex >= maxRounds && !loopResolved)
        {
            if (cumulativeTotal >= targetAnte) ResolveWinLoop();
            else ResolveLossLoop();
        }
    }

    public void HandleEnterShop()
    {
        int minIncrease = Mathf.RoundToInt(anteStep * Mathf.Pow(1.15f, currentLoop));
        int maxIncrease = Mathf.RoundToInt(anteStep * Mathf.Pow(1.30f, currentLoop));
        if (maxIncrease < minIncrease) maxIncrease = minIncrease;
        int increase = UnityEngine.Random.Range(minIncrease, maxIncrease + 1);

        targetAnte += increase;
        currentLoop++;

        UpdateAnteUI();
        ResetAnteRun();
    }

    public void ResetAnteRun()
    {
        currentRoundIndex = 0;
        cumulativeTotal = 0;
        earlyWin = false;
        loopResolved = false;
    }

    // AnteManager.cs
    public bool lastLoopWin;

    void ResolveWinLoop()
    {
        loopResolved = true;
        lastLoopWin = true;

        // NEW: grow everything back
        if (SacrificeManager2.Instance != null)
            SacrificeManager2.Instance.RestoreAllCommittedToHome();

        // Clear per-round dice UI/stash as you already had
        if (DiceStash.Instance != null)
        {
            DiceStash.Instance.ResetGenDiceList();
            DiceStash.Instance.ResetCurrStash();
        }

        OnLoopResolved?.Invoke(true);
        Debug.Log("[ANTE] WIN");
    }


    void ResolveLossLoop()
    {
        loopResolved = true;
        lastLoopWin = false;

        // Move everything currently committed into the permanent-loss bank and disable them.
        if (SacrificeManager2.Instance != null)
            SacrificeManager2.Instance.BankCommittedToPermanentLoss();

        // (Optional) If you still need game-over logic from your older system, keep this:
        // bool gameOver = false;
        // if (SacrificeSystem.Instance != null) gameOver = SacrificeSystem.Instance.ResolveCollateralLose();

        // Clear per-round dice/stash UI
        if (DiceStash.Instance != null)
        {
            DiceStash.Instance.ResetGenDiceList();
            DiceStash.Instance.ResetCurrStash();
        }

        OnLoopResolved?.Invoke(false);
        Debug.Log("[ANTE] LOSS");
    }



    void UpdateAnteUI()
    {
        if (anteTMP) anteTMP.text = $"{antePrefix}{targetAnte}";
    }
}
