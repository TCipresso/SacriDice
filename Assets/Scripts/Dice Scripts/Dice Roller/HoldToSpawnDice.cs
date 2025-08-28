using UnityEngine;
using UnityEngine.EventSystems;

public class HoldToSpawnDice : MonoBehaviour, IPointerDownHandler
{
    public GameObject RollUI;      // assign your Roll UI object here
    public DiceRolling diceRolling;

    bool hasSpawned = false;

    void Awake()
    {
        if (!diceRolling) diceRolling = Camera.main.GetComponent<DiceRolling>();
    }

    // Reset per round when this UI gets enabled again
    void OnEnable()
    {
        hasSpawned = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (hasSpawned) return;

        if (RollUI) RollUI.SetActive(false);

        if (diceRolling)
            diceRolling.SpawnFromStashAndBeginHold();

        hasSpawned = true;
    }
}
