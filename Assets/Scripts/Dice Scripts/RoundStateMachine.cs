using UnityEngine;

public class RoundStateMachine : MonoBehaviour
{
    public static RoundStateMachine Instance { get; private set; }

    public enum RoundState
    {
        Round1,
        Round2,
        Round3,
        Shop
    }

    public RoundState currentState;

    public Round1State round1State;
    public Round2State round2State;
    public Round3State round3State;
    public ShopState shopState;
    //public CanvasGroup rollUICanvasGroup;

    [Header("State Flags")]
    public bool round1Flag;
    public bool round2Flag;
    public bool round3Flag;
    public bool shopFlag;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Update()
    {
        RunCurrentState();
    }

    private void RunCurrentState()
    {
        switch (currentState)
        {
            case RoundState.Round1:
                round1State?.Run();
                break;
            case RoundState.Round2:
                round2State?.Run();
                break;
            case RoundState.Round3:
                round3State?.Run();
                break;
            case RoundState.Shop:
                shopState?.Run();
                break;
        }
    }

    public void ChangeState(RoundState newState)
    {
        currentState = newState;
        PrepRoundUIFor(newState);   // <-- reset shownOnce + set both UIs inactive for that round
    }

    public void ResetAllFlags()
    {
        round1Flag = round2Flag = round3Flag = shopFlag = false;
    }

    public void SetFlag(RoundState state, bool value)
    {
        switch (state)
        {
            case RoundState.Round1: round1Flag = value; break;
            case RoundState.Round2: round2Flag = value; break;
            case RoundState.Round3: round3Flag = value; break;
            case RoundState.Shop: shopFlag = value; break;
        }
    }

    public bool GetFlag(RoundState state)
    {
        return state switch
        {
            RoundState.Round1 => round1Flag,
            RoundState.Round2 => round2Flag,
            RoundState.Round3 => round3Flag,
            RoundState.Shop => shopFlag,
            _ => false
        };
    }

    public void ChangeStateStore()
    {
        ChangeState(RoundState.Round1);
    }



    public RoundState GetNextState()
    {
        // Clear dice lists before moving to the next state
        if (DiceStash.Instance != null)
        {
            DiceStash.Instance.ResetGenDiceList();
            DiceStash.Instance.ResetCurrStash();
        }

        return currentState switch
        {
            RoundState.Round1 => RoundState.Round2,
            RoundState.Round2 => RoundState.Round3,
            RoundState.Round3 => RoundState.Shop,
            RoundState.Shop => RoundState.Round1,
            _ => currentState
        };
    }

    void PrepRoundUIFor(RoundState s)
    {
        switch (s)
        {
            case RoundState.Round1: round1State?.ResetRoundUI(); break;
            case RoundState.Round2: round2State?.ResetRoundUI(); break;
            case RoundState.Round3: round3State?.ResetRoundUI(); break;
                // Shop has its own UI flow
        }
    }


}