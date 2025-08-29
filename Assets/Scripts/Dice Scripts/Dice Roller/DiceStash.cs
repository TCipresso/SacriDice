using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiceStash : MonoBehaviour
{
    public static DiceStash Instance { get; private set; }

    [Header("UI")]
    public GameObject uiImagePrefab;

    [Header("Generated (per round)")]
    public GameObject GenDice;
    public List<GameObject> CurrGenDiceList = new();

    [Header("Purchased (persists across rounds)")]
    public List<GameObject> BoughtDiceList = new();

    [Header("Current Stash (this round = Gen + Bought)")]
    public List<GameObject> CurrStash = new();

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

    public void RebuildCurrStash()
    {
        CurrStash.Clear();
        CurrStash.AddRange(CurrGenDiceList);
        CurrStash.AddRange(BoughtDiceList);
    }

    // ---------- UI ----------
    public void BuildUI(Transform parent)
    {
        if (!parent) return;

        int committedTotal = (SacrificeManager2.Instance != null)
            ? SacrificeManager2.Instance.totalDiceCommitted
            : 0;

        int have = CurrGenDiceList.Count;
        int need = committedTotal - have;

        if (need > 0)
        {
            AddGenDice(need);
        }
        else if (need < 0)
        {
            int remove = -need;
            for (int i = 0; i < remove && CurrGenDiceList.Count > 0; i++)
                CurrGenDiceList.RemoveAt(CurrGenDiceList.Count - 1);
        }

        RebuildCurrStash();

        for (int i = parent.childCount - 1; i >= 0; i--)
            Object.Destroy(parent.GetChild(i).gameObject);

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
