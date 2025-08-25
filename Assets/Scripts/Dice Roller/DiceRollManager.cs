using System.Collections.Generic;
using UnityEngine;

public class DiceRollManager : MonoBehaviour
{
    public static DiceRollManager Instance { get; private set; }
    public List<DiceTopReader> active = new List<DiceTopReader>();
    public int totalInt;
    public string totalDisplay;

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
    }

    public void RecomputeTotals()
    {
        int sum = 0;
        List<string> parts = new List<string>();
        foreach (var d in active)
        {
            if (!d || !d.isSettled) continue;
            parts.Add(d.resultValue);
            int v;
            if (int.TryParse(d.resultValue, out v)) sum += v;
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
