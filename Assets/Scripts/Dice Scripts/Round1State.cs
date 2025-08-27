using UnityEngine;

public class Round1State : MonoBehaviour
{
    [Header("Round 1 UI")]
    public GameObject handPH;          
    public GameObject rollUI;          

    bool shownOnce;

    public void Run()
    {
        var sm = RoundStateMachine.Instance; if (!sm) return;
        if (sm.GetFlag(RoundStateMachine.RoundState.Round1)) return;

        sm.ResetAllFlags();
        sm.SetFlag(RoundStateMachine.RoundState.Round1, true);
        Debug.Log("Round 1");

        if (!shownOnce)
        {
            if (rollUI) rollUI.SetActive(false);
            if (handPH) handPH.SetActive(true);
            shownOnce = true;
        }
    }

    // Hook this to the Round 1 Hand button OnClick
    public void ConfirmHandAndShowRollUI()
    {
        SacrificeManager.Instance.SimulateSacrifice();
        if (handPH) handPH.SetActive(false);
        if (rollUI) rollUI.SetActive(true);
        //RoundStateMachine.Instance.rollUICanvasGroup.alpha = 1f;
    }
}
