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

        // Early WIN (unchanged)
        if (cumulativeTotal >= targetAnte && !earlyWin && currentRoundIndex < maxRounds)
        {
            earlyWin = true;
            ResolveWinLoop();
            return;
        }

        // ---- EARLY LOSS detection ----
        // If we have NOT met the target, are BEFORE final round,
        // and there is NO collateral left to bet -> lose immediately.
        if (cumulativeTotal < targetAnte && currentRoundIndex < maxRounds)
        {
            int available = 0;
            if (SacrificeManager2.Instance != null)
                available = (SacrificeManager2.Instance.LeftHand.Count + SacrificeManager2.Instance.RightHand.Count);

            if (available <= 0)
            {
                Debug.Log("[ANTE] EARLY LOSS: no collateral remaining.");
                ResolveLossLoop();
                SacrificeManager2.Instance.gameOver.SetActive(true);
                return;
            }
        }
        // ---- end EARLY LOSS ----

        // Final round resolution
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

        // Grow everything back
        if (SacrificeManager2.Instance != null)
            SacrificeManager2.Instance.RestoreAllCommittedToHome();

        // PAYOUT POT -> WALLET, then RESET POT
        if (PotManager.Instance != null)
        {
            int payout = PotManager.Instance.CurrentPot;
            if (payout > 0 && WalletManager.Instance != null)
                WalletManager.Instance.AddCash(payout);

            PotManager.Instance.ResetPot();
        }

        // Clear per-round dice UI/stash
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

        // RESET POT WITHOUT PAYOUT
        if (PotManager.Instance != null)
            PotManager.Instance.ResetPot();

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
