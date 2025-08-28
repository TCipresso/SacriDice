using System.Collections.Generic;
using UnityEngine;

public enum LimbSide { Left, Right }

public class SacrificeManager2 : MonoBehaviour
{
    public static SacrificeManager2 Instance { get; private set; }

    [Header("Parents")]
    public GameObject leftHandParent;
    public GameObject rightHandParent;
    const string HAND_TAG = "Hand";

    [Header("Runtime Lists")]
    public List<GameObject> LeftHand = new List<GameObject>();
    public List<GameObject> RightHand = new List<GameObject>();
    public List<GameObject> SelectedSac = new List<GameObject>();
    public List<GameObject> CommittedSac = new List<GameObject>(); // permanently disabled

    Dictionary<GameObject, LimbSide> homeSide = new Dictionary<GameObject, LimbSide>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        RebuildLists();
        EnforceCommittedDisabled();
    }

    [ContextMenu("Rebuild Lists")]
    public void RebuildLists()
    {
        LeftHand.Clear();
        RightHand.Clear();
        // Do NOT clear SelectedSac or CommittedSac here

        homeSide.Clear();

        if (leftHandParent)
        {
            var t = leftHandParent.transform;
            for (int i = 0; i < t.childCount; i++)
            {
                var go = t.GetChild(i).gameObject;
                homeSide[go] = LimbSide.Left;

                if (!go.activeSelf) continue;            // already disabled (committed)
                if (CommittedSac.Contains(go)) continue; // committed items are not listed

                // if it's currently selected, keep it in SelectedSac; otherwise list it
                if (!SelectedSac.Contains(go)) LeftHand.Add(go);
            }
        }

        if (rightHandParent)
        {
            var t = rightHandParent.transform;
            for (int i = 0; i < t.childCount; i++)
            {
                var go = t.GetChild(i).gameObject;
                homeSide[go] = LimbSide.Right;

                if (!go.activeSelf) continue;
                if (CommittedSac.Contains(go)) continue;

                if (!SelectedSac.Contains(go)) RightHand.Add(go);
            }
        }
    }

    public void OnItemClicked(GameObject item)
    {
        if (!item) return;
        if (!item.activeInHierarchy) return;   // disabled/committed: ignore
        if (CommittedSac.Contains(item)) return;

        if (SelectedSac.Contains(item)) Deselect(item);
        else Select(item);
    }

    public void Select(GameObject item)
    {
        RemoveFromAll(item);
        SelectedSac.Add(item);
    }

    public void Deselect(GameObject item)
    {
        RemoveFromAll(item);
        if (homeSide.TryGetValue(item, out var side))
        {
            if (side == LimbSide.Left) LeftHand.Add(item);
            else RightHand.Add(item);
        }
        else
        {
            LeftHand.Add(item);
        }
    }

    void RemoveFromAll(GameObject item)
    {
        LeftHand.Remove(item);
        RightHand.Remove(item);
        SelectedSac.Remove(item);
        // do NOT remove from CommittedSac here — those stay committed
    }

    public void CommitSelected()
    {
        if (SelectedSac.Count == 0) return;

        var toCommit = new HashSet<GameObject>(SelectedSac);

        bool leftHandSelected = false;
        bool rightHandSelected = false;

        foreach (var go in SelectedSac)
        {
            if (!go) continue;
            if (go.CompareTag(HAND_TAG))
            {
                if (homeSide.TryGetValue(go, out var side))
                {
                    if (side == LimbSide.Left) leftHandSelected = true;
                    if (side == LimbSide.Right) rightHandSelected = true;
                }
                else
                {
                    var p = go.transform.parent;
                    if (leftHandParent && p == leftHandParent.transform) leftHandSelected = true;
                    if (rightHandParent && p == rightHandParent.transform) rightHandSelected = true;
                }
            }
        }

        if (leftHandSelected && leftHandParent)
        {
            var t = leftHandParent.transform;
            for (int i = 0; i < t.childCount; i++)
            {
                var child = t.GetChild(i).gameObject;
                if (child && child.activeSelf) toCommit.Add(child);
            }
        }

        if (rightHandSelected && rightHandParent)
        {
            var t = rightHandParent.transform;
            for (int i = 0; i < t.childCount; i++)
            {
                var child = t.GetChild(i).gameObject;
                if (child && child.activeSelf) toCommit.Add(child);
            }
        }

        SelectedSac.Clear();

        foreach (var item in toCommit)
        {
            LeftHand.Remove(item);
            RightHand.Remove(item);
            if (!CommittedSac.Contains(item)) CommittedSac.Add(item);
            item.SetActive(false); // committed = disabled GameObject
        }
    }



    [ContextMenu("Enforce Committed Disabled")]
    public void EnforceCommittedDisabled()
    {
        // ensure all committed items are disabled (useful after scene load)
        foreach (var item in CommittedSac)
        {
            if (item && item.activeSelf) item.SetActive(false);
        }
    }
}
