using UnityEngine;

public class RollUIBuilder : MonoBehaviour
{
    public Transform uiParent;

    void OnEnable()
    {
        if (DiceStash.Instance != null && uiParent != null)
        {
            DiceStash.Instance.BuildUI(uiParent);
        }
    }
}
