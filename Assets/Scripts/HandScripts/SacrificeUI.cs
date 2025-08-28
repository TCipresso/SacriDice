using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SacrificeUI : MonoBehaviour
{
    public GameObject root;

    public Button leftThumb;
    public Button leftIndex;
    public Button leftMiddle;
    public Button leftRing;
    public Button leftPinky;
    public Button leftHand;

    public Button rightThumb;
    public Button rightIndex;
    public Button rightMiddle;
    public Button rightRing;
    public Button rightPinky;
    public Button rightHand;

    public Button commitButton;
    public Button cancelButton;
    public TextMeshProUGUI bonusTMP;

    public bool hideStaked = true;

    void OnEnable()
    {
        RefreshAll();
    }

    // no OnDisable — per your request

    public void Open()
    {
        root.SetActive(true);
        // keep this if you want a fresh selection each time the panel opens (does NOT clear staked/permanent)
        SacrificeSystem.Instance.BeginSacrificeRound();
        RefreshAll();
    }

    public void Close()
    {
        root.SetActive(false);
    }

    // ----- Clicks (manual refresh after each toggle) -----
    public void Click_LeftThumb() { SacrificeSystem.Instance.ToggleFinger(HandSide.Left, Finger.Thumb); RefreshAll(); }
    public void Click_LeftIndex() { SacrificeSystem.Instance.ToggleFinger(HandSide.Left, Finger.Index); RefreshAll(); }
    public void Click_LeftMiddle() { SacrificeSystem.Instance.ToggleFinger(HandSide.Left, Finger.Middle); RefreshAll(); }
    public void Click_LeftRing() { SacrificeSystem.Instance.ToggleFinger(HandSide.Left, Finger.Ring); RefreshAll(); }
    public void Click_LeftPinky() { SacrificeSystem.Instance.ToggleFinger(HandSide.Left, Finger.Pinky); RefreshAll(); }
    public void Click_LeftHand() { SacrificeSystem.Instance.ToggleHand(HandSide.Left); RefreshAll(); }

    public void Click_RightThumb() { SacrificeSystem.Instance.ToggleFinger(HandSide.Right, Finger.Thumb); RefreshAll(); }
    public void Click_RightIndex() { SacrificeSystem.Instance.ToggleFinger(HandSide.Right, Finger.Index); RefreshAll(); }
    public void Click_RightMiddle() { SacrificeSystem.Instance.ToggleFinger(HandSide.Right, Finger.Middle); RefreshAll(); }
    public void Click_RightRing() { SacrificeSystem.Instance.ToggleFinger(HandSide.Right, Finger.Ring); RefreshAll(); }
    public void Click_RightPinky() { SacrificeSystem.Instance.ToggleFinger(HandSide.Right, Finger.Pinky); RefreshAll(); }
    public void Click_RightHand() { SacrificeSystem.Instance.ToggleHand(HandSide.Right); RefreshAll(); }

    public void Click_Commit()
    {
        bool hadSelection = SacrificeSystem.Instance.IsAnySelectionActive();
        SacrificeSystem.Instance.CommitSacrifice();
        if (hadSelection)
        {
            DiceStash.Instance.AddGenDice(SacrificeSystem.Instance.totalBonusDiceThisRound);
            DiceStash.Instance.RebuildCurrStash();
            Close();
        }
        else
        {
            RefreshAll();
        }
    }

    public void Click_Cancel()
    {
        SacrificeSystem.Instance.CancelSelection();
        RefreshAll();
    }

    void RefreshAll()
    {
        if (!root || !root.activeSelf) return;
        var s = SacrificeSystem.Instance;

        SetFinger(leftThumb, s.left.thumbAvailable, s.left.thumbSelected, s.left.thumbStaked, s.left.handSelected, s.left.handStaked);
        SetFinger(leftIndex, s.left.indexAvailable, s.left.indexSelected, s.left.indexStaked, s.left.handSelected, s.left.handStaked);
        SetFinger(leftMiddle, s.left.middleAvailable, s.left.middleSelected, s.left.middleStaked, s.left.handSelected, s.left.handStaked);
        SetFinger(leftRing, s.left.ringAvailable, s.left.ringSelected, s.left.ringStaked, s.left.handSelected, s.left.handStaked);
        SetFinger(leftPinky, s.left.pinkyAvailable, s.left.pinkySelected, s.left.pinkyStaked, s.left.handSelected, s.left.handStaked);
        SetHand(leftHand, s.left.handAvailable, s.left.handSelected, s.left.handStaked);

        SetFinger(rightThumb, s.right.thumbAvailable, s.right.thumbSelected, s.right.thumbStaked, s.right.handSelected, s.right.handStaked);
        SetFinger(rightIndex, s.right.indexAvailable, s.right.indexSelected, s.right.indexStaked, s.right.handSelected, s.right.handStaked);
        SetFinger(rightMiddle, s.right.middleAvailable, s.right.middleSelected, s.right.middleStaked, s.right.handSelected, s.right.handStaked);
        SetFinger(rightRing, s.right.ringAvailable, s.right.ringSelected, s.right.ringStaked, s.right.handSelected, s.right.handStaked);
        SetFinger(rightPinky, s.right.pinkyAvailable, s.right.pinkySelected, s.right.pinkyStaked, s.right.handSelected, s.right.handStaked);
        SetHand(rightHand, s.right.handAvailable, s.right.handSelected, s.right.handStaked);

        if (bonusTMP) bonusTMP.text = "Bonus Dice: " + s.GetProjectedBonus().ToString();

        if (commitButton) commitButton.interactable = true;
        if (cancelButton) cancelButton.interactable = true;
    }

    void SetFinger(Button b, bool available, bool selected, bool staked, bool handSelected, bool handStaked)
    {
        if (!b) return;
        if (hideStaked) b.gameObject.SetActive(available && !staked);
        b.interactable = available && !staked;
    }

    void SetHand(Button b, bool available, bool selected, bool staked)
    {
        if (!b) return;
        if (hideStaked) b.gameObject.SetActive(available && !staked);
        b.interactable = available && !staked;
    }
}
