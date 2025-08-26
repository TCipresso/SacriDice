using UnityEngine;
using System.Collections;

public class ShopState : MonoBehaviour
{
    [SerializeField] private float duration = 3f;

    public void Run()
    {
        var sm = RoundStateMachine.Instance;
        if (sm == null) return;

        if (sm.GetFlag(RoundStateMachine.RoundState.Shop)) return;

        sm.ResetAllFlags();
        sm.SetFlag(RoundStateMachine.RoundState.Shop, true);
        Debug.Log("Shop");

        // If you want the shop UI to appear here, it should already happen via your ShopManager enable.
        //StartCoroutine(RunRoutine());
    }

    private IEnumerator RunRoutine()
    {
        yield return new WaitForSeconds(duration);
        RoundStateMachine.Instance.ChangeState(RoundStateMachine.RoundState.Round1);
    }
}
