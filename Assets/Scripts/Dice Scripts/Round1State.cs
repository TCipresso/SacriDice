using UnityEngine;

public class Round1State : MonoBehaviour
{
    public GameObject handPH;
    public GameObject rollUI;
    public GameObject commitButton;

    bool shownOnce;

    public void Run()
    {
        Debug.Log("Round1");
        var sm = RoundStateMachine.Instance; if (!sm) return;
        if (sm.GetFlag(RoundStateMachine.RoundState.Round1)) return;

        sm.ResetAllFlags();
        sm.SetFlag(RoundStateMachine.RoundState.Round1, true);

        // Always put Round 1 UI into the correct visible state on entry
        if (rollUI) rollUI.SetActive(false);
        if (handPH) handPH.SetActive(true);
        if (commitButton) commitButton.SetActive(true);

        if (!shownOnce)
        {
            // per-round setup
            if (SacrificeManager2.Instance)
            {
                SacrificeManager2.Instance.ResetRoundCommitTotals();
                SacrificeManager2.Instance.RebuildLists();
                SacrificeManager2.Instance.EnforceCommittedDisabled();
            }

            // handle loop win carryover, if you still use this
            if (AnteManager.Instance && AnteManager.Instance.lastLoopWin)
            {
                SacrificeSystem.Instance.ResolveCollateralWin();
                AnteManager.Instance.lastLoopWin = false;
            }

            shownOnce = true;
        }
    }

    /// <summary>
    /// BUTTON: Use this as the Sacrifice button hook for Round 1.
    /// Recommended OnClick order:
    ///   1) SacrificeManager2.CommitSelected
    ///   2) Round1State.Round1_SacrificePressed
    /// </summary>
    public void Round1_SacrificePressed()
    {
        var mgr = SacrificeManager2.Instance;

        // Allow if we either still have a selection OR a commit just priced something.
        bool justCommitted = mgr != null && (mgr.lastDiceValue != 0 || mgr.lastCoinsValue != 0 || mgr.lastHealthValue != 0);
        bool hasSelection = mgr != null && mgr.SelectedSac != null && mgr.SelectedSac.Count > 0;

        if (!justCommitted && !hasSelection)
        {
            Debug.LogWarning("[Sacrifice] You need to select a sacrifice first.");
            return;
        }

        // Hide Round 1 UI immediately
        if (handPH) handPH.SetActive(false);
        if (commitButton) commitButton.SetActive(false);

        // Re-enable camera shake script (HoldToSpawnDice turned it off on enable)
        CameraShakeSimple shaker = null;
        if (HurtSequence.Instance && HurtSequence.Instance.shaker)
            shaker = HurtSequence.Instance.shaker;
        else if (Camera.main)
            shaker = Camera.main.GetComponent<CameraShakeSimple>();
        if (shaker) shaker.enabled = true;

        // Delegate to the cinematic entry point (keeps your central flow)
        if (HurtSequence.Instance != null)
            HurtSequence.Instance.OnSacrificePressed();
        else
            Debug.LogWarning("[Round1] HurtSequence.Instance is null.");
    }

    // (Optional) Keep if you still use a separate 'show roll UI' path later
    public void ConfirmHandAndShowRollUI()
    {
        var mgr = SacrificeManager2.Instance;
        bool justCommitted = mgr != null && (mgr.lastDiceValue != 0 || mgr.lastCoinsValue != 0 || mgr.lastHealthValue != 0);
        bool hasSelection = mgr != null && mgr.SelectedSac != null && mgr.SelectedSac.Count > 0;

        if (!justCommitted && !hasSelection)
        {
            Debug.LogWarning("[Sacrifice] You need to select a sacrifice first.");
            return;
        }

        if (handPH) handPH.SetActive(false);
        if (commitButton) commitButton.SetActive(false);
        if (rollUI) rollUI.SetActive(true);
    }

    public void ResetRoundUI()
    {
        shownOnce = false;
        if (handPH) handPH.SetActive(false);
        if (commitButton) commitButton.SetActive(false);
        if (rollUI) rollUI.SetActive(false);
    }
}
