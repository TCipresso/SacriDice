using UnityEngine;

public class SacrificeManager : MonoBehaviour
{
    [Header("Simulate Sacrifice")]
    [Tooltip("How many general dice to add when simulating a sacrifice.")]
    public int testDiceCount = 2;

    [ContextMenu("Simulate Sacrifice")]
    public void SimulateSacrifice()
    {
        if (DiceStash.Instance == null)
        {
            Debug.LogWarning("[SacrificeManager] No DiceStash instance found!");
            return;
        }

        DiceStash.Instance.AddGenDice(testDiceCount);
        Debug.Log($"[SacrificeManager] Added {testDiceCount} general dice to CurrGenDiceList.");
    }
}
