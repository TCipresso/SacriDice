using UnityEngine;
using UnityEngine.EventSystems;

public class HoldToSpawnDice : MonoBehaviour, IPointerDownHandler
{
    public CanvasGroup uiGroup;
    public DiceRolling diceRolling;

    bool hasSpawned = false;

    void Awake()
    {
        if (!diceRolling) diceRolling = Camera.main.GetComponent<DiceRolling>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (hasSpawned) return;

        if (uiGroup)
        {
            uiGroup.alpha = 0f;
            uiGroup.interactable = false;
            uiGroup.blocksRaycasts = false;
        }

        if (diceRolling)
            diceRolling.SpawnFromStashAndBeginHold();

        hasSpawned = true;
    }
}
