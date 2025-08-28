using UnityEngine;

public class Round2State : MonoBehaviour
{
    public GameObject handPH;
    public GameObject rollUI;
    public GameObject commitButton;

    bool shownOnce;

    public void Run()
    {
        var sm = RoundStateMachine.Instance; if (!sm) return;
        if (sm.GetFlag(RoundStateMachine.RoundState.Round2)) return;

        sm.ResetAllFlags();
        sm.SetFlag(RoundStateMachine.RoundState.Round2, true);

        if (!shownOnce)
        {
            if (rollUI) rollUI.SetActive(false);

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

    public void ConfirmHandAndShowRollUI()
    {
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
