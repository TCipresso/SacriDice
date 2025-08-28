using UnityEngine;
using System;
using System.Collections.Generic;

public enum HandSide { Left, Right }
public enum Finger { Thumb, Index, Middle, Ring, Pinky }

[Serializable]
public class HandState
{
    public bool handAvailable = true;
    public bool thumbAvailable = true;
    public bool indexAvailable = true;
    public bool middleAvailable = true;
    public bool ringAvailable = true;
    public bool pinkyAvailable = true;

    [NonSerialized] public bool handSelected;
    [NonSerialized] public bool thumbSelected;
    [NonSerialized] public bool indexSelected;
    [NonSerialized] public bool middleSelected;
    [NonSerialized] public bool ringSelected;
    [NonSerialized] public bool pinkySelected;

    [NonSerialized] public bool handStaked;
    [NonSerialized] public bool thumbStaked;
    [NonSerialized] public bool indexStaked;
    [NonSerialized] public bool middleStaked;
    [NonSerialized] public bool ringStaked;
    [NonSerialized] public bool pinkyStaked;

    public void ClearSelection()
    {
        handSelected = false;
        thumbSelected = false;
        indexSelected = false;
        middleSelected = false;
        ringSelected = false;
        pinkySelected = false;
    }

    public void ClearStake()
    {
        handStaked = false;
        thumbStaked = false;
        indexStaked = false;
        middleStaked = false;
        ringStaked = false;
        pinkyStaked = false;
    }
}

public class SacrificeSystem : MonoBehaviour
{
    public static SacrificeSystem Instance { get; private set; }

    public int dicePerFinger = 2;
    public int dicePerHand = 5;

    public HandState left = new HandState();
    public HandState right = new HandState();

    public int totalBonusDiceThisRound { get; private set; }
    public bool roundLocked { get; private set; }

    public event Action OnSelectionChanged;
    public event Action<int> OnSacrificeCommitted;
    public event Action<bool> OnCollateralResolved;
    public event Action OnRoundStart;
    public event Action OnRoundCanceled;
    public event Action OnGameOver;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void BeginSacrificeRound()
    {
        roundLocked = false;
        totalBonusDiceThisRound = 0;
        left.ClearSelection();
        right.ClearSelection();
        OnRoundStart?.Invoke();
        OnSelectionChanged?.Invoke();
    }

    public void CancelSelection()
    {
        if (roundLocked) return;
        left.ClearSelection();
        right.ClearSelection();
        OnRoundCanceled?.Invoke();
        OnSelectionChanged?.Invoke();
    }

    public void ToggleFinger(HandSide side, Finger finger)
    {
        if (roundLocked) return;
        var h = side == HandSide.Left ? left : right;
        if (!IsFingerAvailable(h, finger)) return;
        if (IsFingerStaked(h, finger)) return;
        if (IsHandSelected(h) || h.handStaked) return;
        SetFingerSelected(h, finger, !IsFingerSelected(h, finger));
        OnSelectionChanged?.Invoke();
    }

    public void ToggleHand(HandSide side)
    {
        if (roundLocked) return;
        var h = side == HandSide.Left ? left : right;
        if (!h.handAvailable || h.handStaked) return;
        bool newState = !h.handSelected;
        h.handSelected = newState;
        if (newState)
        {
            h.thumbSelected = false;
            h.indexSelected = false;
            h.middleSelected = false;
            h.ringSelected = false;
            h.pinkySelected = false;
        }
        OnSelectionChanged?.Invoke();
    }

    public void CommitSacrifice()
    {
        if (roundLocked) return;
        if (!IsAnySelectionActive()) return;
        StakeFromSelection(left);
        StakeFromSelection(right);
        totalBonusDiceThisRound = GetStakedBonus();
        left.ClearSelection();
        right.ClearSelection();
        roundLocked = true;
        OnSacrificeCommitted?.Invoke(totalBonusDiceThisRound);
        OnSelectionChanged?.Invoke();
    }

    public void ResolveCollateralWin()
    {
        ReturnStake(left);
        ReturnStake(right);
        totalBonusDiceThisRound = 0;
        roundLocked = false;
        OnCollateralResolved?.Invoke(true);
        OnSelectionChanged?.Invoke();
    }

    public bool ResolveCollateralLose()
    {
        ForfeitStake(left);
        ForfeitStake(right);
        totalBonusDiceThisRound = 0;
        roundLocked = false;
        OnCollateralResolved?.Invoke(false);
        OnSelectionChanged?.Invoke();
        if (!AnyFingerAvailable()) { OnGameOver?.Invoke(); return true; }
        return false;
    }

    public int GetProjectedBonus()
    {
        int bonus = 0;
        bonus += GetHandProjected(left);
        bonus += GetHandProjected(right);
        return bonus;
    }

