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

    void OnEnable()
    {
        if (SacrificeSystem.Instance == null) return;
        SacrificeSystem.Instance.OnRoundStart += RefreshAll;
        SacrificeSystem.Instance.OnSelectionChanged += RefreshAll;
        SacrificeSystem.Instance.OnSacrificeCommitted += OnCommitted;
        SacrificeSystem.Instance.OnRoundCanceled += RefreshAll;
        SacrificeSystem.Instance.OnCollateralResolved += HandleCollateralResolved;
    }

    void OnDisable()
    {
        if (SacrificeSystem.Instance == null) return;
        SacrificeSystem.Instance.OnRoundStart -= RefreshAll;
        SacrificeSystem.Instance.OnSelectionChanged -= RefreshAll;
        SacrificeSystem.Instance.OnSacrificeCommitted -= OnCommitted;
        SacrificeSystem.Instance.OnRoundCanceled -= RefreshAll;
        SacrificeSystem.Instance.OnCollateralResolved -= HandleCollateralResolved;
    }

    public void Open()
    {
        root.SetActive(true);
        SacrificeSystem.Instance.BeginSacrificeRound();
        RefreshAll();
    }

    public void Close()
    {
        root.SetActive(false);
    }

    void OnCommitted(int bonus)
    {
        RefreshAll();
    }

    void HandleCollateralResolved(bool win)
    {
        RefreshAll();
    }

    public void Click_LeftThumb() { SacrificeSystem.Instance.ToggleFinger(HandSide.Left, Finger.Thumb); }
    public void Click_LeftIndex() { SacrificeSystem.Instance.ToggleFinger(HandSide.Left, Finger.Index); }
    public void Click_LeftMiddle() { SacrificeSystem.Instance.ToggleFinger(HandSide.Left, Finger.Middle); }
    public void Click_LeftRing() { SacrificeSystem.Instance.ToggleFinger(HandSide.Left, Finger.Ring); }
    public void Click_LeftPinky() { SacrificeSystem.Instance.ToggleFinger(HandSide.Left, Finger.Pinky); }
    public void Click_LeftHand() { SacrificeSystem.Instance.ToggleHand(HandSide.Left); }

    public void Click_RightThumb() { SacrificeSystem.Instance.ToggleFinger(HandSide.Right, Finger.Thumb); }
    public void Click_RightIndex() { SacrificeSystem.Instance.ToggleFinger(HandSide.Right, Finger.Index); }
    public void Click_RightMiddle() { SacrificeSystem.Instance.ToggleFinger(HandSide.Right, Finger.Middle); }
    public void Click_RightRing() { SacrificeSystem.Instance.ToggleFinger(HandSide.Right, Finger.Ring); }
    public void Click_RightPinky() { SacrificeSystem.Instance.ToggleFinger(HandSide.Right, Finger.Pinky); }
    public void Click_RightHand() { SacrificeSystem.Instance.ToggleHand(HandSide.Right); }

    public void Click_Commit()
    {
        SacrificeSystem.Instance.CommitSacrifice();
        DiceStash.Instance.AddGenDice(SacrificeSystem.Instance.totalBonusDiceThisRound);
        DiceStash.Instance.RebuildCurrStash();
        Close();
    }

    public void Click_Cancel()
    {
        SacrificeSystem.Instance.CancelSelection();
        RefreshAll();
    }

    void RefreshAll()
    {
        if (!root.activeSelf) return;
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
        if (commitButton) commitButton.interactable = !s.roundLocked && s.IsAnySelectionActive();
        if (cancelButton) cancelButton.interactable = !s.roundLocked && s.IsAnySelectionActive();
    }

    void SetFinger(Button b, bool available, bool selected, bool staked, bool handSelected, bool handStaked)
    {
        if (!b) return;
        b.interactable = available && !staked && !handSelected && !handStaked && !SacrificeSystem.Instance.roundLocked;
    }

    void SetHand(Button b, bool available, bool selected, bool staked)
    {
        if (!b) return;
        b.interactable = available && !staked && !SacrificeSystem.Instance.roundLocked;
    }
}
