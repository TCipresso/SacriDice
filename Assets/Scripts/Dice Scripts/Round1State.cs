using UnityEngine;
using System.Collections;

public class Round1State : MonoBehaviour
{
    [SerializeField] private float duration = 2f;

    public void Run()
    {
        var sm = RoundStateMachine.Instance;
        if (sm == null) return;

        // already running this state? then do nothing
        if (sm.GetFlag(RoundStateMachine.RoundState.Round1)) return;

        sm.ResetAllFlags();
        sm.SetFlag(RoundStateMachine.RoundState.Round1, true);
        Debug.Log("Round 1");

        StartCoroutine(RunRoutine());
    }

    private IEnumerator RunRoutine()
    {
        yield return new WaitForSeconds(duration);
        RoundStateMachine.Instance.ChangeState(RoundStateMachine.RoundState.Round2);
    }
}
