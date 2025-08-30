using UnityEngine;
using UnityEngine.EventSystems;

public class HoldToSpawnDice : MonoBehaviour, IPointerDownHandler
{
    public GameObject RollUI;      // assign your Roll UI object here
    public DiceRolling diceRolling;

    [Header("Camera Shake")]
    public CameraShakeSimple shaker;   // drag your CameraShakeSimple here

    bool hasSpawned = false;

    void Awake()
    {
        if (!diceRolling && Camera.main)
            diceRolling = Camera.main.GetComponent<DiceRolling>();

        if (!shaker && Camera.main)
            shaker = Camera.main.GetComponent<CameraShakeSimple>();
    }

    // Reset per round when this UI gets enabled again
    void OnEnable()
    {
        hasSpawned = false;

        // Disable camera shake component while this UI is active
        if (shaker) shaker.enabled = false;
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
