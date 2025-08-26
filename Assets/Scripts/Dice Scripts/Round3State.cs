using UnityEngine;
using System.Collections;

public class Round3State : MonoBehaviour
{
    [SerializeField] private float duration = 2f;

    public void Run()
    {
        var sm = RoundStateMachine.Instance;
        if (sm == null) return;

        if (sm.GetFlag(RoundStateMachine.RoundState.Round3)) return;

        sm.ResetAllFlags();
        sm.SetFlag(RoundStateMachine.RoundState.Round3, true);
        Debug.Log("Round 3");

        StartCoroutine(RunRoutine());
    }

    private IEnumerator RunRoutine()
    {
        yield return new WaitForSeconds(duration);
        RoundStateMachine.Instance.ChangeState(RoundStateMachine.RoundState.Shop);
    }
}
