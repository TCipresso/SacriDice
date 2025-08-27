using UnityEngine;

public class Round3State : MonoBehaviour
{
    [Header("Round 3 UI")]
    public GameObject handPH;
    public GameObject rollUI;

    bool shownOnce;

    public void Run()
    {
        var sm = RoundStateMachine.Instance; if (!sm) return;
        if (sm.GetFlag(RoundStateMachine.RoundState.Round3)) return;

        sm.ResetAllFlags();
        sm.SetFlag(RoundStateMachine.RoundState.Round3, true);
        Debug.Log("Round 3");

        if (!shownOnce)
        {
            if (rollUI) rollUI.SetActive(false);
            if (handPH) handPH.SetActive(true);
            shownOnce = true;
        }
    }

    // Hook this to the Round 3 Hand button OnClick
    public void ConfirmHandAndShowRollUI()
    {
        SacrificeManager.Instance.SimulateSacrifice();
        if (handPH) handPH.SetActive(false);
        if (rollUI) rollUI.SetActive(true);
        //RoundStateMachine.Instance.rollUICanvasGroup.alpha = 1f;
    }

    public void ResetRoundUI()
    {
        shownOnce = false;
        if (handPH) handPH.SetActive(false);
        if (rollUI) rollUI.SetActive(false);
    }
}