    int GetHandProjected(HandState h)
    {
        if (h.handSelected && h.handAvailable && !h.handStaked) return dicePerHand;
        int count = 0;
        if (h.thumbSelected && h.thumbAvailable && !h.thumbStaked) count++;
        if (h.indexSelected && h.indexAvailable && !h.indexStaked) count++;
        if (h.middleSelected && h.middleAvailable && !h.middleStaked) count++;
        if (h.ringSelected && h.ringAvailable && !h.ringStaked) count++;
        if (h.pinkySelected && h.pinkyAvailable && !h.pinkyStaked) count++;
        return count * dicePerFinger;
    }

    int GetStakedBonus()
    {
        int b = 0;
        b += GetHandStaked(left);
        b += GetHandStaked(right);
        return b;
    }

    int GetHandStaked(HandState h)
    {
        if (h.handStaked) return dicePerHand;
        int c = 0;
        if (h.thumbStaked) c++;
        if (h.indexStaked) c++;
        if (h.middleStaked) c++;
        if (h.ringStaked) c++;
        if (h.pinkyStaked) c++;
        return c * dicePerFinger;
    }

    void StakeFromSelection(HandState h)
    {
        if (h.handSelected && h.handAvailable && !h.handStaked)
        {
            h.handStaked = true;
            h.thumbStaked = true;
            h.indexStaked = true;
            h.middleStaked = true;
            h.ringStaked = true;
            h.pinkyStaked = true;
            return;
        }
        if (h.thumbSelected && h.thumbAvailable && !h.thumbStaked) h.thumbStaked = true;
        if (h.indexSelected && h.indexAvailable && !h.indexStaked) h.indexStaked = true;
        if (h.middleSelected && h.middleAvailable && !h.middleStaked) h.middleStaked = true;
        if (h.ringSelected && h.ringAvailable && !h.ringStaked) h.ringStaked = true;
        if (h.pinkySelected && h.pinkyAvailable && !h.pinkyStaked) h.pinkyStaked = true;
    }

    void ReturnStake(HandState h)
    {
        h.ClearStake();
    }

    void ForfeitStake(HandState h)
    {
        if (h.handStaked)
        {
            h.handAvailable = false;
            h.thumbAvailable = false;
            h.indexAvailable = false;
            h.middleAvailable = false;
            h.ringAvailable = false;
            h.pinkyAvailable = false;
            h.ClearStake();
            return;
        }
        if (h.thumbStaked) { h.thumbAvailable = false; h.thumbStaked = false; }
        if (h.indexStaked) { h.indexAvailable = false; h.indexStaked = false; }
        if (h.middleStaked) { h.middleAvailable = false; h.middleStaked = false; }
        if (h.ringStaked) { h.ringAvailable = false; h.ringStaked = false; }
        if (h.pinkyStaked) { h.pinkyAvailable = false; h.pinkyStaked = false; }
    }

    bool IsFingerAvailable(HandState h, Finger f)
    {
        switch (f)
        {
            case Finger.Thumb: return h.thumbAvailable;
            case Finger.Index: return h.indexAvailable;
            case Finger.Middle: return h.middleAvailable;
            case Finger.Ring: return h.ringAvailable;
            case Finger.Pinky: return h.pinkyAvailable;
        }
        return false;
    }

    bool IsFingerSelected(HandState h, Finger f)
    {
        switch (f)
        {
            case Finger.Thumb: return h.thumbSelected;
            case Finger.Index: return h.indexSelected;
            case Finger.Middle: return h.middleSelected;
            case Finger.Ring: return h.ringSelected;
            case Finger.Pinky: return h.pinkySelected;
        }
        return false;
    }

    bool IsFingerStaked(HandState h, Finger f)
    {
        switch (f)
        {
            case Finger.Thumb: return h.thumbStaked;
            case Finger.Index: return h.indexStaked;
            case Finger.Middle: return h.middleStaked;
            case Finger.Ring: return h.ringStaked;
            case Finger.Pinky: return h.pinkyStaked;
        }
        return false;
    }

    void SetFingerSelected(HandState h, Finger f, bool v)
    {
        switch (f)
        {
            case Finger.Thumb: h.thumbSelected = v; break;
            case Finger.Index: h.indexSelected = v; break;
            case Finger.Middle: h.middleSelected = v; break;
            case Finger.Ring: h.ringSelected = v; break;
            case Finger.Pinky: h.pinkySelected = v; break;
        }
    }

    bool IsHandSelected(HandState h)
    {
        return h.handSelected;
    }

    bool AnyFingerAvailable()
    {
        if (left.thumbAvailable || left.indexAvailable || left.middleAvailable || left.ringAvailable || left.pinkyAvailable) return true;
        if (right.thumbAvailable || right.indexAvailable || right.middleAvailable || right.ringAvailable || right.pinkyAvailable) return true;
        return false;
    }

    public bool IsAnySelectionActive()
    {
        if (left.handSelected || right.handSelected) return true;
        if (left.thumbSelected || left.indexSelected || left.middleSelected || left.ringSelected || left.pinkySelected) return true;
        if (right.thumbSelected || right.indexSelected || right.middleSelected || right.ringSelected || right.pinkySelected) return true;
        return false;
    }
}
