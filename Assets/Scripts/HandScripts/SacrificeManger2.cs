using System.Collections.Generic;
using UnityEngine;

public enum LimbSide { Left, Right }

public class SacrificeManager2 : MonoBehaviour
{
    public static SacrificeManager2 Instance { get; private set; }

    [Header("Parents")]
    public GameObject leftHandParent;
    public GameObject rightHandParent;

    [Header("Runtime Lists")]
    public List<GameObject> LeftHand = new List<GameObject>();
    public List<GameObject> RightHand = new List<GameObject>();
    public List<GameObject> SelectedSac = new List<GameObject>();
    public List<GameObject> CommittedSac = new List<GameObject>(); // committed (this loop)
    public List<GameObject> PermaSac = new List<GameObject>(); // permanently lost (across loops)

    [Header("Last Commit Totals")]
    public int lastDiceValue = 0;
    public int lastCoinsValue = 0;
    public int lastHealthValue = 0;

    [Header("Cumulative Totals (per round)")]
    public int totalDiceCommitted = 0;     // DiceStash.BuildUI reads this
    public int totalCoinsCommitted = 0;
    public int totalHealthCommitted = 0;

    private readonly Dictionary<GameObject, LimbSide> homeSide = new Dictionary<GameObject, LimbSide>();
    private readonly HashSet<GameObject> _committedSet = new HashSet<GameObject>();
    private readonly HashSet<GameObject> _permaSet = new HashSet<GameObject>();

    private const string HAND_TAG = "Hand";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // Sync backing sets with any serialized lists (e.g., from saves)
        _committedSet.Clear(); foreach (var go in CommittedSac) if (go) _committedSet.Add(go);
        _permaSet.Clear(); foreach (var go in PermaSac) if (go) _permaSet.Add(go);

