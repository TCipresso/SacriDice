using UnityEngine;
using System.Collections;

public class Round2State : MonoBehaviour
{
    [SerializeField] private float duration = 2f;

    public void Run()
    {
        var sm = RoundStateMachine.Instance;
        if (sm == null) return;

        if (sm.GetFlag(RoundStateMachine.RoundState.Round2)) return;

        sm.ResetAllFlags();
        sm.SetFlag(RoundStateMachine.RoundState.Round2, true);
        Debug.Log("Round 2");

        StartCoroutine(RunRoutine());
    }

    private IEnumerator RunRoutine()
    {
        yield return new WaitForSeconds(duration);
        RoundStateMachine.Instance.ChangeState(RoundStateMachine.RoundState.Round3);
    }
}
