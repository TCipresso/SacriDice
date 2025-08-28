using UnityEngine;

public class RollUIBuilder : MonoBehaviour
{
    public Transform uiParent;

    void OnEnable()
    {
        if (DiceStash.Instance == null || uiParent == null) return;

        // Merge BoughtDiceList + CurrGenDiceList into CurrStash for this round
        DiceStash.Instance.RebuildCurrStash();

        // Now build the UI from CurrStash
        DiceStash.Instance.BuildUI(uiParent);
    }
}