        RebuildLists();
        EnforceCommittedDisabled(); // also enforces PermaSac (see method body)
    }

    [ContextMenu("Rebuild Lists")]
    public void RebuildLists()
    {
        LeftHand.Clear();
        RightHand.Clear();
        homeSide.Clear();

        if (leftHandParent)
        {
            var t = leftHandParent.transform;
            for (int i = 0; i < t.childCount; i++)
            {
                var go = t.GetChild(i).gameObject;
                homeSide[go] = LimbSide.Left;

                // If permanently lost, force disable and skip
                if (_permaSet.Contains(go) || PermaSac.Contains(go))
                {
                    if (go.activeSelf) go.SetActive(false);
                    continue;
                }

                // If committed (this loop), force disable and skip
                if (_committedSet.Contains(go) || CommittedSac.Contains(go))
                {
                    if (go.activeSelf) go.SetActive(false);
                    continue;
                }

                if (!go.activeSelf) continue;
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

                if (_permaSet.Contains(go) || PermaSac.Contains(go))
                {
                    if (go.activeSelf) go.SetActive(false);
                    continue;
                }

                if (_committedSet.Contains(go) || CommittedSac.Contains(go))
                {
                    if (go.activeSelf) go.SetActive(false);
                    continue;
                }

                if (!go.activeSelf) continue;
                if (!SelectedSac.Contains(go)) RightHand.Add(go);
            }
        }
    }

    public void OnItemClicked(GameObject item)
    {
        if (!item) return;
        if (!item.activeInHierarchy) return;           // disabled = ignore
        if (_permaSet.Contains(item)) return;          // permanently lost = ignore
        if (_committedSet.Contains(item)) return;      // committed (this loop) = ignore

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

    private void RemoveFromAll(GameObject item)
    {
        LeftHand.Remove(item);
        RightHand.Remove(item);
        SelectedSac.Remove(item);
        // do NOT remove from CommittedSac / PermaSac / their sets
    }

    // ---------- PRICED COMMIT (uses LimbStats on each item) ----------
    public void CommitSelected()
    {
        lastDiceValue = lastCoinsValue = lastHealthValue = 0;
        if (SelectedSac.Count == 0) return;

        // Detect per-side "hand picked" and collect finger items
        bool leftHandPicked = false, rightHandPicked = false;
        var leftFingers = new List<GameObject>();
        var rightFingers = new List<GameObject>();
        GameObject leftHandObj = null, rightHandObj = null;

        foreach (var go in SelectedSac)
        {
            if (!go) continue;

            // find side: mapping first, fallback by parent
            LimbSide side;
            if (!homeSide.TryGetValue(go, out side))
            {
                var p = go.transform.parent;
                if (leftHandParent && p == leftHandParent.transform) side = LimbSide.Left;
                else if (rightHandParent && p == rightHandParent.transform) side = LimbSide.Right;
                else continue;
            }

            bool isHand = go.CompareTag(HAND_TAG);
            if (isHand)
            {
                if (side == LimbSide.Left) { leftHandPicked = true; leftHandObj = go; }
                else { rightHandPicked = true; rightHandObj = go; }
            }
            else
            {
                if (side == LimbSide.Left) leftFingers.Add(go);
                else rightFingers.Add(go);
            }
        }

        // Price left side
        if (leftHandPicked)
        {
            var stats = GetStats(leftHandObj);
            lastDiceValue += stats?.Dice() ?? 0;
            lastCoinsValue += stats?.Coins() ?? 0;
            lastHealthValue += stats?.Health() ?? 0;
        }
        else
        {
            foreach (var f in leftFingers)
            {
                var s = GetStats(f);
                lastDiceValue += s?.Dice() ?? 0;
                lastCoinsValue += s?.Coins() ?? 0;
                lastHealthValue += s?.Health() ?? 0;
            }
        }

        // Price right side
        if (rightHandPicked)
        {
            var stats = GetStats(rightHandObj);
            lastDiceValue += stats?.Dice() ?? 0;
            lastCoinsValue += stats?.Coins() ?? 0;
            lastHealthValue += stats?.Health() ?? 0;
        }
        else
        {
            foreach (var f in rightFingers)
            {
                var s = GetStats(f);
                lastDiceValue += s?.Dice() ?? 0;
                lastCoinsValue += s?.Coins() ?? 0;
                lastHealthValue += s?.Health() ?? 0;
            }
        }

        totalDiceCommitted += lastDiceValue;
        totalCoinsCommitted += lastCoinsValue;
        totalHealthCommitted += lastHealthValue;

        // Build commit set (hands pull entire side)
        var toCommit = new HashSet<GameObject>(SelectedSac);

        if (leftHandPicked && leftHandParent)
        {
            var t = leftHandParent.transform;
            for (int i = 0; i < t.childCount; i++)
            {
                var child = t.GetChild(i).gameObject;
                if (child && child.activeSelf) toCommit.Add(child);
            }
        }
        if (rightHandPicked && rightHandParent)
        {
            var t = rightHandParent.transform;
            for (int i = 0; i < t.childCount; i++)
            {
                var child = t.GetChild(i).gameObject;
                if (child && child.activeSelf) toCommit.Add(child);
            }
        }

        // Move and disable
        SelectedSac.Clear();

        foreach (var item in toCommit)
        {
            if (!item) continue;

            LeftHand.Remove(item);
            RightHand.Remove(item);

            if (_committedSet.Add(item))
                if (!CommittedSac.Contains(item)) CommittedSac.Add(item);

            if (item.activeSelf) item.SetActive(false);   // committed = disabled GameObject
        }

        // NEW  add this to grow the pot from this commit's coins
        if (PotManager.Instance != null)
            PotManager.Instance.AddFromLastCommit();

        EnforceCommittedDisabled(); // also covers PermaSac items
        RebuildLists();
    }

    private LimbStats GetStats(GameObject go)
    {
        if (!go) return null;
        return go.GetComponent<LimbStats>() ?? go.GetComponentInChildren<LimbStats>(true);
    }

    [ContextMenu("Enforce Committed Disabled")]
    public void EnforceCommittedDisabled()
    {
        // Disable every committed item and ensure it’s not in any selectable list
        foreach (var item in CommittedSac)
        {
            if (!item) continue;
            if (item.activeSelf) item.SetActive(false);
            LeftHand.Remove(item);
            RightHand.Remove(item);
            SelectedSac.Remove(item);
            _committedSet.Add(item);
        }

        // Also enforce permanent-loss items
        foreach (var item in PermaSac)
        {
            if (!item) continue;
            if (item.activeSelf) item.SetActive(false);
            LeftHand.Remove(item);
            RightHand.Remove(item);
            SelectedSac.Remove(item);
            _permaSet.Add(item);
        }
    }

    // Call at the start of EACH round so last round's dice don't carry over
    public void ResetRoundCommitTotals()
    {
        lastDiceValue = 0;
        lastCoinsValue = 0;
        lastHealthValue = 0;
        totalDiceCommitted = 0;
    }

    public void RestoreAllCommittedToHome()
    {
        // Re-enable and return to original hand lists
        foreach (var item in CommittedSac)
        {
            if (!item) continue;

            item.SetActive(true);

            // Put back where it belongs
            if (homeSide.TryGetValue(item, out var side))
            {
                if (side == LimbSide.Left)
                {
                    if (!LeftHand.Contains(item)) LeftHand.Add(item);
                    RightHand.Remove(item);
                }
                else
                {
                    if (!RightHand.Contains(item)) RightHand.Add(item);
                    LeftHand.Remove(item);
                }
            }
            else
            {
                // Fallback by parent if mapping missing
                var p = item.transform.parent;
                if (leftHandParent && p == leftHandParent.transform)
                {
                    if (!LeftHand.Contains(item)) LeftHand.Add(item);
                    RightHand.Remove(item);
                }
                else if (rightHandParent && p == rightHandParent.transform)
                {
                    if (!RightHand.Contains(item)) RightHand.Add(item);
                    LeftHand.Remove(item);
                }
            }
        }

        // Clear committed + selection and per-round totals
        CommittedSac.Clear();
        _committedSet.Clear();
        SelectedSac.Clear();
        ResetRoundCommitTotals();

        // Refresh lists to reflect the now-active items
        RebuildLists();
    }

    // >>> EXACT NAME YOU ASKED FOR <<<
    public void BankCommittedToPermanentLoss()
    {
        if (CommittedSac.Count == 0) return;

        foreach (var item in CommittedSac)
        {
            if (!item) continue;

            // ensure disabled
            if (item.activeSelf) item.SetActive(false);

            // add to permanent-loss bank
            if (!_permaSet.Contains(item))
            {
                _permaSet.Add(item);
                if (!PermaSac.Contains(item)) PermaSac.Add(item);
            }

            // make sure not in selectable lists
            LeftHand.Remove(item);
            RightHand.Remove(item);
            SelectedSac.Remove(item);
        }

        // clear "this-loop" committed
        CommittedSac.Clear();
        _committedSet.Clear();

        // refresh lists/enforcement
        EnforceCommittedDisabled();
        RebuildLists();
    }
}
