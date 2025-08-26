using System;
using System.Collections.Generic;
using UnityEngine;

public class DiceRollManager : MonoBehaviour
{
    public static DiceRollManager Instance { get; private set; }

    [Header("Active dice in current roll")]
    public List<DiceTopReader> active = new List<DiceTopReader>();

    [Header("Totals")]
    public int totalInt;
    public string totalDisplay;

    [Header("Cinematic")]
    public bool autoPlayCinematic = true;                 // auto-run camera sequence when all dice are settled
    public DiceCinematicSequencer sequencerOverride;      // optional explicit reference; if null we FindObjectOfType

    public event Action AllDiceSettled;                   // subscribe if other systems need to react

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Register(DiceTopReader d)
    {
        if (d && !active.Contains(d)) active.Add(d);
    }

    public void Unregister(DiceTopReader d)
    {
        if (d) active.Remove(d);
    }

    public void OnDieSettled(DiceTopReader d)
    {
        RecomputeTotals();

        Debug.Log($"[DiceRollManager] Die settled: {d.resultValue}  | Total: {totalInt} ({totalDisplay})");

        if (AllSettled())
        {
            Debug.Log("[DiceRollManager] All dice settled.");

            // notify listeners first
            AllDiceSettled?.Invoke();

            // optionally kick off the cinematic
            if (autoPlayCinematic)
            {
                var seq = sequencerOverride ? sequencerOverride : FindObjectOfType<DiceCinematicSequencer>();
                if (seq) seq.PlayComputedForSettled();
                else Debug.LogWarning("[DiceRollManager] No DiceCinematicSequencer found to play.");
            }
        }
    }

    public void RecomputeTotals()
    {
        int sum = 0;
        List<string> parts = new List<string>();

        foreach (var d in active)
        {
            if (!d || !d.isSettled) continue;
            parts.Add(d.resultValue);
            if (int.TryParse(d.resultValue, out var v)) sum += v;
        }

        totalInt = sum;
        totalDisplay = string.Join("+", parts);
    }

    public bool AllSettled()
    {
        foreach (var d in active)
        {
            if (!d || !d.gameObject.activeInHierarchy) continue;
            if (!d.isSettled) return false;
        }
        return true;
    }

    public void ClearAll()
    {
        active.Clear();
        totalInt = 0;
        totalDisplay = "";
    }
}
