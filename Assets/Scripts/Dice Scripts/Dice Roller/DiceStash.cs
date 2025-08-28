using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiceStash : MonoBehaviour
{
    public static DiceStash Instance { get; private set; }

    [Header("UI")]
    public GameObject uiImagePrefab;

    [Header("Generated (per round)")]
    public GameObject GenDice;                       // prefab for general D6 earned via sacrifice
    public List<GameObject> CurrGenDiceList = new(); // per-round generated dice

    [Header("Purchased (persists across rounds)")]
    public List<GameObject> BoughtDiceList = new();  // all dice bought in shop

    [Header("Current Stash (this round = Gen + Bought)")]
    public List<GameObject> CurrStash = new();       // used for UI & spawning this round

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ---------- Generated Dice (per round) ----------
    public void AddGenDice(int count)
    {
        if (!GenDice || count <= 0) return;
        for (int i = 0; i < count; i++) CurrGenDiceList.Add(GenDice);
    }

    public void ResetGenDiceList() => CurrGenDiceList.Clear();

    // ---------- Purchased Dice (persistent) ----------
    public void AddBoughtDie(GameObject prefab)
    {
        if (!prefab) return;
        BoughtDiceList.Add(prefab);
    }

    // ---------- Current Stash Management ----------
    public void ResetCurrStash() => CurrStash.Clear();

    /// <summary>
    /// Rebuilds CurrStash from CurrGenDiceList + BoughtDiceList (in that order).
    /// </summary>
    public void RebuildCurrStash()
    {
        CurrStash.Clear();
        CurrStash.AddRange(CurrGenDiceList);
        CurrStash.AddRange(BoughtDiceList);
    }

    // ---------- UI ----------
    /// <summary>
    /// Builds roll UI from CurrStash (effective dice this round).
    /// </summary>
    public void BuildUI(Transform parent)
    {
        if (!parent) return;

        for (int i = parent.childCount - 1; i >= 0; i--)
            Destroy(parent.GetChild(i).gameObject);

        foreach (var d in CurrStash)
        {
            if (!d) continue;
            var db = d.GetComponent<DiceBase>();
            if (db == null || db.uiPromptSprite == null) continue;

            var icon = Object.Instantiate(uiImagePrefab, parent);
            var img = icon.GetComponent<Image>();
            if (img) img.sprite = db.uiPromptSprite;
        }
    }
}
