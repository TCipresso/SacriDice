using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    public static PlayerStateMachine Instance { get; private set; }

    public enum State { Active, InActive, Pause }
    public State CurrentState { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        SetState(State.Active);
    }

    public void SetState(State newState)
    {
        CurrentState = newState;

        switch (newState)
        {
            case State.Active:
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                Time.timeScale = 1f;
                break;

            case State.InActive:
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                Time.timeScale = 1f;
                break;

            case State.Pause:
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                Time.timeScale = 0f;
                break;
        }
    }

    // Optional quick methods
    public void SetActive() => SetState(State.Active);
    public void SetInactive() => SetState(State.InActive);
    public void SetPause() => SetState(State.Pause);
}
