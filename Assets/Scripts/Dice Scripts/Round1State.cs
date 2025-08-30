using UnityEngine;

public class Round1State : MonoBehaviour
{
    public GameObject handPH;
    public GameObject rollUI;
    public GameObject commitButton;

    bool shownOnce;

    public void Run()
    {
        var sm = RoundStateMachine.Instance; if (!sm) return;
        if (sm.GetFlag(RoundStateMachine.RoundState.Round1)) return;

        sm.ResetAllFlags();
        sm.SetFlag(RoundStateMachine.RoundState.Round1, true);

        if (!shownOnce)
        {
            if (rollUI) rollUI.SetActive(false);

            // per-round setup
            SacrificeManager2.Instance.ResetRoundCommitTotals();
            SacrificeManager2.Instance.RebuildLists();
            SacrificeManager2.Instance.EnforceCommittedDisabled();

            // handle loop win carryover, if you still use this
            if (AnteManager.Instance && AnteManager.Instance.lastLoopWin)
            {
                SacrificeSystem.Instance.ResolveCollateralWin();
                AnteManager.Instance.lastLoopWin = false;
            }

            if (handPH) handPH.SetActive(true);
            if (commitButton) commitButton.SetActive(true);
            shownOnce = true;
        }
    }

    // Button calls CommitSelected FIRST, then this method.
    public void ConfirmHandAndShowRollUI()
    {
        var mgr = SacrificeManager2.Instance;

        // allow transition if a commit just priced anything OR if selection still exists
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
