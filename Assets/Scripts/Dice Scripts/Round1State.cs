using UnityEngine;

public class Round1State : MonoBehaviour
{
    public GameObject handPH;
    public GameObject rollUI;

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
            if (handPH) handPH.SetActive(true);
            shownOnce = true;
        }
    }

    public void ConfirmHandAndShowRollUI()
    {
        if (handPH) handPH.SetActive(false);
        if (rollUI) rollUI.SetActive(true);
    }

    public void ResetRoundUI()
    {
        shownOnce = false;
        if (handPH) handPH.SetActive(false);
        if (rollUI) rollUI.SetActive(false);
    }
}
