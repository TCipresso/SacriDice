using UnityEngine;

public class Round2State : MonoBehaviour
{
    public GameObject handPH;
    public GameObject rollUI;
    public GameObject commitButton;

    bool shownOnce;

    public void Run()
    {
        Debug.Log("Round2");
        var sm = RoundStateMachine.Instance; if (!sm) return;
        if (sm.GetFlag(RoundStateMachine.RoundState.Round2)) return;

        sm.ResetAllFlags();
        sm.SetFlag(RoundStateMachine.RoundState.Round2, true);

        if (rollUI) rollUI.SetActive(false);
        if (handPH) handPH.SetActive(true);
        if (commitButton) commitButton.SetActive(true);

        if (!shownOnce)
        {
            if (SacrificeManager2.Instance)
            {
                SacrificeManager2.Instance.ResetRoundCommitTotals();
                SacrificeManager2.Instance.RebuildLists();
                SacrificeManager2.Instance.EnforceCommittedDisabled();
            }
            shownOnce = true;
        }
    }

    // OnClick order: 1) SacrificeManager2.CommitSelected  2) Round2_SacrificePressed
    public void Round2_SacrificePressed()
    {
        var mgr = SacrificeManager2.Instance;
        bool justCommitted = mgr != null && (mgr.lastDiceValue != 0 || mgr.lastCoinsValue != 0 || mgr.lastHealthValue != 0);
        bool hasSelection = mgr != null && mgr.SelectedSac != null && mgr.SelectedSac.Count > 0;

        if (!justCommitted && !hasSelection)
        {
            Debug.LogWarning("[Round2] You need to select a sacrifice first.");
            return;
        }

        // Hide Round 2 UI immediately
        if (handPH) handPH.SetActive(false);
        if (commitButton) commitButton.SetActive(false);

        // Re-enable camera shake script (HoldToSpawnDice turned it off on enable)
        CameraShakeSimple shaker = null;
        if (HurtSequence.Instance && HurtSequence.Instance.shaker)
            shaker = HurtSequence.Instance.shaker;
        else if (Camera.main)
            shaker = Camera.main.GetComponent<CameraShakeSimple>();
        if (shaker) shaker.enabled = true;

        // Kick the cinematic entry
        if (HurtSequence.Instance) HurtSequence.Instance.OnSacrificePressed();
    }

    public void ResetRoundUI()
    {
        shownOnce = false;
        if (handPH) handPH.SetActive(false);
        if (commitButton) commitButton.SetActive(false);
        if (rollUI) rollUI.SetActive(false);
    }
}
