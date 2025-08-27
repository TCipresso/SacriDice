using UnityEngine;

public class SacrificeManager : MonoBehaviour
{
    public static SacrificeManager Instance { get; private set; }

    [Header("Simulate Sacrifice")]
    [Tooltip("How many general dice to add when simulating a sacrifice.")]
    public int testDiceCount = 2;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

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
