using UnityEngine;

public class Round2State : MonoBehaviour
{
    [Header("Round 2 UI")]
    public GameObject handPH;
    public GameObject rollUI;

    bool shownOnce;

    public void Run()
    {
        var sm = RoundStateMachine.Instance; if (!sm) return;
        if (sm.GetFlag(RoundStateMachine.RoundState.Round2)) return;

        sm.ResetAllFlags();
        sm.SetFlag(RoundStateMachine.RoundState.Round2, true);
        Debug.Log("Round 2");

        if (!shownOnce)
        {
            if (rollUI) rollUI.SetActive(false);
            if (handPH) handPH.SetActive(true);
            shownOnce = true;
        }
    }

    // Hook this to the Round 2 Hand button OnClick
    public void ConfirmHandAndShowRollUI()
    {
        SacrificeManager.Instance.SimulateSacrifice();
        if (handPH) handPH.SetActive(false);
        if (rollUI) rollUI.SetActive(true);
        //RoundStateMachine.Instance.rollUICanvasGroup.alpha = 1f;
    }
}
