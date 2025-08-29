using UnityEngine;
using TMPro;

public class PotManager : MonoBehaviour
{
    public static PotManager Instance { get; private set; }

    [Header("UI")]
    public TextMeshProUGUI potTMP;
    public string potPrefix = "Pot: $ ";

    [Header("Runtime")]
    public int CurrentPot { get; private set; } = 0;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        RefreshUI();
    }

    void OnEnable()
    {
        if (AnteManager.Instance != null)
            AnteManager.Instance.OnLoopResolved += HandleLoopResolved;
    }

    void OnDisable()
    {
        if (AnteManager.Instance != null)
            AnteManager.Instance.OnLoopResolved -= HandleLoopResolved;
    }

    /// <summary>
    /// Call this immediately AFTER SacrificeManager2.CommitSelected().
    /// Adds the last committed coins into the pot.
    /// </summary>
    public void AddFromLastCommit()
    {
        int add = 0;
        if (SacrificeManager2.Instance != null)
            add = Mathf.Max(0, SacrificeManager2.Instance.lastCoinsValue);

        if (add > 0)
        {
            CurrentPot += add;
            RefreshUI();
        }
    }

    /// <summary>
    /// Optional: direct add if you want to feed a number.
    /// </summary>
    public void AddToPot(int coins)
    {
        if (coins <= 0) return;
        CurrentPot += coins;
        RefreshUI();
    }

    /// <summary>
    /// Clears the pot (used after a win/loss or when starting a new loop).
    /// </summary>
    public void ResetPot()
    {
        CurrentPot = 0;
        RefreshUI();
    }

    void HandleLoopResolved(bool win)
    {
        if (win && WalletManager.Instance != null && CurrentPot > 0)
            WalletManager.Instance.AddCash(CurrentPot);

        // Always reset for the next loop, win or lose
        ResetPot();
    }

    void RefreshUI()
    {
        if (potTMP != null)
            potTMP.text = $"{potPrefix}{CurrentPot}";
    }
}
