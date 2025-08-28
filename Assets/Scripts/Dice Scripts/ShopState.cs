using UnityEngine;

public class ShopState : MonoBehaviour
{
    [Header("Shop UI Root")]
    public GameObject ShopBase;

    public void Run()
    {
        var sm = RoundStateMachine.Instance;
        if (!sm) return;

        if (sm.GetFlag(RoundStateMachine.RoundState.Shop)) return;

        sm.ResetAllFlags();
        sm.SetFlag(RoundStateMachine.RoundState.Shop, true);

        Debug.Log("Shop");

        if (ShopBase) ShopBase.SetActive(true);

        AnteManager.Instance?.HandleEnterShop();
    }

    public void ResetShop()
    {
        if (ShopBase) ShopBase.SetActive(false);
        RoundStateMachine.Instance?.ChangeStateStore();
    }
}
